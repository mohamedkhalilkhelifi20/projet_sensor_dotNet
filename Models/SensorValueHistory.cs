using System.ComponentModel.DataAnnotations;

namespace DashboardData.Models
{
	public class SensorValueHistory
	{
		[Key]
		public int Id { get; set; }

		public double Value { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.Now;

		// Clé étrangère
		public int SensorDataId { get; set; }

		// Propriété de navigation
		public SensorData SensorData { get; set; }
	}
}


