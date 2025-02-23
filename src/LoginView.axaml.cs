using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Network;

namespace Client;

public partial class LoginView : UserControl
{
    private MainWindow mainWindow;

    public LoginView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
    }

    public void SignupButtonHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new SignupView(this.mainWindow));
    }
    public void AddQuestionHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new AddQuestionView(this.mainWindow));
    }

    public void LoginButtonHandler(object sender, RoutedEventArgs args)
    {
        Utils.TryOrElseOpenPopup(
            () => {
                var username = usernameInput.Text ?? "";
                var password = passwordInput.Text ?? "";

                var portString = portInput.Text ?? "";

                if (!Utils.IsInteger(portString))
                    throw new ArgumentException("Invalid port");
                var port = int.Parse(portString);

                var communicator = Communicator.Instance;
                communicator.Connect(port);

                communicator.Request(RequestCode.LOGIN, new { username, password });

                this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
            },
            shouldDisplayExceptionMessage: (e) =>
                e is ArgumentException || e is System.Net.Sockets.SocketException,
            popup: errorPopup
        );
    }
}
