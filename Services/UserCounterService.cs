namespace DashboardData.Services
{
	public class UserCounterService
	{
		public int Count { get; set; }

		public void Increment()
		{
			Count++;
		}
	}
}

