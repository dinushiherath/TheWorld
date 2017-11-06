using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TheWorld.Models;
using TheWorld.ViewModels;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.API
{
    [Route("api/trips")]
    [Authorize]
    public class TripsController : Controller
    {
        private IWorldRepository _repository;
        private ILogger<TripsController> _logger;

        public TripsController(IWorldRepository repository, ILogger<TripsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            try
            {
                var results = _repository.GetTripsByUsername(this.User.Identity.Name);

                return Ok(Mapper.Map<IEnumerable<TripViewModel>>(results));
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to get all trips: { ex }");

                return BadRequest("Error occured");
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody]TripViewModel theTrip)
        {
            if (ModelState.IsValid)
            {
                var newTrip = Mapper.Map<Trip>(theTrip);
                _repository.AddTrip(newTrip);

                newTrip.UserName = User.Identity.Name;

                if (await _repository.SaveChangesAsync())
                {
                    return Created($"api/trips/{theTrip.Name}", Mapper.Map<TripViewModel>(newTrip));
                }
                
            }

            return BadRequest("Failed to save the trip"); //don't send the model state for an API that is publicly used
        }
    }
}
