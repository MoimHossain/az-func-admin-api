

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var kudoUserName = string.Empty; // Environment.GetEnvironmentVariable("kudoUserName");
            var kudoPassword = string.Empty; // Environment.GetEnvironmentVariable("kudoPassword");

            var tenantId = Environment.GetEnvironmentVariable("ADTenantID");
            var clientId = Environment.GetEnvironmentVariable("ADClientID");
            var clientSecret = Environment.GetEnvironmentVariable("ADClientSecret");
            var subscriptionId = Environment.GetEnvironmentVariable("AzureSubscriptionID");
            var resourceGroupName = "az-funcs-experiments";
            var funcAppName = "azfunchostingdd";
            var kudoBaseUrl = $"https://{funcAppName}.scm.azurewebsites.net/api";
            var siteBaseUrl = $"https://{funcAppName}.azurewebsites.net";

            var app = GetPublishProfileAsync(tenantId, clientId, clientSecret, subscriptionId, resourceGroupName, funcAppName).Result;
            
            kudoUserName = app.properties.publishingUserName;
            kudoPassword = app.properties.publishingPassword;

            var base64AuthInfo = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{kudoUserName}:{kudoPassword}"));
            var token = GetAdminTokenAsync(kudoBaseUrl, base64AuthInfo).Result;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var status = GetHostStatusAsync(siteBaseUrl).Result;
            Console.WriteLine("Done");
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

        static async Task<FunctionAppMetadata> GetPublishProfileAsync(string tenantId, string clientId, string clientSecret, string subscriptionId, string resourceGroupName, string functionName)
        {
            var token = await GetAzureAdTokenAsync(tenantId, clientId, clientSecret);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web/sites/{functionName}/config/publishingcredentials/list?api-version=2018-02-01";
            var response = await httpClient.PostAsync(url, new StringContent(string.Empty));

            if (response.IsSuccessStatusCode)
            {
                var metadata = JsonConvert.DeserializeObject<FunctionAppMetadata>(await response.Content.ReadAsStringAsync());
                return metadata;
            }
            return default(FunctionAppMetadata);
        }

        private static async Task<string> GetAzureAdTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            httpClient.DefaultRequestHeaders.Clear();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("resource", "https://management.azure.com/"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/token", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AdTokenResponse>(responseString).access_token;
            }
            return string.Empty;
        }
    }
}
