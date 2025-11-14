using DziennikPlecakowy.ViewModels;
using System.ComponentModel;
using System.Text.Json;
using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Views;

public partial class TripDetailPage : ContentPage
{
    private readonly TripDetailViewModel _viewModel;

    private bool _isWebViewReady = false;

    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
    };

    public TripDetailPage(TripDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        mapWebView.Navigating += MapWebView_Navigating;
    }

    private void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url != null && e.Url.StartsWith("jsready://"))
        {
            _isWebViewReady = true;
            e.Cancel = true;
            if (_viewModel.TripDetails != null)
            {
                InjectDataIntoWebView();
            }
        }
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TripDetailViewModel.TripDetails) && _isWebViewReady)
        {
            InjectDataIntoWebView();
        }
    }

    private async void InjectDataIntoWebView()
    {
        if (_viewModel.TripDetails == null)
            return;

        var geoPoints = _viewModel.TripDetails.GeoPointList;

        if (geoPoints == null || !geoPoints.Any())
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] InjectData: GeoPointList jest pusta lub null.");
            return;
        }

        try
        {
            string json = JsonSerializer.Serialize(geoPoints, _jsonOptions);

            System.Diagnostics.Debug.WriteLine("[DEBUG] Wstrzykiwanie JSON do WebView:");
            System.Diagnostics.Debug.WriteLine(json);

            string jsCommand = $"window.loadMapAndRoute({json});";
            await mapWebView.EvaluateJavaScriptAsync(jsCommand);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] B³¹d wstrzykiwania JSON do WebView: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        mapWebView.Navigating -= MapWebView_Navigating;
    }
}