using System;
using System.Collections.Generic;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Newtonsoft.Json;

using Network;

namespace Client;

public partial class UserStatisticsView : UserControl
{
    private MainWindow mainWindow;

    public UserStatisticsView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;

        statisticsText.Text = string.Join("\n", GetStatistics());
    }

    private List<string> GetStatistics()
    {
        var communicator = Communicator.Instance;
        var jsonString = communicator.Request(RequestCode.GET_PERSONAL_STATISTICS, new {});
        var statistics = JsonConvert.DeserializeAnonymousType(
                jsonString,
                new { status = 0, statistics = new List<string>() }
            )?.statistics;
        return statistics ?? new List<string>();
    }
    public void BackToMenuButtonHandler(object sender, RoutedEventArgs args)
    {
        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }
}
