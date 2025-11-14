using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using DziennikPlecakowy.Interfaces.Local;

namespace DziennikPlecakowy.Platforms.Android.Services
{
    [Service(Name = "com.DziennikPlecakowy.DziennikPlecakowy.TrackingService", ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
    public class TrackingService : Service
    {
        public const string ChannelId = "DziennikPlecakowyChannel";
        public const int NotificationId = 101;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();

            string title = intent?.GetStringExtra("title") ?? "Dziennik Plecakowy";
            string text = intent?.GetStringExtra("text") ?? "Śledzenie aktywne";

            var notification = BuildNotification(title, text);

            StartForeground(NotificationId, notification);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            StopForeground(true);
            StopSelf();
        }

        private Notification BuildNotification(string title, string text)
        {
            var intent = PackageManager.GetLaunchIntentForPackage(PackageName);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable);

            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Mipmap.appicon_foreground)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent)
                .SetPriority(NotificationCompat.PriorityLow)
                .SetSilent(true)
                .Build();

            return notification;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

            var channelName = "Dziennik Plecakowy";
            var channelDescription = "Powiadomienia o śledzeniu wycieczki";
            var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Low)
            {
                Description = channelDescription
            };

            channel.SetSound(null, null);
            channel.EnableVibration(false);
            channel.EnableLights(false);

            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}