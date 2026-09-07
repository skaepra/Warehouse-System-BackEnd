namespace Online_Store_Backend.Services
{
    public class WeatherForecastServices : IWeatherForecastServices
    {
        private readonly ILogger<WeatherForecastServices> _logger;

        public WeatherForecastServices(ILogger<WeatherForecastServices> logger)
        {
            _logger = logger;
        }

        private static readonly string[] Summaries = new[]
      {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
      };
       
        public IEnumerable<WeatherForecast> GetForecast()
        {
            _logger.LogInformation("Getting forecast data");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
