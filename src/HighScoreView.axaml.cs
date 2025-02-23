using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Newtonsoft.Json;

using Network;

namespace Client;

public partial class HighScoreView : UserControl
{
    private const int updateInterval = 10;
    private MainWindow mainWindow;


    private Clock clock;

    public ObservableCollection<string> HighScores { get; set; }

    public HighScoreView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        this.HighScores = new ObservableCollection<string>(new List<string>());

        this.clock = new Clock(updateInterval, () => {
            HighScoreView.UpdateHighScores(this.HighScores);
        });

        DataContext = this;
    }

    public void BackToMenuButtonHandler(object sender, RoutedEventArgs args)
    {
        this.clock.Terminate();

        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }

    private static void UpdateHighScores(ObservableCollection<string> highScores)
    {
        var communicator = Communicator.Instance;
        string highScoresJson = communicator.Request(RequestCode.GET_HIGH_SCORE, new {});
        var receivedHighScores =
            JsonConvert.DeserializeAnonymousType(
                highScoresJson,
                new { status = 0, statistics = new List<string>() }
            )?.statistics;

        if (receivedHighScores != null)
        {
            highScores.Clear();
            foreach (var item in @receivedHighScores)
            {
                highScores.Add(item);
            }
        }
    }
}
