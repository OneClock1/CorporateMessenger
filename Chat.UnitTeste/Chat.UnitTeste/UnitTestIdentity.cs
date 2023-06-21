using FluentAssertions;
using System.Net;
using System.Net.Http;
using Xunit;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace Chat.API.Tests
{
    public class UnitTestIdentity
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public UnitTestIdentity()
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = "http://109.201.239.30:5080"; 
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedStatusCode()
        {
            
            var user = new
            {
                userName = "Anreatol85",
                email = "tfdfdgfg39@gmail.com",
                password = "Pass123$",
                passwordConfirm = "Pass123$"
            };


            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

           
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/users", content);
            var statusCode = response.StatusCode;

           
            Assert.Equal(HttpStatusCode.OK, statusCode);
        }

        
    }
}