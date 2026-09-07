using Microsoft.AspNetCore.Mvc;
using Online_Store_Backend.Services;

namespace Online_Store_Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherForecastServices _service;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastServices service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet()]
    [Route("GetData")]
    public IEnumerable<WeatherForecast> Get()
    {
        return _service.GetForecast();
    }
}
