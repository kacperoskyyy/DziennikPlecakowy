using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views
{
    public partial class TripDetailPage : ContentPage
    {
        public TripDetailPage(TripDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}