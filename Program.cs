using DashboardData.Components;
using DashboardData.Data;
using DashboardData.Models;
using DashboardData.Services;
using Microsoft.EntityFrameworkCore;
using Radzen;

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

var app = builder.Build();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
