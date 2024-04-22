using Microsoft.AspNetCore.Mvc;
using simposio.Models;
using simposio.Services.DAO;

namespace simposio.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MerchandisingController : Controller
    {
        private readonly MerchandisingDAO _merchandisingDAO;

        public MerchandisingController(MerchandisingDAO merchandisingDAO)
        {
            _merchandisingDAO = merchandisingDAO;
        }


        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            List<Merchandising> merchandisings = await _merchandisingDAO.GetAllAsync();
            return Ok(merchandisings);
        }
    }
}
