using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetService<LoginViewModel>();
        }
    }
}