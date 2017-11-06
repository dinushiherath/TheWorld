using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TheWorld.Models;
using Microsoft.Extensions.Logging;
using TheWorld.ViewModels;
using AutoMapper;
using TheWorld.Services;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.API
{
    [Authorize]
    [Route("/api/trips/{tripName}/stops")]
    public class StopsController : Controller
    {
        private IWorldRepository _repository;
        private ILogger<StopsController> _logger;
        private GeoCoordsService _coordsService;

        // GET: /<controller>/
        public StopsController(IWorldRepository repository, ILogger<StopsController> logger,
            GeoCoordsService coordsService)
        {
            _repository = repository;
            _logger = logger;
            _coordsService = coordsService;

        }

        [HttpGet("")]
        public IActionResult Get(string tripName)
        {
            try
            {
                var trip = _repository.GetUserTripByName(tripName, User.Identity.Name);

                return Ok(Mapper.Map<IEnumerable<StopViewModel>>(trip.Stops.OrderBy(s => s.Order).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get stops: {0}", ex);
            }

            return BadRequest("Failed to get stops");

        }

        [HttpPost("")]
        public async Task<IActionResult> Post(string tripName, [FromBody]StopViewModel vm)
        {
            try
            {
                //If VM is valid
                if (ModelState.IsValid)
                {
                    var newStop = Mapper.Map<Stop>(vm);
                    // Lookup the Geocodes
                    var result = await _coordsService.GetCoordsAsync(newStop.Name);
                    if (!result.Success)
                    {
                        _logger.LogError(result.Message);
                    }
                    else
                    {
                        newStop.Latitude = result.Latitude;
                        newStop.Longitude = result.Longitude;
                        //Save to the Database
                        _repository.AddStop(tripName, newStop, User.Identity.Name);

                        if (await _repository.SaveChangesAsync())
                        {
                            return Created($"/api/trips/{tripName}/stops/{newStop.Name}",
                           Mapper.Map<StopViewModel>(newStop));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save new stop: {0}", ex);
            }

            return BadRequest("Failed to save new stop");
        }
    }
}
