using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Services;
using Microsoft.AspNetCore.Authorization;

namespace DziennikPlecakowy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripController : Controller
    {
        private readonly ITripService _tripService;
        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }
        [Authorize]
        [HttpPost("addTrip")]
        public async Task<IActionResult> AddTrip([FromBody] TripAddRequest tripAddRequest)
        {
            try
            {
                Trip trip = new Trip()
                {
                    UserId = tripAddRequest.UserId,
                    TripDate = tripAddRequest.TripDate,
                    Distance = tripAddRequest.Distance,
                    Duration = tripAddRequest.Duration,
                    GeopointList = tripAddRequest.GeopointList
                };
                var result = await _tripService.AddTrip(trip);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się dodać wycieczki.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się dodać wycieczki. " + e);
            }
        }
        [Authorize]
        [HttpPost("updateTrip/{trip}")]
        public async Task<IActionResult> UpdateTrip([FromBody] Trip trip)
        {
            try
            {
                var result = await _tripService.UpdateTrip(trip);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować wycieczki.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się zaktualizować wycieczki. " + e);
            }
        }
        [Authorize]
        [HttpPost("deleteTrip/{tripId}")]
        public async Task<IActionResult> DeleteTrip(string tripId)
        {
            try
            {
                var result = await _tripService.DeleteTrip(tripId);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się usunąć wycieczki.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się usunąć wycieczki. " + e);
            }
        }
        [Authorize]
        [HttpGet("getUserTrips/{user}")]
        public async Task<IActionResult> GetUserTrips([FromBody] AuthData user)
        {
            try
            {
                var result = await _tripService.GetUserTrips(user);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się pobrać wycieczek użytkownika.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się pobrać wycieczek użytkownika. " + e);
            }
        }
    }
}
