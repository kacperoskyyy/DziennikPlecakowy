namespace DziennikPlecakowy.PermissionsApp;
// uprawnienia do rozpoznawania aktywności na Androidzie
public class ActivityRecognitionPermission : Microsoft.Maui.ApplicationModel.Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
            (Android.Manifest.Permission.ActivityRecognition, true)
        }.ToArray();
#endif
}