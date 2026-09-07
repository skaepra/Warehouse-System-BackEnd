namespace Online_Store_Backend.Services
{
    public interface IWeatherForecastServices
    {
        IEnumerable<WeatherForecast> GetForecast();
    }
}
