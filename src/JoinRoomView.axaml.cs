using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Newtonsoft.Json;

using Network;

namespace Client;

public partial class JoinRoomView : UserControl
{
    private const int updateInterval = 3;

    private MainWindow mainWindow;
    public ObservableCollection<RoomData> Rooms { get; set; }
    
    public int SelectedIndex {
        get => -1; // Nothing is selected.
        set {
            Utils.TryOrElseOpenPopup(
                () => {
                    this.JoinRoom(this.Rooms[value].ID);
                },
                shouldDisplayExceptionMessage: (e) => true,
                popup: errorPopup
            );
        }
    }

    private Clock clock;

    public JoinRoomView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        this.Rooms = new ObservableCollection<RoomData>(new List<RoomData>());

        this.clock = new Clock(updateInterval, () => {
            this.UpdateRoomList();
        });

        DataContext = this;
    }
    public void BackToMenuButtonHandler(object sender, RoutedEventArgs args)
    {
        this.clock.Terminate();

        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }
    private void UpdateRoomList()
    {
        var communicator = Communicator.Instance;
        string jsonString = communicator.Request(RequestCode.GET_ROOMS, new {});
        var receivedRooms =
            JsonConvert.DeserializeAnonymousType(
                jsonString,
                new { status = 0, rooms = new[] { new { id = 0, name = "", maxPlayers = 0, numOfQuestionsInGame = 0, timePerQuestion = 0, isActive = 0 }} }
            )?.rooms;

        if (receivedRooms == null)
            throw new Exception("Received an invalid room");

        this.Rooms.Clear();
        foreach (var room in @receivedRooms )
        {
            var roomData = new RoomData{
                ID = room.id,
                Name = room.name,
                PlayerCount = JoinRoomView.GetPlayerCountInRoom(room.id),
                MaxPlayers = room.maxPlayers,
                NumOfQuestionsInGame = room.numOfQuestionsInGame,
                TimePerQuestion = room.timePerQuestion,
                IsActive = room.isActive == 1,
            };

            this.Rooms.Add(roomData);
        }
    }

    private static int GetPlayerCountInRoom(int roomId)
    {
        var communicator = Communicator.Instance;
        string jsonString = communicator.Request(RequestCode.GET_PLAYERS_IN_ROOM, new { roomId });
        var roomPlayers = JsonConvert.DeserializeAnonymousType(
            jsonString,
            new { status = 0, players = new List<string>() }
        )?.players;

        return roomPlayers?.Count ?? 0;
    }

    private void JoinRoom(int roomId)
    {
        this.clock.Terminate();

        var communicator = Communicator.Instance;
        communicator.Request(RequestCode.ROOM_JOIN, new { roomId });

        this.mainWindow.ChangeView(new AmongUsLobbyView(this.mainWindow, roomId, false));
    }
}

public class RoomData
{
    public int ID { get; set; }
    public string Name { get; set; } = "[Unnamed Room]";
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public int NumOfQuestionsInGame { get; set; }
    public int TimePerQuestion { get; set; }
    public bool IsActive { get; set; }
}
