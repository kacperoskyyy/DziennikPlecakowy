using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class TripsListPage : ContentPage
    {
        public TripsListPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetService<TripsListViewModel>();
        }
    }
}
