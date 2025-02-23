using System;
using System.Text.RegularExpressions;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Network;

namespace Client;

public partial class AddQuestionView : UserControl
{
    private MainWindow mainWindow;

    public AddQuestionView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = this;
    }

    private static bool ValidateInput(string city)
    {
        Regex pattern = new Regex(@"^[a-zA-Z\- ]+$");

        return pattern.IsMatch(city);
    }
    public void AddQuestionHandler(object sender, RoutedEventArgs args)
    {
        Utils.TryOrElseOpenPopup(
            () => {
                var question = questionInput.Text ?? "";
                var answer = answerInput.Text ?? "";
                var wrong1 = wrongInput1.Text ?? "";
                var wrong2 = wrongInput2.Text ?? "";
                var wrong3 = wrongInput3.Text ?? "";
                var port = portInput.Text ?? "";


                if (!Utils.IsInteger(port))
                    throw new ArgumentException("Apartment number must be an integer");

                if (!ValidateInput(question) || !ValidateInput(answer) || !ValidateInput(wrong1) || !ValidateInput(wrong2) || !ValidateInput(wrong3))
                    throw new ArgumentException("Input must only contain letters, '-' and spaces and cannot be empty");

                var communicator = Communicator.Instance;
                communicator.Connect(int.Parse(port));

                communicator.Request(RequestCode.QUESTION_ADD, new {
                    question,
                    answer,
                    wrong1,
                    wrong2,
                    wrong3,
                });

                this.mainWindow.ChangeView(new LoginView(this.mainWindow));
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
