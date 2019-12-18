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

        [HttpGet("Status")]
        public ActionResult GetStatus()
        {
            return Ok(1);
        }

        [HttpGet("healthcheck")]
        public ActionResult CheckHealth()
        {
            return Ok("Payment Service is running normally.");
        }
    }
}
