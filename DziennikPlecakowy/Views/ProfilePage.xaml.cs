using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetService<ProfileViewModel>();
        }
    }
}
