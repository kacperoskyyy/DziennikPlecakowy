using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class AccountPage : ContentPage
{
    public AccountPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnReportBugClicked(object sender, EventArgs e)
    {
        await Shell.Current.DisplayAlert(
            "Zg³aszanie b³êdów",
            "Prosimy o zg³aszanie b³êdów na adres email:\n\nkacper.d2208@gmail.com",
            "OK");
    }

}