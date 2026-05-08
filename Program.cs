using DashboardData.Components;
using DashboardData.Data;
using DashboardData.Models;
using DashboardData.Services;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ISensorService, SensorService>();


//builder.Services.AddScoped<UserCounterService>();

builder.Services.AddTransient<UserCounterService>();

builder.Services.AddRadzenComponents();


//builder.Services.AddSingleton<UserCounterService>();

var connectionString  = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(
options=>options.UseSqlite(connectionString)
);

// Identity configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = false;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Seed Identity roles and admin user
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

	if (!await roleManager.RoleExistsAsync("Admin"))
		await roleManager.CreateAsync(new IdentityRole("Admin"));

	if (await userManager.FindByEmailAsync("admin@data.com") == null)
	{
		var adminUser = new IdentityUser { UserName = "admin@data.com", Email = "admin@data.com" };
		var result = await userManager.CreateAsync(adminUser, "Admin123!");

		if (result.Succeeded)
			await userManager.AddToRoleAsync(adminUser, "Admin");
	}
}

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var context = services.GetRequiredService<AppDbContext>();

	if (!context.Sensors.Any())
	{
		Console.WriteLine("------ Géneration de données de test ------");

		//1. créer les emplacements
		var labo = new Location { Name = "Labo", Building = "Bat. A"};
		var usine = new Location { Name = "Usine", Building = "Bat. B" };
		context.Locations.AddRange(labo, usine);

		//2. creer les tags
		var tagCritique = new Tag { Label = "Critique" };
		var tagMaintenance = new Tag { Label = "Maintenance" };
		context.Tags.AddRange(tagCritique, tagMaintenance);
		context.SaveChanges();

		//3. créer les capteurs avec relations
		var soned1 = new SensorData
		{
			Name = "Soned_Alpha", Value = 25.4,
			LocationId = labo.Id,
			Tags = new List<Tag>{tagCritique}

		};

		var soned2 = new SensorData
		{
			Name = "Soned_Beta",
			Value = 40.2,
			LocationId = labo.Id,
			Tags = new List<Tag> { tagCritique, tagMaintenance }

		};

		context.Sensors.AddRange(soned1,soned2);
		context.SaveChanges();
	}
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Enable authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

// --- AUTHENTICATION ENDPOINTS (Outside WebSocket) ---
app.MapPost("/api/auth/login", async (
	[FromServices] SignInManager<IdentityUser> signInManager,
	[FromForm] string email,
	[FromForm] string password) =>
{
	var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);

	if (result.Succeeded) return Results.Redirect("/dashboard");

	return Results.Redirect("/login?error=Invalid+credentials");
}).DisableAntiforgery();

app.MapPost("/api/auth/logout", async ([FromServices] SignInManager<IdentityUser> signInManager) =>
{
	await signInManager.SignOutAsync();
	return Results.Redirect("/");
}).DisableAntiforgery();

app.Run();
