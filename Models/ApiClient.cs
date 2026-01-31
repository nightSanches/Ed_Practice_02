using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EquipmentAccounting.Models.AuthModels;

namespace EquipmentAccounting.Models
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7233/api";

        public ApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string endpoint,
            object? data = null)
        {
            var url = $"{_baseUrl}/{endpoint}";

            var request = new HttpRequestMessage(method, url);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
            => await SendRequestAsync(HttpMethod.Get, endpoint, null);

        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
            => await SendRequestAsync(HttpMethod.Post, endpoint, data);

        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
            => await SendRequestAsync(HttpMethod.Put, endpoint, data);

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
            => await SendRequestAsync(HttpMethod.Delete, endpoint, null);
    }
}