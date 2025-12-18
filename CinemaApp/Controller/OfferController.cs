using CinemaApp.DTO.Offer;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class OfferController(IOfferService service) : ControllerBase
    {
        private readonly IOfferService _service = service;

        [HttpGet]
        public ActionResult<List<OfferDTO>> GetAll([FromQuery] bool active = false, [FromQuery] bool includeApplied = false)
        {
            if (active)
            {
                return _service.GetActiveOffers(includeApplied);
            }
            else
            {
                return _service.GetAllOffers(includeApplied);
            }
        }

        [HttpGet("{id:int}")]
        public ActionResult<OfferDTO> GetById(int id, [FromQuery] bool includeApplied = false)
        {
            var dto = _service.GetById(id, includeApplied);
            if (dto == null) return NotFound();
            return dto;
        }
    }
}