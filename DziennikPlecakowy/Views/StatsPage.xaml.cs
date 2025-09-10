using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class StatsPage : ContentPage
    {
        public StatsPage()
        {
            InitializeComponent();
            BindingContext = App.Current.Services.GetService<StatsViewModel>();
        }
    }
}
