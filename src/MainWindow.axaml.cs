using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        this.Content = new LoginView(this);
    }

    public void ChangeView(UserControl view)
    {
        this.Content = view;
    }
}
