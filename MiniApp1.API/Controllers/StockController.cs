using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace MiniApp1.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        public IActionResult GetStock()
        {
            //Veri tabanında userName e göre datayı çek
            var userName = HttpContext.User.Identity.Name;

            //Veri tabanında Id ye göre datayı çek
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            return Ok($"Stok İşlemleri => UserName:{userName} userId:{userId}");
          
        }
    }
}
