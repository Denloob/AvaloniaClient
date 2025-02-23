using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client;

public partial class ErrorPopup : UserControl
{
    public ErrorPopup()
    {
        InitializeComponent();
    }

    public void Open(string message)
    {
        errorPopupText.Text = message;
        errorPopup.IsVisible = true;
    }

    public void Close()
    {
        errorPopup.IsVisible = false;
    }

    public void Close(object sender, RoutedEventArgs args)
    {
        Close();
    }
}
