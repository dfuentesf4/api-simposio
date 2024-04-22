using Microsoft.AspNetCore.Mvc;
using simposio.Models;
using simposio.Services.DAO;

namespace simposio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpositorController : Controller
    {
        private readonly ExpositorDAO _expositorDAO;

        public ExpositorController(ExpositorDAO expositorDAO)
        {
            _expositorDAO = expositorDAO;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            List<Expositor> expositores = await _expositorDAO.GetAllAsync();
            return Ok(expositores);
        }
    }
}
