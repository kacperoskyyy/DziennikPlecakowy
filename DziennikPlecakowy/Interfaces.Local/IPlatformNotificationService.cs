namespace DziennikPlecakowy.Interfaces.Local;
// Interfejs usługi powiadomień 
public interface IPlatformNotificationService
{
    void StartForegroundService(string title, string text);

    void UpdateNotification(string title, string text);

    void StopForegroundService();
}