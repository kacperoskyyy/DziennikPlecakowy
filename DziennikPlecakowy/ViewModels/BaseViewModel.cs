using CommunityToolkit.Mvvm.ComponentModel;

namespace DziennikPlecakowy.ViewModels;
//View Model bazowy dla innych ViewModeli

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;

    [ObservableProperty]
    string title;

    public bool IsNotBusy => !IsBusy;
}