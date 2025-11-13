using DziennikPlecakowy.Extensions;
using CommunityToolkit.Maui;

namespace DziennikPlecakowy;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMauiCommunityToolkit();

        builder.RegisterConfiguration();
        builder.RegisterDatabaseAndRepositories();
        builder.RegisterAppServices();
        builder.RegisterViewModelsAndPages();

        return builder.Build();
    }
}