using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Produces("application/json")]
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheckController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Ping() => await Task.FromResult(Ok());
    }
}