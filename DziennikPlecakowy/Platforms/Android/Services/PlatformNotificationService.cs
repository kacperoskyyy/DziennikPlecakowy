using Android.App;
using Android.Content;
using Android.OS;
using DziennikPlecakowy.Interfaces.Local;
using DziennikPlecakowy.Platforms.Android.Services;
using MApplication = Android.App.Application;

namespace DziennikPlecakowy.Platforms.Android.Services
{
    internal class PlatformNotificationService : IPlatformNotificationService
    {
        private readonly Context _context;

        public PlatformNotificationService()
        {
            _context = MApplication.Context;
        }

        public void StartForegroundService(string title, string text)
        {
            var intent = new Intent(_context, typeof(TrackingService));
            intent.PutExtra("title", title);
            intent.PutExtra("text", text);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                _context.StartForegroundService(intent);
            }
            else
            {
                _context.StartService(intent);
            }
        }

        public void UpdateNotification(string title, string text)
        {
            var intent = new Intent(_context, typeof(TrackingService));
            intent.PutExtra("title", title);
            intent.PutExtra("text", text);

            _context.StartService(intent);
        }

        public void StopForegroundService()
        {
            var intent = new Intent(_context, typeof(TrackingService));
            _context.StopService(intent);
        }
    }
}