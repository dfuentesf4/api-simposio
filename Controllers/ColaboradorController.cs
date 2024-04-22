using Microsoft.AspNetCore.Mvc;
using simposio.Models;
using simposio.Services.DAO;

namespace simposio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ColaboradorController : Controller
    {
        private readonly ColaboradorDAO _colaboradorDAO;

        public ColaboradorController(ColaboradorDAO colaboradorDAO)
        {
            _colaboradorDAO = colaboradorDAO;
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] Colaborador colaborador)
        {
            colaborador.Contraseña = BCrypt.Net.BCrypt.HashPassword(colaborador.Contraseña);
            if (_colaboradorDAO.Insert(colaborador))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("UpdatePassword/{OldPassword}")]
        public IActionResult UpdatePassword( string OldPassword, [FromBody] Colaborador colaborador)
        {
            Colaborador old = _colaboradorDAO.GetColaborador(colaborador.Usuario);
            if (BCrypt.Net.BCrypt.Verify(OldPassword, old.Contraseña))
            {
                colaborador.Contraseña = BCrypt.Net.BCrypt.HashPassword(colaborador.Contraseña);
                colaborador.Id = old.Id;
                if (_colaboradorDAO.UpdatePassword(colaborador))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost("RequestAccess")]
        public IActionResult RequestAccess([FromBody] Colaborador colaborador)
        {
            Colaborador col = _colaboradorDAO.requestAccess(colaborador); 
            if (col != null)
            {
                col.Contraseña = "";
                return Ok(col);
            }
            return BadRequest();
        }
    }
}
