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
        [Authorize(Roles = "User, Admin")]
        [HttpPost("addTrip")]
        public async Task<IActionResult> AddTrip([FromBody] TripAddRequest tripAddRequest)
        {
            Console.WriteLine("Trip/addTrip");
            try
            {
                Trip trip = new Trip()
                {
                    UserId = tripAddRequest.UserId,
                    TripDate = tripAddRequest.TripDate,
                    Distance = tripAddRequest.Distance,
                    Duration = tripAddRequest.Duration,
                    GeoPointList = tripAddRequest.GeoPointList
                };
                var result = await _tripService.AddTrip(trip);
                if (result > 0)
                {
                    Console.WriteLine("Trip added");
                    return Ok(result);
                }
                else
                {
                    Console.WriteLine("Trip not added");
                    return BadRequest("Nie udało się dodać wycieczki.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Trip not added exception: " + e);
                return BadRequest("Nie udało się dodać wycieczki. " + e);
            }
        }
        [Authorize(Roles = "User, Admin")]
        [HttpPost("updateTrip")]
        public async Task<IActionResult> UpdateTrip([FromBody] Trip trip)
        {
            Console.WriteLine("Trip/updateTrip");
            try
            {
                var result = await _tripService.UpdateTrip(trip);
                if (result > 0)
                {
                    Console.WriteLine("Trip updated");
                    return Ok(result);
                }
                else
                {
                    Console.WriteLine("Trip not updated");
                    return BadRequest("Nie udało się zaktualizować wycieczki.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Trip not updated exception: " + e);
                return BadRequest("Nie udało się zaktualizować wycieczki. " + e);
            }
        }
        [Authorize(Roles = "User, Admin")]
        [HttpPost("deleteTrip")]
        public async Task<IActionResult> DeleteTrip(string tripId)
        {
            Console.WriteLine("Trip/DeleteTrip");
            try
            {
                var result = await _tripService.DeleteTrip(tripId);
                if (result > 0)
                {
                    Console.WriteLine("Trip deleted");
                    return Ok(result);
                }
                else
                {
                    Console.WriteLine("Trip not deleted");
                    return BadRequest("Nie udało się usunąć wycieczki.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Trip not deleted exception: " + e);
                return BadRequest("Nie udało się usunąć wycieczki. " + e);
            }
        }
        [Authorize(Roles = "User, Admin")]
        [HttpGet("getUserTrips")]
        public async Task<IActionResult> GetUserTrips([FromBody] string token)
        {
            Console.WriteLine("Trip/getUserTrips");
            try
            {
                var result = await _tripService.GetUserTrips(token);
                if (result != null)
                {
                    Console.WriteLine("User trips retrieved");
                    return Ok(result);
                }
                else
                {
                    Console.WriteLine("User trips not retrieved");
                    return BadRequest("Nie udało się pobrać wycieczek użytkownika.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("User trips not retrieved exception: " + e);
                return BadRequest("Nie udało się pobrać wycieczek użytkownika. " + e);
            }
        }
    }
}
