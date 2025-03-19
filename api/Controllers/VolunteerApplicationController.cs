using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VolunteerApplicationController : ControllerBase
    {
        private readonly VolunteerApplicationService _volunteerApplicationService;

        public VolunteerApplicationController(VolunteerApplicationService volunteerApplicationServic)
        {
            _volunteerApplicationService = volunteerApplicationServic;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply(VolunteerApplication application)
        {
            var isValid = _volunteerApplicationService.AreValidAreasOfInterest(application.AreasOfInterest);
            var isValidOtherAreas = application.ValidateOtherAreas();

            if (string.IsNullOrWhiteSpace(application.FullName) || application.FullName.Length < 3)
            {
                return BadRequest("El nombre completo debe tener al menos 3 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(application.Email) || !new EmailAddressAttribute().IsValid(application.Email))
            {
                return BadRequest("El correo electrónico no tiene un formato válido.");
            }

            if (!isValid)
            {
                return BadRequest("Algunas de las áreas de interés seleccionadas no son válidas.");
            }

            if (!isValidOtherAreas)
            {
                return BadRequest("El campo 'Otras áreas de interés' debe contener un valor si se selecciona.");
            }

            await _volunteerApplicationService.CreateAsync(application);
            return Ok("Solicitud de voluntariado enviada exitosamente.");
        }
    }
}
