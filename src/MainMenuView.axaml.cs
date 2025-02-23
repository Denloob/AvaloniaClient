using System;
using System.IO;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Network;

namespace Client;

public partial class MainMenuView : UserControl
{
    private MainWindow mainWindow;

    public MainMenuView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
    }

    public void createRoomButtonHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new CreateRoomView(this.mainWindow));
    }

    public void joinGameButtonHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new JoinRoomView(this.mainWindow));
    }

    public void highScoreButtonHandler(object sender, RoutedEventArgs args)
    {
        Utils.TryOrElseOpenPopup(() => { this.mainWindow.ChangeView(new HighScoreView(this.mainWindow)); },
            shouldDisplayExceptionMessage: (e) =>
            e is ArgumentException || e is System.Net.Sockets.SocketException,
            popup: errorPopup);
        
    }

    public void userStatisticsButtonHandler(object sender, RoutedEventArgs args)
    {
        Utils.TryOrElseOpenPopup(() => { this.mainWindow.ChangeView(new UserStatisticsView(this.mainWindow));},
            shouldDisplayExceptionMessage: (e) =>
            e is ArgumentException || e is System.Net.Sockets.SocketException,
            popup: errorPopup);
        
    }
    //
    public void logoutButtonHandler(object sender, RoutedEventArgs args)
    {
        var communicator = Communicator.Instance;
        communicator.Disconnect();
        this.mainWindow.ChangeView(new LoginView(this.mainWindow));
    }

}