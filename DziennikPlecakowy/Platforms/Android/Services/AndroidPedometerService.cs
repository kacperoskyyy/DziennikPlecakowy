using Android.Content;
using Android.Hardware;
using DziennikPlecakowy.Interfaces.Local;
using MApplication = Android.App.Application;

//Implementacja krokomierza dla Androida

namespace DziennikPlecakowy.Platforms.Android.Services;

public class AndroidPedometerService : Java.Lang.Object, IPedometerService, ISensorEventListener
{
    private SensorManager _sensorManager;
    private Sensor _stepCounterSensor;
    private bool _isStarted = false;
    private long _initialSteps = -1;

    public event EventHandler<long> ReadingChanged;

    public AndroidPedometerService()
    {
        _sensorManager = (SensorManager)MApplication.Context.GetSystemService(Context.SensorService);
        _stepCounterSensor = _sensorManager.GetDefaultSensor(SensorType.StepCounter);
    }

    public void Start()
    {
        if (_isStarted || _stepCounterSensor == null)
            return;

        _sensorManager.RegisterListener(this, _stepCounterSensor, SensorDelay.Normal);
        _isStarted = true;
        _initialSteps = -1;
    }

    public void Stop()
    {
        if (!_isStarted || _stepCounterSensor == null)
            return;

        _sensorManager.UnregisterListener(this);
        _isStarted = false;
    }

    public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) { }

    public void OnSensorChanged(SensorEvent e)
    {
        if (e.Sensor.Type == SensorType.StepCounter)
        {
            long totalSteps = (long)e.Values[0];

            if (_initialSteps == -1)
            {
                _initialSteps = totalSteps;
            }

            long stepsSinceStart = totalSteps - _initialSteps;
            ReadingChanged?.Invoke(this, stepsSinceStart);
        }
    }
}