using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace msal_api.Controllers
{
    [Authorize]
    //[RequiredScope("read-access")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IServiceBusMessageHandler _messageHandler;
        public FileController(IServiceBusMessageHandler messageHandler)
        {
            _messageHandler= messageHandler;
        }

        [HttpPost]
        [Route("post")]
        public async Task<IActionResult> PostFileInfo(FileInfoMessage fileInfoMessage)
        {
            var user = HttpContext.User;
            var objectId = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            await _messageHandler.SendMessageAsync(fileInfoMessage);

            return Ok(objectId);
        }
    }
}
