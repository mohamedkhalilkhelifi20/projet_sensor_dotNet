using System.ComponentModel.DataAnnotations;
namespace DashboardData.Models;

public class SensorData
{
    [Key]
    public int Id{ get; set;}

	// Nom du capteur : obligatoire, entre 3 et 50 caractères
	[Required(ErrorMessage = "Le nom est obligatoire.")]
	[StringLength(50, MinimumLength = 3, ErrorMessage = "Le nom doit faire entre 3 et 50 caractères.")]
	public string Name{get; set;}

	public string Type{get; set;} = "Temperature";

	// Valeur mesurée : doit être dans une plage logique (-50 à 150)
	[Range(-50.0, 150.0, ErrorMessage = "La valeur doit être entre -50 et 150.")]
	public double Value{get; set;}

    public DateTime LastUpdate{get; set;} = DateTime.Now;

	//Relations
	// Identifiant du lieu : doit être > 0 pour forcer un vrai choix (pas la valeur "0" par défaut)
	[Range(1, int.MaxValue,
		ErrorMessage = "Veuillez sélectionner un lieu valide.")]
	public int LocationId{get; set;}
    public Location Location{get; set;}

    public ICollection<Tag> Tags {get; set;} = new List<Tag>();
	public ICollection<SensorValueHistory> Values { get; set; } = new List<SensorValueHistory>();
}

public class LocationStat
{
	public string LocationName { get; set; }
	public double AverageValue { get; set; }
}

