using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simposio.Models;
using simposio.Services.DAO;
using simposio.Services.Email;
using simposio.Services.PDF;

namespace simposio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParticipanteController : Controller
    {
        private readonly ILogger<ParticipanteController> _logger;
        private readonly ParticipanteDAO _participanteDAO;
        private readonly EmailService _emailSender;
        private IWebHostEnvironment _environment;
        private readonly PDFEditor _pdfEditor;

        public ParticipanteController(ILogger<ParticipanteController> logger, 
            ParticipanteDAO participanteDAO,
            EmailService emailSender,
            IWebHostEnvironment environment)
        {
            _participanteDAO = participanteDAO;
            _logger = logger;
            _emailSender = emailSender;
            _environment = environment;
            _pdfEditor = new PDFEditor(_environment);
        }

        [Authorize]
        [HttpGet("Get")]
        public IEnumerable<Participante> Get()
        {
            return _participanteDAO.GetAllAsync().Result;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody]Participante participante)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool creado = await _participanteDAO.InsertAsync(participante);
            if(!creado) return BadRequest();
            _emailSender.EnviarCorreo(participante);
            return Created();
        }

        [HttpGet("GetByCarnet/{carnet}")]
        public async Task<IActionResult> GetIdByCarnet(string carnet)
        {
            var participante = await _participanteDAO.GetByCarnetAsync(carnet);
            if (participante == null)
            {
                return NotFound();
            }
            return Ok(participante);
        }

        [HttpPost("AddMerchandising/{participanteId}/{idMerchandising}/{cantidad}")]
        public async Task<IActionResult> AddMerchandising(int participanteId, int idMerchandising, int cantidad, string? opcion)
        {
            if( await _participanteDAO.AddMerchandisingAsync(participanteId, idMerchandising, cantidad, opcion)) return Ok();
            else return BadRequest();
        }

        [HttpPost("AddDetallePago/{participanteId}/{monto}")]
        public async Task<IActionResult> AddDetallePago(int participanteId, decimal monto)
        {
            if (await _participanteDAO.AddDetallePagoAsync(participanteId, monto))
            {
                _emailSender.EnviarCorreoDetallePago(await _participanteDAO.GetDetallePagoCorreoAsync(participanteId));
                return Ok();
            }
            else return BadRequest();
        }

        [HttpGet("DetallePagoByCarnet/{carnet}")]
        public async Task<IActionResult> DetallePagoByCarnet(string carnet)
        {
            int detallePagoId = await _participanteDAO.GetDetallePagoByCarnetAsync(carnet);
            if (detallePagoId > 5)
            {
                return Ok(detallePagoId);
            }
            return NotFound(detallePagoId);
        }

        [HttpPost("AddPago/{carnet}/{detallePagoId}/{imagen}/{fechaHora}")]
        public async Task<IActionResult> AddPago(string carnet, int detallePagoId, string imagen, DateTime fechaHora)
        {
            imagen = Uri.UnescapeDataString(imagen);
            if (await _participanteDAO.AddPagoAsync(detallePagoId, imagen, fechaHora))
            {
                _emailSender.EnviarCorreoReciboPago(await _participanteDAO.GetByCarnetAsync(carnet));
                return Ok();
            }
            else return BadRequest();
        }

        [HttpGet("GetPagoByDetalle/{idDetalle}")]
        public async Task<IActionResult> GetPagoByCarnet(int idDetalle)
        {
            var pago = await _participanteDAO.GetPagoByDetallePagoAsync(idDetalle);
            if (pago == null)
            {
                return NotFound();
            }
            return Ok(pago);
        }

        [Authorize]
        [HttpPost("PagoVerificado/{pagoId}/{carnet}")]
        public async Task<IActionResult> PagoVerificado(int pagoId, string carnet)
        {
            if (await _participanteDAO.PagoVerificadoAsync(pagoId))
            {
                _emailSender.EnviarCorreoPagoVerificado(await _participanteDAO.GetByCarnetAsync(carnet));
                return Ok();
            }
            else return BadRequest();
        }

        [Authorize]
        [HttpGet("GetPagos")]
        public async Task<IActionResult> GetPagos()
        {
            var pagos = await _participanteDAO.GetPagosAsync();
            if (pagos == null)
            {
                return NotFound();
            }
            return Ok(pagos);
        }

        [Authorize]
        [HttpPost("SendCertificate")]
        public async Task<IActionResult> SendCertificate()
        {
            List<Participante> participantes = _participanteDAO.GetAssisted();
            foreach (var participante in participantes)
            {
                _pdfEditor.AddNameToCertificateAndSave($"{participante.Nombres} {participante.Apellidos}");

                _emailSender.EnviarCorreoCertificado(participante);
            }

            return Ok();
        }
        
    }
}
