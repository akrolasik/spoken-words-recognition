using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.CodeGeneration.TypeScript;

namespace API.Controllers
{
    [Route("")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Policy = "ClaimsAuthorizationPolicy")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Swagger()
        {
            return Redirect("swagger");
        }

        [HttpGet]
        [Route("health")]
        [AllowAnonymous]
        public ActionResult Health()
        {
            return Ok();
        }

        [HttpGet]
        [Route("client.ts")]
        [AllowAnonymous]
        public async Task<ActionResult> GetTypescriptClient()
        {
            var host = HttpContext.Request.Host;
            var document = await OpenApiDocument.FromUrlAsync($"https://{host}/swagger/v1/swagger.json");

            var settings = new TypeScriptClientGeneratorSettings
            {
                ClassName = "{controller}Client",
            };

            var generator = new TypeScriptClientGenerator(document, settings);
            var code = generator.GenerateFile();

            var bytes = Encoding.ASCII.GetBytes(code);

            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "client.ts");
        }

        [HttpGet]
        [Route("protected-endpoint")]
        public ActionResult ProtectedEndpoint()
        {
            return Ok();
        }
    }
}