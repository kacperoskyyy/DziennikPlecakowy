using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using DziennikPlecakowy.Interfaces.Local;
using MApplication = Android.App.Application;

namespace DziennikPlecakowy.Platforms.Android.Services;

// Implementacja serwisu powiadomień dla Androida

[Service]
internal class PlatformNotificationService : Service, IPlatformNotificationService
{
    public const string ChannelId = "DziennikPlecakowyChannel";
    public const int NotificationId = 101;

    public override IBinder OnBind(Intent intent) => null;

    public void StartForegroundService(string title, string text)
    {
        CreateNotificationChannel();
        var notification = BuildNotification(title, text);

        StartForeground(NotificationId, notification);
    }

    public void UpdateNotification(string title, string text)
    {
        var notification = BuildNotification(title, text);
        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        notificationManager.Notify(NotificationId, notification);
    }

    public void StopForegroundService()
    {
        StopForeground(true);
        StopSelf();
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        return StartCommandResult.Sticky;
    }

    private Notification BuildNotification(string title, string text)
    {
        var context = MApplication.Context;
        var intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
        var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetSmallIcon(Resource.Mipmap.appicon_foreground)
            .SetOngoing(true) 
            .SetContentIntent(pendingIntent)
            .Build();

        return notification;
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var channelName = "Dziennik Plecakowy";
        var channelDescription = "Powiadomienia o śledzeniu wycieczki";
        var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Default)
        {
            Description = channelDescription
        };

        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        notificationManager.CreateNotificationChannel(channel);
    }
}