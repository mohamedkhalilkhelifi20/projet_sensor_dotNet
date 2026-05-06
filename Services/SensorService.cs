using DashboardData.Data;
using DashboardData.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardData.Services
{
	public class SensorService : ISensorService
	{
		private readonly AppDbContext _context;
		public SensorService(AppDbContext context)
		{
			_context = context; 
		}

		public async Task<List<SensorData>> GetSensors()
		{
			return await _context.Sensors
				.Include(s => s.Location)
				.ToListAsync();
		}

		public async Task AddSensor(SensorData sensor)
		{
			_context.Sensors.Add(sensor);

			await _context.SaveChangesAsync();
		}

		public async Task<List<SensorData>> GetCriticalSensorsAsync(double theshold)
		{
			return await _context.Sensors
				.Include(s => s.Location)
				.Where(s => s.Value >  theshold)
				.OrderByDescending(s => s.Value)
				.ToListAsync();
		}

		public async Task<int> GetTotalCountAsync()
		{
			// CountAsync() est traduit en SQL : SELECT COUNT(*)
			return await _context.Sensors.CountAsync();
		}

		public async Task<double> GetAverageValueAsync()
		{
			// Si la base est vide, AverageAsync plante.
			if (!await _context.Sensors.AnyAsync()) return 0;

			// AverageAsync() => SQL : SELECT AVG(Value)
			// Le lambda s => s.Value sélectionne la colonne
			return await _context.Sensors.AverageAsync(s => s.Value);
		}

		public async Task<double> GetMaxValueAsync()
		{
			// Même sécurité pour éviter l’exception sur table vide
			if (!await _context.Sensors.AnyAsync()) return 0;

			// MaxAsync() => SQL : SELECT MAX(Value)
			return await _context.Sensors.MaxAsync(s => s.Value);
		}

		public async Task<List<Location>> GetLocationsAsync()
		{
			return await _context.Locations.ToListAsync();
		}

		public async Task<SensorData?> GetSensorByIdAsync(int id)
		{
			// FindAsync cherche directement par la Clé Primaire (Id)
			return await _context.Sensors.FindAsync(id);
		}

		public async Task AddSensorAsync(SensorData sensor)
		{
			sensor.LastUpdate = DateTime.Now;

			// Historisation de la valeur initiale (TP5)
			sensor.Values.Add(new SensorValueHistory
			{
				Value = sensor.Value,
				Timestamp = DateTime.Now
			});

			_context.Sensors.Add(sensor);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateSensorAsync(SensorData sensor)
		{
			sensor.LastUpdate = DateTime.Now; // Mise à jour de la date

			// Ajout à l'historique lors d'une modification (TP5)
			sensor.Values.Add(new SensorValueHistory
			{
				Value = sensor.Value,
				Timestamp = DateTime.Now
			});

			_context.Sensors.Update(sensor);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteSensorAsync(int id)
		{
			var sensor = await _context.Sensors.FindAsync(id);
			if (sensor != null)
			{
				_context.Sensors.Remove(sensor);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<List<LocationStat>> GetAverageValueByLocationAsync()
		{
			// EF Core traduit ceci en : SELECT Location, AVG(Value) FROM Sensors GROUP BY Location
			return await _context.Sensors
				.Include(s => s.Location)
				.GroupBy(s => s.Location.Name)
				.Select(g => new LocationStat
				{
					LocationName = g.Key ?? "Inconnu",
					AverageValue = g.Average(s => s.Value)
				})
				.ToListAsync();
		}

		public async Task<List<LocationCountStat>> GetSensorCountByLocationAsync()
		{
			return await _context.Sensors
				.Include(s => s.Location)
				.GroupBy(s => s.Location.Name)
				.Select(g => new LocationCountStat
				{
					LocationName = g.Key ?? "Inconnu",
					Count = g.Count()
				})
				.ToListAsync();
		}

		public async Task<List<LocationCountStat>> GetCountStatsAsync()
		{
			// Réutilise la même logique que GetSensorCountByLocationAsync
			return await GetSensorCountByLocationAsync();
		}
	}

	
}
