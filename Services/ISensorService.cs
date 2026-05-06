using DashboardData.Models;

namespace DashboardData.Services
{
	public interface ISensorService
	{
		Task<List<SensorData>> GetSensors();
		Task AddSensor(SensorData sensor);

		Task<List<SensorData>> GetCriticalSensorsAsync(double threshold);

		// Retourne le nombre total de capteurs en base
		Task<int> GetTotalCountAsync();

		// Retourne la valeur moyenne de tous les capteurs
		Task<double> GetAverageValueAsync();

		// Retourne la valeur maximale parmi tous les capteurs
		Task<double> GetMaxValueAsync();

		Task<List<Location>> GetLocationsAsync();
		Task<SensorData?> GetSensorByIdAsync(int id);
		Task AddSensorAsync(SensorData sensor);
		Task UpdateSensorAsync(SensorData sensor);
		Task DeleteSensorAsync(int id);

		Task<List<LocationStat>> GetAverageValueByLocationAsync();
		Task<List<LocationCountStat>> GetSensorCountByLocationAsync();
		Task<List<LocationCountStat>> GetCountStatsAsync();

        Task<List<SensorData>> SearchSensorsAsync(string? locationName, string? searchText, bool showCriticalOnly = false);
	}
}
