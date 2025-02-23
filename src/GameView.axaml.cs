using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;

using Newtonsoft.Json;

using Network;

namespace Client;

public partial class GameView : UserControl
{
    private const int updateInterval = 1;
    private const int alwaysWrongQuestionId = -1;

    private MainWindow mainWindow;

    private List<string> possibleAnswers;

    private int TimeLeftSeconds { get; set; }
    private int timePerQuestion;

    private int QuestionsLeftCount { get; set; }

    private Clock clock;

    public GameView(MainWindow mainWindow, int timePerQuestion, int totalQuestions)
    {
        this.mainWindow = mainWindow;

        QuestionsLeftCount = totalQuestions;
        InitializeComponent();

        this.timePerQuestion = timePerQuestion;
        this.TimeLeftSeconds = this.timePerQuestion + 1; // On the first question there's 1 more second because the clock subtracts it immediately.
        RequestQuestion();

        this.clock = new Clock(updateInterval, () => {
                this.TimeLeftSeconds -= updateInterval;
                if (this.TimeLeftSeconds < 0)
                {
                    AnswerAndUpdateQuestion(alwaysWrongQuestionId);
                }
        });

        DataContext = this;
    }

    public void LeaveGameButtonHandler(object sender, RoutedEventArgs args)
    {
        this.clock.Terminate();

        var communicator = Communicator.Instance;
        communicator.Request(RequestCode.GAME_LEAVE, new {});

        this.mainWindow.ChangeView(new MainMenuView(this.mainWindow));
    }

    private int ButtonToId(Button button)
    {
        // Not "elegant" but simple, understandable, and probably is faster than any other approach. And follows the KISS principle.
        if (button == possibleAnswer1) return 0;
        if (button == possibleAnswer2) return 1;
        if (button == possibleAnswer3) return 2;
        if (button == possibleAnswer4) return 3;

        throw new ArgumentException("Invalid button provided", nameof(button));
    }

    public void SubmitAnswerButtonHandler(object sender, RoutedEventArgs args)
    {
        var button = (Button)sender;
        AnswerAndUpdateQuestion(ButtonToId(button));
    }

    private void AnswerAndUpdateQuestion(int answerId)
    {
        SubmitAnswer(answerId);
        this.TimeLeftSeconds = this.timePerQuestion;
        if (this.QuestionsLeftCount > 0)
        {
            RequestQuestion();
        }
        else
        {
            this.clock.Terminate();
            this.mainWindow.ChangeView(new GameResultsView(this.mainWindow));
        }
    }

    private void RequestQuestion()
    {
        var communicator = Communicator.Instance;
        string jsonString = communicator.Request(RequestCode.QUESTION_GET, new {});
        var questionInfo = JsonConvert.DeserializeAnonymousType(
            jsonString,
            new {
                status = 0,
                question = "",
                answers = new List<List<object>>(),
            }
        );

        if (questionInfo == null)
            throw new ArgumentException("Network error"); // TODO: better error plz

        currentQuestion.Text = questionInfo.question;
        this.possibleAnswers = Utils.ExtractSecond<string>(questionInfo.answers);
        SyncAnswerContent();
    }

    private void SyncAnswerContent()
    {
        // @see the comment on simplicity vs elegancy inside ButtonToId method
        possibleAnswer1.Content = this.possibleAnswers[0];
        possibleAnswer2.Content = this.possibleAnswers[1];
        possibleAnswer3.Content = this.possibleAnswers[2];
        possibleAnswer4.Content = this.possibleAnswers[3];
    }

    private void SubmitAnswer(int answerId)
    {
        if (this.QuestionsLeftCount == 0)
        {
            return;
        }

        var communicator = Communicator.Instance;
        string jsonString = communicator.Request(RequestCode.SUBMIT_ANSWER, new {answerId});
        var correctAnswerId = JsonConvert.DeserializeAnonymousType(
            jsonString,
            new {
                status = 0,
                correctAnswerId = 0,
            }
        )?.correctAnswerId;

        if (correctAnswerId == null)
            throw new ArgumentException("Network error"); // TODO: better error plz

        this.QuestionsLeftCount--;

        if (answerId == correctAnswerId)
        {
            answerDisplay.IsVisible = true;
            answerDisplayColor.Fill = Brushes.Green;
            answerDisplayText.Text = "Correct!";
        }
        else 
        {
            answerDisplay.IsVisible = true;
            answerDisplayColor.Fill = Brushes.Red;

            answerDisplayText.Text = answerId == alwaysWrongQuestionId ? "You ran out of time." : "Incorrect." +
                $" The correct answer was: {possibleAnswers[(int)correctAnswerId]}";
        }
    }
}
