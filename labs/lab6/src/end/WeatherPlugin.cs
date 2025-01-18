using Microsoft.SemanticKernel;
using System.ComponentModel;


namespace AgentsSample
{
    /// <summary>
    /// WeatherPlugin class
    /// This class provides a method to get the weather of a city
    /// The method is exposed as a kernel function
    /// </summary>
    public class WeatherPlugin
    {
        [KernelFunction("getCurrentCityWeather")]
        [Description("Provide current City weather using the city name as key search.")]
        public async Task<string> GetWeather(string city)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://wttr.in/{city}?format=%C+%t");
            string theWeather = await response.Content.ReadAsStringAsync();
            return theWeather;
        }
    }
}