using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class ForgotPasswordPage : ContentPage
{
	public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}