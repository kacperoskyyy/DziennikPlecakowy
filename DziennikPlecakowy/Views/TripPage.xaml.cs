using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class TripPage : ContentPage
    {
        public TripPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetService<TripViewModel>();
        }
    }
}
