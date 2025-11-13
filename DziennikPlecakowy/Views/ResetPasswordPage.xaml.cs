using DziennikPlecakowy.ViewModels;

namespace DziennikPlecakowy.Views;

public partial class ResetPasswordPage : ContentPage 
{
	public ResetPasswordPage(ResetPasswordViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}