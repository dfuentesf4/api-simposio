using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using simposio.Models;
using simposio.Services.DAO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace simposio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ColaboradorController : Controller
    {
        private readonly ColaboradorDAO _colaboradorDAO;
        private readonly IConfiguration _configuration;

        public ColaboradorController(ColaboradorDAO colaboradorDAO, IConfiguration configuration)
        {
            _colaboradorDAO = colaboradorDAO;
            _configuration = configuration;
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
                var token = GenerateJwtToken(col);
                col.Contraseña = "";
                return Ok(new { Colaborador = col, Token = token });
            }
            return BadRequest("Credenciales invalidas");
        }

        private string GenerateJwtToken(Colaborador colaborador)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, colaborador.Usuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
