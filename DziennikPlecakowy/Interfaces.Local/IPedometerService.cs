namespace DziennikPlecakowy.Interfaces.Local;

public interface IPedometerService
{
    event EventHandler<long> ReadingChanged;
    void Start();
    void Stop();
}
