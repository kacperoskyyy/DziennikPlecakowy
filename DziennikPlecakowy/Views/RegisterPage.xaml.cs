using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetService<RegisterViewModel>();
        }
    }
}
