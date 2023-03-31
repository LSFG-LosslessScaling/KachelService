using EasyJwt.Models;
using KachelService.Model.Customer;
using System.Net.Http.Headers;
using System.Text.Json;

namespace KachelService.Services
{
    public class CustomerService
    {
        private static string jwtoken = String.Empty;
        private static string jwRenewToken = String.Empty;

        public async Task<Customer?> GetCustomer(string token)
        {
            var client = new HttpClient();

            if (string.IsNullOrEmpty(jwtoken))
            {
                await UpdateJwt();
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{Configuration.CustomerService}/api/Customer/Token/{token}/86400");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtoken);
            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<Customer>
                (await response.Content.ReadAsStringAsync(), new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }

        private async Task UpdateJwt()
        {
            var client = new HttpClient();
            var user = "Holger";
            var password = "123";

            HttpResponseMessage result = await client.PostAsJsonAsync($"{Configuration.CustomerService}/api/Account/Login/", new LoginRequest() { UserName = user, Password = password });
            if (result.IsSuccessStatusCode)
            {
                var loginResult = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginResult>(await result.Content.ReadAsStreamAsync());
                if (loginResult != null)
                {
                    jwtoken = loginResult.AccessToken;
                    jwRenewToken = loginResult.RefreshToken;
                }
            }
        }

    }
}
