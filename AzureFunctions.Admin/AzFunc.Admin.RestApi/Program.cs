

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AzFunc.Admin.RestApi
{
    class Program
    {
        static HttpClient httpClient = new HttpClient();
        static void Main(string[] args)
        {
            var funcAppName = "azfunchostingdd";
            var kudoUserName = Environment.GetEnvironmentVariable("kudoUserName");
            var kudoPassword = Environment.GetEnvironmentVariable("kudoPassword");
            var kudoBaseUrl = $"https://{funcAppName}.scm.azurewebsites.net/api";
            var siteBaseUrl = $"https://{funcAppName}.azurewebsites.net";

            var base64AuthInfo = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{kudoUserName}:{kudoPassword}"));
            var token = GetAdminTokenAsync(kudoBaseUrl, base64AuthInfo).Result;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var status = GetHostStatusAsync(siteBaseUrl).Result;

            Console.WriteLine($"Token: {token}");
        }

        static async Task<HostStatus> GetHostStatusAsync(string siteBaseUrl)
        {
            var response = await httpClient.GetAsync($"{siteBaseUrl}/admin/host/status");  

            return await GetObjectAsync<HostStatus>(response);
        }

        static async Task<string> GetAdminTokenAsync(string kudoBaseUrl, string base64AuthInfo)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);
            var response = await httpClient.GetAsync($"{kudoBaseUrl}/functions/admin/token");

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return body.Replace("\"", string.Empty);
            }
            return string.Empty;
        }

        static async Task<T> GetObjectAsync<T>(HttpResponseMessage response) where T : new()
        {
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(body);
            }
            return default(T);
        }
    }
}
