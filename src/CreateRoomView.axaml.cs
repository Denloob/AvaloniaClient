using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Newtonsoft.Json;

using Network;

namespace Client;

public partial class CreateRoomView : UserControl
{
    private MainWindow mainWindow;

    public CreateRoomView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
    }

    public void BackToMenuButtonHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }

    public void CreateRoomButtonHandler(object sender, RoutedEventArgs args)
    {
        Utils.TryOrElseOpenPopup(
            () => {
                var roomName = roomNameInput.Text ?? "";
                var timeForQuestionString = timePerQuestionInput.Text ?? "";
                var amountOfPlayersString = amountOfPlayersInput.Text ?? "";
                var questionCountString = questionCountInput.Text ?? "";

                if (roomName == "")
                    throw new ArgumentException("Room name can't be empty");

                if (!Utils.IsInteger(timeForQuestionString))
                    throw new ArgumentException("'Time For Question' number must be an integer");

                var timeForQuestion = int.Parse(timeForQuestionString);
                if (timeForQuestion <= 0)
                    throw new ArgumentException("'Time For Question' number must be positive");

                if (!Utils.IsInteger(amountOfPlayersString))
                    throw new ArgumentException("'Amount Of Players' number must be an integer");

                var amountOfPlayers = int.Parse(amountOfPlayersString);
                if (amountOfPlayers < 1)
                    throw new ArgumentException("'Amount Of Players' cannot be smaller than 1");

                if (!Utils.IsInteger(questionCountString))
                    throw new ArgumentException("'Question Count' number must be an integer");

                var questionCount = int.Parse(questionCountString);
                if (questionCount <= 0)
                    throw new ArgumentException("'Question Count' number must be positive");

                var communicator = Communicator.Instance;

                string jsonString = communicator.Request(RequestCode.ROOM_CREATE, new {
                    roomName,
                    maxUsers = amountOfPlayers,
                    answerTimeout = timeForQuestion,
                    questionCount,
                });

                int roomId = JsonConvert.DeserializeAnonymousType(
                    jsonString,
                    new { status = 0, roomId = 0 }
                )?.roomId ?? throw new Exception("The server sent a response without a roomId");

                this.mainWindow.ChangeView(new AmongUsLobbyView(this.mainWindow, roomId, true));
            },
            shouldDisplayExceptionMessage: (e) =>
                e is ArgumentException || e is System.Net.Sockets.SocketException,
            popup: errorPopup
        );
    }
}
