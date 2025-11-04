using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using DziennikPlecakowy.Interfaces.Local;
using MApplication = Android.App.Application;

namespace DziennikPlecakowy.Platforms.Android.Services;

internal class PlatformNotificationService : IPlatformNotificationService
{
    public const string ChannelId = "DziennikPlecakowyChannel";
    public const int NotificationId = 101;

    private readonly Context _context;
    private readonly NotificationManager _notificationManager;

    public PlatformNotificationService()
    {

        _context = MApplication.Context;
        _notificationManager = (NotificationManager)_context.GetSystemService(Context.NotificationService);
    }


    public void StartForegroundService(string title, string text)
    {
        CreateNotificationChannel();
        var notification = BuildNotification(title, text);

        _notificationManager.Notify(NotificationId, notification);
    }

    public void UpdateNotification(string title, string text)
    {
        var notification = BuildNotification(title, text);
        _notificationManager.Notify(NotificationId, notification);
    }

    public void StopForegroundService()
    {
        _notificationManager.Cancel(NotificationId);
    }


    private Notification BuildNotification(string title, string text)
    {
        var intent = _context.PackageManager.GetLaunchIntentForPackage(_context.PackageName);
        var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(_context, ChannelId)
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

        _notificationManager.CreateNotificationChannel(channel);
    }
}