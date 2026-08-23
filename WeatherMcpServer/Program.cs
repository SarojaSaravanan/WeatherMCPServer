using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WeatherTools>();

var app = builder.Build();
await app.RunAsync();

[McpServerToolType]
public sealed class WeatherTools
{
    [McpServerTool, Description("Gets the current weather (temperature and conditions) for a given city")]
    public async Task<string> GetCurrentWeather(
        [Description("The name of the city, e.g. Chennai")] string city)
    {
        using var httpClient = new HttpClient();

        var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
        var geoResponse = await httpClient.GetStringAsync(geoUrl);
        using var geoDoc = JsonDocument.Parse(geoResponse);

        if (!geoDoc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
        {
            return $"Could not find location data for '{city}'.";
        }

        var location = results[0];
        var latitude = location.GetProperty("latitude").GetDouble();
        var longitude = location.GetProperty("longitude").GetDouble();
        var resolvedName = location.GetProperty("name").GetString();

        var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";
        var weatherResponse = await httpClient.GetStringAsync(weatherUrl);
        using var weatherDoc = JsonDocument.Parse(weatherResponse);

        var current = weatherDoc.RootElement.GetProperty("current_weather");
        var temp = current.GetProperty("temperature").GetDouble();
        var windSpeed = current.GetProperty("windspeed").GetDouble();

        return $"Current weather in {resolvedName}: {temp}°C, wind speed {windSpeed} km/h.";
    }
}