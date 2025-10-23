using DziennikPlecakowy.ViewModels;
namespace DziennikPlecakowy.Views;

public partial class DashboardPage : ContentPage
{
	private readonly DashboardViewModel _viewModel;

    private System.Timers.Timer _stopHoldTimer;
    private bool _isStopCommandExecuted = false;

    public DashboardPage(DashboardViewModel dashboardViewModel)
	{
        _viewModel = dashboardViewModel;
		BindingContext = _viewModel;
        InitializeComponent();


        _stopHoldTimer = new System.Timers.Timer(3000);
        _stopHoldTimer.Elapsed += OnStopTimerElapsed;
        _stopHoldTimer.AutoReset = false;
    }

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
        _viewModel.Cleanup();
    }

    private void StopButton_PointerPressed(object sender, PointerEventArgs e)
    {
        _isStopCommandExecuted = false;
        _stopHoldTimer.Start();

        StopButtonBorder.ScaleTo(1.1, 100);
    }

    private void StopButton_PointerReleased(object sender, PointerEventArgs e)
    {
        _stopHoldTimer.Stop();

        if (!_isStopCommandExecuted)
        {
            StopButtonBorder.ScaleTo(1.0, 100);
        }
    }

    private async void OnStopTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        // Timer dzia³a w tle, musimy wróciæ do w¹tku UI
        await MainThread.InvokeOnMainThreadAsync(() =>
        {

            _isStopCommandExecuted = true;

            if (_viewModel.StopTrackingCommand.CanExecute(null))
            {
                _viewModel.StopTrackingCommand.Execute(null);
            }

            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100)); } catch { }

            StopButtonBorder.ScaleTo(1.0, 100);
        });
    }


}