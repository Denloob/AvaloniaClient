using System;
using System.Text.RegularExpressions;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Network;

namespace Client;

public partial class SignupView : UserControl
{
    private MainWindow mainWindow;

    public SignupView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;

        birthdateInput.MaxYear = DateTimeOffset.Now;
    }

    private static bool ValidateCity(string city)
    {
        Regex pattern = new Regex(@"^[a-zA-Z\- ]+$");

        return pattern.IsMatch(city);
    }

    private static bool ValidateStreet(string street)
    {
        Regex pattern = new Regex(@"^[a-zA-Z\\. ]+$");
        return pattern.IsMatch(street);
    }
    
    public void SignupButtonHandler(object sender, RoutedEventArgs args)
    {
        Utils.TryOrElseOpenPopup(
            () => {
                var username = usernameInput.Text ?? "";
                var password = passwordInput.Text ?? "";
                var email = emailInput.Text ?? "";
                var city = cityInput.Text ?? "";
                var apartment = apartmentInput.Text ?? "";
                var street = streetInput.Text ?? "";
                var phone = phoneInput.Text ?? "";
                var birthdate = birthdateInput.SelectedDate;

                var birthdateEpoch = birthdate.HasValue ? birthdate.Value.ToUnixTimeSeconds() : 0;
                var address = $"{city}, {apartment}, {street}";

                var portString = portInput.Text ?? "";

                if (!Utils.IsInteger(portString))
                    throw new ArgumentException("Invalid port");
                var port = int.Parse(portString);

                if (!Utils.IsInteger(apartment))
                    throw new ArgumentException("Apartment number must be an integer");

                if (!ValidateCity(city))
                    throw new ArgumentException("City must only contain letters, '-' and spaces and cannot be empty");

                if (!ValidateStreet(street))
                    throw new ArgumentException("Street must only contain letters, '.' and spaces and cannot be empty");

                if (username == "")
                    throw new ArgumentException("Username must not be empty");

                if (birthdateEpoch == 0)
                    throw new ArgumentException("Birthdate must be valid");

                var communicator = Communicator.Instance;
                communicator.Connect(port);

                communicator.Request(RequestCode.SIGNUP, new {
                    username,
                    password,
                    email,
                    address,
                    phone,
                    birthdate = birthdateEpoch
                });

                communicator.Request(RequestCode.LOGIN, new { username, password });

                this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
            },
            shouldDisplayExceptionMessage: (e) =>
                e is ArgumentException || e is System.Net.Sockets.SocketException,
            popup: errorPopup
        );
    }

    public void LoginButtonHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new LoginView(this.mainWindow));
    }


}
