using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DziennikPlecakowy.Services;

namespace DziennikPlecakowy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User, Admin")]
public class TripController : ControllerBase
{
    private readonly ITripService _tripService;
    private readonly ILogger<TripController> _logger;
    public TripController(ITripService tripService, ILogger<TripController> logger)
    {
        _tripService = tripService;
        _logger = logger;
    }
    [HttpPost("addTrip")]
    public async Task<IActionResult> AddTrip([FromBody] TripAddRequestDTO tripAddRequest)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Brak Id użytkownika w tokenie.");
        }
        try
        {
            Trip trip = new Trip()
            {
                UserId = userId,
                TripDate = tripAddRequest.TripDate,
                Distance = tripAddRequest.Distance,
                Duration = tripAddRequest.Duration,
                GeoPointList = tripAddRequest.GeoPointList,
                Name = tripAddRequest.Name,
                ElevationGain = tripAddRequest.ElevationGain,
                Steps = tripAddRequest.Steps
            };

            var newTrip = await _tripService.AddTripAsync(trip);

            if (newTrip != null)
            {
                _logger.LogInformation("Trip added successfully by user {UserId}.", userId);

                return Ok(new
                {
                    Message = "Wycieczka została pomyślnie dodana.",
                    TripId = newTrip.Id 
                });
            }
            else
            {
                _logger.LogWarning("Trip not added for user {UserId}. Service returned false.", userId);
                return BadRequest("Nie udało się dodać wycieczki. Sprawdź dane wejściowe.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during AddTrip for user {UserId}.", userId);
            return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera: {e.Message}");
        }
    }
    [HttpPost("updateTrip")]
    public async Task<IActionResult> UpdateTrip([FromBody] Trip trip)
    {
        _logger.LogInformation("Endpoint: POST api/Trip/updateTrip invoked for trip {TripId}.", trip.Id);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _tripService.UpdateTripAsync(trip, userId);

            if (result)
            {
                _logger.LogInformation("Trip {TripId} updated successfully by user {UserId}.", trip.Id, userId);
                return Ok(new { Message = "Wycieczka została pomyślnie zaktualizowana." });
            }
            else
            {
                _logger.LogWarning("Trip update failed for trip {TripId}.", trip.Id);
                return NotFound("Wycieczka o podanym Id nie została znaleziona.");
            }
        }
        catch (UnauthorizedTripModificationException e)
        {
            _logger.LogWarning(e, "Trip modification forbidden: {Message}", e.Message);
            return Forbid("Nie masz uprawnień do edycji tej wycieczki.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during UpdateTrip for trip {TripId}.", trip.Id);
            return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera: {e.Message}");
        }
    }
    [HttpDelete("delete/{tripId}")]
    public async Task<IActionResult> DeleteTrip([FromRoute] string tripId) 
    {
        _logger.LogInformation("Endpoint: DELETE api/Trip/deleteTrip invoked for trip {TripId}.", tripId);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _tripService.DeleteTripAsync(tripId, userId);

            if (result)
            {
                _logger.LogInformation("Trip {TripId} deleted successfully by user {UserId}.", tripId, userId);
                return Ok(new { Message = "Wycieczka została pomyślnie usunięta." });
            }
            else
            {
                _logger.LogWarning("Trip deletion failed for trip {TripId}. Not found or already deleted.", tripId);
                return NotFound("Nie znaleziono wycieczki o podanym Id.");
            }
        }
        catch (UnauthorizedTripModificationException e)
        {
            _logger.LogWarning(e, "Trip deletion forbidden: {Message}", e.Message);
            return Forbid("Nie masz uprawnień do usunięcia tej wycieczki.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during DeleteTrip for trip {TripId}.", tripId);
            return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera: {e.Message}");
        }
    }

    [HttpGet("{tripId}")]
    public async Task<IActionResult> GetTripById([FromRoute] string tripId)
    {
        _logger.LogInformation("Endpoint: GET api/Trip/{tripId} invoked.", tripId);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var trip = await _tripService.GetTripByIdAsync(tripId, userId);

            if (trip != null)
            {
                var tripDetailDto = new TripDetailDTO
                {
                    Id = trip.Id,
                    Name = trip.Name,
                    TripDate = trip.TripDate,
                    Distance = trip.Distance,
                    Duration = trip.Duration,
                    ElevationGain = trip.ElevationGain,
                    Steps = trip.Steps,
                    GeoPointList = trip.GeoPointList.Select(p => new GeoPointDTO
                    {
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        Height = p.Height,
                        Timestamp = p.Timestamp
                    }).ToList()
                };

                _logger.LogInformation("Trip {tripId} retrieved successfully by user {userId}.", tripId, userId);
                return Ok(tripDetailDto);
            }

            _logger.LogWarning("Trip {tripId} not found for user {userId}.", tripId, userId);
            return NotFound("Nie znaleziono wycieczki.");
        }
        catch (UnauthorizedTripModificationException e)
        {
            _logger.LogWarning(e, "Trip access forbidden: {Message}", e.Message);
            return Forbid("Nie masz uprawnień do wyświetlenia tej wycieczki.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during GetTripById for trip {tripId}.", tripId);
            return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera.");
        }
    }

    [HttpGet("getUserTrips")]
    public async Task<IActionResult> GetUserTrips()
    {
        _logger.LogInformation("Endpoint: GET api/Trip/getUserTrips invoked.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _tripService.GetUserTripsAsync(userId);

            if (result != null)
            {
                _logger.LogInformation("Successfully retrieved {Count} trips for user {UserId}.", result.Count(), userId);
                return Ok(result);
            }
            else
            {
                _logger.LogInformation("No trips found for user {UserId}.", userId);
                return NotFound("Nie znaleziono wycieczek dla tego użytkownika.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during GetUserTrips for user {UserId}.", userId);
            return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera: {e.Message}");
        }
    }
    [HttpGet("getUserTripSummaries")]
    public async Task<IActionResult> GetUserTripSummaries()
    {
        _logger.LogInformation("Endpoint: GET api/Trip/getUserTripSummaries invoked.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _tripService.GetUserTripSummariesAsync(userId);

            if (result != null)
            {
                _logger.LogInformation("Successfully retrieved {Count} trip summaries for user {UserId}.", result.Count(), userId);
                return Ok(result);
            }
            else
            {
                _logger.LogInformation("No trip summaries found for user {UserId}.", userId);
                return NotFound("Nie znaleziono wycieczek dla tego użytkownika.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during GetUserTripSummaries for user {UserId}.", userId);
            return StatusCode(500, $"Wystąpił nieoczekiwany błąd serwera: {e.Message}");
        }
    }
}