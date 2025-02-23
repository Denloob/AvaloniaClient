using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Newtonsoft.Json;
using Network;
using System.Numerics;

namespace Client;
public class Player
{
    public string Name { get; set; }
    public IBrush Color { get; set; }
}

public partial class AmongUsLobbyView : UserControl, INotifyPropertyChanged
{
    private const int updateInterval = 1;
    private MainWindow mainWindow;

    private Clock clock;

    public ObservableCollection<Player> Players { get; set; }

    private int roomId;

    private int timePerQuestion;
    private int questionCount;

    bool isAdmin;

    public AmongUsLobbyView(MainWindow mainWindow, int roomId, bool isAdmin)
    {
        InitializeComponent();
        this.roomId = roomId;
        this.mainWindow = mainWindow;
        this.Players = new ObservableCollection<Player>(new List<Player>());
        this.isAdmin = isAdmin;

        this.clock = new Clock(updateInterval, () => {
            this.UpdatePlayerList();
        });

        if (!this.isAdmin)
        {
            RemoveButton("AdminButton");
        }
        exitRoomButton.Content = this.isAdmin ? "Close Room" : "Leave Room";

        DataContext = this;
    }

    private void SwitchToGameView()
    {
        this.clock.Terminate();

        this.mainWindow.ChangeView(new GameView(this.mainWindow, this.timePerQuestion, this.questionCount));
    }

    public void StartGameButtonHandler(object sender, RoutedEventArgs args)
    {
        this.clock.Terminate(); // We must do it separately, because we don't want the clock to tick after GAME_START is sent

        var communicator = Communicator.Instance;
        communicator.Request(RequestCode.GAME_START, new { });
        SwitchToGameView();
    }

    public void ExitRoomButtonHandler(object sender, RoutedEventArgs args)
    {
        this.clock.Terminate();

        var communicator = Communicator.Instance;
        communicator.Request(this.isAdmin ? RequestCode.ROOM_CLOSE : RequestCode.ROOM_LEAVE, new { });

        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }

    private void UpdatePlayerList()
    {
        var communicator = Communicator.Instance;
        string jsonString = communicator.Request(RequestCode.GET_ROOM_STATE, new { });
        var roomState = JsonConvert.DeserializeAnonymousType(
            jsonString,
            new
            {
                status = 0,
                hasGameBegun = false,
                players = new List<string>(),
                questionCount = 0,
                answerTimeout = 0,
            }
        );

        var gameHasBegun = roomState?.hasGameBegun ?? false;
        if (gameHasBegun)
        {
            SwitchToGameView();
            return;
        }

        var roomPlayers = roomState?.players;
        var questionCount = roomState?.questionCount;
        var timePerQuestion = roomState?.answerTimeout;
        if (roomPlayers == null || questionCount == null || timePerQuestion == null)
        {
            return;
        }

        this.questionCount = questionCount.Value;
        this.timePerQuestion = timePerQuestion.Value;

        this.Players.Clear();

        foreach (var playerName in roomPlayers)
        {
            this.Players.Add(new Player { Name = playerName, Color = GetRandomColor() });
        }
    }

    private void RemoveButton(string buttonName)
    {
        var button = this.FindControl<Button>(buttonName);
        if (button != null)
        {
            if (button.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(button);
            }
        }
    }

    public IBrush GetRandomColor()
    {
        Random random = new Random();
        return new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
