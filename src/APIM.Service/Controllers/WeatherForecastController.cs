using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace APIM.Service.Controllers;

[ApiController]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("WeatherForecast")]
    public IEnumerable<WeatherForecast> WeatherForecast()
    {

        Response.Headers.Add("APIM-Source-System", "WeatherForecastController");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet]
    [Route("WeatherForecastCity")]
    public JsonResult WeatherForecastCity(string city){

        Response.Headers.Add("APIM-Source-System", "WeatherForecastController");

        var result = new object[2];
        result[0] = new { city = city, region = "Europe" };
        result[1] = new { city = "Other City", region = "Other Region" };

        return new JsonResult(result) ;
    }
}
