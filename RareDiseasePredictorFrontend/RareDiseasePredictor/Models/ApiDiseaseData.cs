using System.Text.Json;

namespace RareDiseasePredictor.Models
{
    public class ApiDiseaseData
    {
        public List<Disease> diseases { get; private set; }
        public List<Symptom> symptoms { get; private set; }
        public List<Region> regions { get; private set; }

        public async Task LoadDiseaseData(HttpClient httpClient, string url)
        {
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                diseases = JsonSerializer.Deserialize<List<Disease>>(jsonData);
            }

        }

        public async Task LoadSymptomsData(HttpClient httpClient, string url)
        {
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                symptoms = JsonSerializer.Deserialize<List<Symptom>>(jsonData);
            }

        }

        public async Task LoadRegionsData(HttpClient httpClient, string url)
        {
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                regions = JsonSerializer.Deserialize<List<Region>>(jsonData);
            }

        }

    }
}