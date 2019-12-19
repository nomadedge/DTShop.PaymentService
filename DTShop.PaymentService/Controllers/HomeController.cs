using Microsoft.AspNetCore.Mvc;

namespace DTShop.PaymentService.Controllers
{
    [Route("/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("info")]
        public ActionResult GetInfo()
        {
            return Ok("Payment Service for DTShop application.");
        }
    }
}
