using DziennikPlecakowy.Models;
using DziennikPlecakowy.Interfaces;

namespace DziennikPlecakowy.Services;

// Klasa wyjątku dla nieautoryzowanych modyfikacji wycieczek
public class UnauthorizedTripModificationException : Exception
{
    public UnauthorizedTripModificationException(string message) : base(message) { }
}

// Serwis zarządzania wycieczkami
public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IUserStatRepository _userStatRepository;
    // Konstruktor serwisu wycieczek
    public TripService(ITripRepository tripRepository, IUserStatRepository userStatRepository)
    {
        _tripRepository = tripRepository;
        _userStatRepository = userStatRepository;
    }
    // Dodawanie nowej wycieczki
    public async Task<Trip> AddTripAsync(Trip trip)
    {
        if (trip == null) return null;

        await _tripRepository.AddAsync(trip);

        var stats = await _userStatRepository.GetByUserIdAsync(trip.UserId);

        if (stats != null)
        {
            stats.TripsCount++;
            stats.TotalDistance += trip.Distance;
            stats.TotalDuration += trip.Duration;
            stats.TotalElevationGain += trip.ElevationGain;
            stats.TotalSteps += trip.Steps;

            await _userStatRepository.UpdateAsync(stats);
        }

        return trip;
    }
    // Aktualizowanie istniejącej wycieczki
    public async Task<bool> UpdateTripAsync(Trip trip, string userId)
    {
        Trip? existingTrip = await _tripRepository.GetByIdAsync(trip.Id);

        if (existingTrip == null)
        {
            return false;
        }

        if (existingTrip.UserId != userId)
        {
            throw new UnauthorizedTripModificationException(
                $"Użytkownik {userId} próbował zaktualizować wycieczkę {trip.Id} należącą do innego użytkownika."
            );
        }

        trip.UserId = userId;

        var result = await _tripRepository.UpdateAsync(trip);

        return result;
    }
    // Usuwanie wycieczki
    public async Task<bool> DeleteTripAsync(string tripId, string userId)
    {
        Trip? existingTrip = await _tripRepository.GetByIdAsync(tripId);

        if (existingTrip == null)
        {
            return false;
        }

        if (existingTrip.UserId != userId)
        {
            throw new UnauthorizedTripModificationException(
                $"Użytkownik {userId} próbował usunąć wycieczkę {tripId} należącą do innego użytkownika."
            );
        }

        var result = await _tripRepository.DeleteAsync(tripId);

        if (result)
        {
            var stats = await _userStatRepository.GetByUserIdAsync(userId);
            if (stats != null)
            {
                stats.TripsCount--;
                stats.TotalDistance -= existingTrip.Distance;
                stats.TotalDuration -= existingTrip.Duration;
                stats.TotalElevationGain -= existingTrip.ElevationGain;
                stats.TotalSteps -= existingTrip.Steps;

                await _userStatRepository.UpdateAsync(stats);
            }
        }

        return result;
    }
    // Pobieranie wycieczek użytkownika
    public async Task<IEnumerable<Trip>> GetUserTripsAsync(string userId)
    {
        return await _tripRepository.GetByUserAsync(userId);
    }
}