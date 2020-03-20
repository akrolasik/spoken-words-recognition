using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NUnit.Framework;

namespace API.UnitTests.Tests
{
    [Order(1)]
    public class AuthorizationUnitTests : BaseControllerUnitTests
    {
        [Test, Order(1)]
        public async Task TestAnonymous()
        {
            var result = await Client.GetAsync("health");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test, Order(2)]
        public async Task TestUnauthorized()
        {
            var result = await Client.GetAsync("protected-endpoint");
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test, Order(3)]
        public async Task TestAuthorized()
        {
            var token = Configuration["TestToken"];
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

            var result = await Client.GetAsync("protected-endpoint");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }
    }
}