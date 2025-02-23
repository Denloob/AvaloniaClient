using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Newtonsoft.Json;

using Network;

namespace Client;

public partial class GameResultsView : UserControl
{
    private const int updateInterval = 2;
    private MainWindow mainWindow;


    private Clock clock;

    public ObservableCollection<PlayerResults> Results { get; set; }

    public GameResultsView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        this.Results = new ObservableCollection<PlayerResults>(new List<PlayerResults>());

        this.clock = new Clock(updateInterval, () => {
            GameResultsView.UpdateResults(this.Results);
        });

        DataContext = this;
    }

    public void BackToMenuButtonHandler(object sender, RoutedEventArgs args)
    {
        this.clock.Terminate();

        var communicator = Communicator.Instance;
        communicator.Request(RequestCode.GAME_LEAVE, new {});

        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }

    private static void UpdateResults(ObservableCollection<PlayerResults> Results)
    {
        var communicator = Communicator.Instance;
        string gameResultsJson = communicator.Request(RequestCode.GET_GAME_RESULT, new {});
        var receivedResults =
            JsonConvert.DeserializeAnonymousType(
                gameResultsJson,
                new { status = 0, results = new List<PlayerResults>() }
            )?.results;

        if (receivedResults != null)
        {
            Results.Clear();
            foreach (var item in @receivedResults)
            {
                Results.Add(item);
            }
        }
    }
}
