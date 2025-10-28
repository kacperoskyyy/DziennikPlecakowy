namespace DziennikPlecakowy.Interfaces.Local;
// Interfejs usługi krokomierza
public interface IPedometerService
{
    event EventHandler<long> ReadingChanged;
    void Start();
    void Stop();
}
