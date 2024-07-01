using HNGTASK.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace HNGTASK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openWeatherApiKey;

        public HelloController(IOptions<OpenWeatherOption> openWeatherOptions, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _openWeatherApiKey = openWeatherOptions.Value.ApiKey;
        }

        [HttpGet]
        public async Task<DataResponse> Get([FromQuery] string visitor_name)
        {
            DataResponse response = new DataResponse();
            try
            {
                var temperature = "Unknown";
                var location = "unknown";
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (clientIp == "::1")
                {
                    clientIp = "8.8.8.8";
                }
                else if (string.IsNullOrEmpty(clientIp))
                {
                    clientIp = "8.8.8.8"; 
                }

                using (var httClient = _httpClientFactory.CreateClient())
                {
                    var query = $"http://ip-api.com/json/{clientIp}";
                    var locationResponse = await httClient.GetStringAsync(query);
                    var locationData = JsonDocument.Parse(locationResponse).RootElement;

                    if (locationData.TryGetProperty("lat", out var latElement) && locationData.TryGetProperty("lon", out var lonElement))
                    {
                        var lat = latElement.GetDouble();
                        var lon = lonElement.GetDouble();
                        location = locationData.GetProperty("city").GetString() ?? "Unknown";

                        var weatherUrl = $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&appid={_openWeatherApiKey}";
                        var weatherResponse = await httClient.GetStringAsync(weatherUrl);
                        var weatherData = JObject.Parse(weatherResponse);
                        temperature = weatherData["main"]["temp"].ToString();
                    }

                }


                response.client_ip = clientIp;
                response.location = location;
                response.greeting = $"Hello, {visitor_name}!, the temperature is {temperature} degrees Celcius in {response.location}";
            }
            catch (Exception)
            {


            }
            return response;
        }
    }
}
