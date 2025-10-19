namespace DziennikPlecakowy.Interfaces.Local;

public interface IPlatformNotificationService
{
    void StartForegroundService(string title, string text);

    void UpdateNotification(string title, string text);

    void StopForegroundService();
}