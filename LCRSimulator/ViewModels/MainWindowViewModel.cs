﻿using LCRLogic;
using LCRSimulator.Helpers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LCRSimulator.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private int[] _playerCountPresets = new int[] { 3, 4, 5, 5, 5, 5, 6, 7 };
    private int[] _gameCountPresets = new int[] { 100, 100, 100, 1000, 10000, 100000, 100, 100 };
    private ImageSource _playerImageSource;
    private ImageSource _winnerImageSource;
    private ImageSource _blankTextImageSource;
    private ImageSource _winnerTextImageSource;
    private List<LCRGame> _games = new List<LCRGame>();
    private BackgroundWorker _simulationWorker;
    private const double _yExtent = 1.1;
    private const double _xExtent = 1.05;

    public int PlayerRowHeight => 150;

    public ObservableCollection<string> GamePresets { get; set; }

    private int _selectedGamePresetIndex;
    public int SelectedGamePresetIndex
    {
        get { return _selectedGamePresetIndex; }
        set 
        {
            _selectedGamePresetIndex = value; 
            OnPropertyChanged("SelectedGamePresetIndex");

            PlayerCount = _playerCountPresets[_selectedGamePresetIndex].ToString();
            GameCount = _gameCountPresets[_selectedGamePresetIndex].ToString();                
        }
    }

    private int _winnerIndex = 1;
    public int WinnerIndex
    {
        get { return _winnerIndex; }
        set { _winnerIndex = value; OnPropertyChanged("WinnerIndex"); }
    }

    private string _playerCount = string.Empty;
    public string PlayerCount
    {
        get { return _playerCount; }
        set 
        {
            _playerCount = value;
            if (!int.TryParse(_playerCount, out var count))
            {
                _playerCount = "0";
            }
            OnPropertyChanged("PlayerCount"); 
            UpdatePlayerImages(count, WinnerIndex); 
        }
    }

    private string _gameCount = string.Empty;
    public string GameCount
    {
        get { return _gameCount; }
        set { _gameCount = value; OnPropertyChanged("GameCount"); }
    }

    private ObservableCollection<PlayerViewModel> _playerViewModels = new ObservableCollection<PlayerViewModel>();
    public ObservableCollection<PlayerViewModel> PlayerViewModels
    {
        get { return _playerViewModels; }
        set { _playerViewModels = value; OnPropertyChanged("PlayerViewModel"); }
    }

    private int _gamesSimulated;
    public int GamesSimulated
    {
        get { return _gamesSimulated; }
        private set
        {
            if (_gamesSimulated != value)
            {
                _gamesSimulated = value;
                OnPropertyChanged("GamesSimulated");
            }
        }
    }

    private int _currentProgress;
    public int CurrentProgress
    {
        get { return _currentProgress; }
        private set
        {
            if (_currentProgress != value)
            {
                _currentProgress = value;
                OnPropertyChanged("CurrentProgress");
            }
        }
    }


    private bool _isSimulating;
    public bool IsSimulating 
    {
        get { return _isSimulating; }
        set { _isSimulating = value; OnPropertyChanged("IsSimulating"); }
    }

    private PlotModel _model;
    public PlotModel Model
    {
        get { return _model; }
        set { _model = value; OnPropertyChanged("Model"); }
    }

    public ICommand PlayCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }

    public MainWindowViewModel()
    {
        _simulationWorker = new BackgroundWorker();
        _simulationWorker.DoWork += Simulate;
        _simulationWorker.ProgressChanged += ProgressChanged;
        _simulationWorker.WorkerReportsProgress = true;
        _simulationWorker.WorkerSupportsCancellation = true;

        var imageSourceConverter = new ImageSourceConverter();
        _playerImageSource = imageSourceConverter.ConvertFromString("pack://application:,,,/Images/player.png") as ImageSource;
        _winnerImageSource = imageSourceConverter.ConvertFromString("pack://application:,,,/Images/winner.png") as ImageSource;
        _blankTextImageSource = imageSourceConverter.ConvertFromString("pack://application:,,,/Images/blankText.png") as ImageSource;
        _winnerTextImageSource = imageSourceConverter.ConvertFromString("pack://application:,,,/Images/winnerText.png") as ImageSource;

        var gamePresets = new ObservableCollection<string>();
        for (var i = 0; i < _playerCountPresets.Length; ++i)
        {
            var preset = $"{_playerCountPresets[i]} players X {_gameCountPresets[i]} games";
            gamePresets.Add(preset);
        }
        GamePresets = gamePresets;

        PlayCommand = new RelayCommand(x => StartSimulation(x));
        CancelCommand = new RelayCommand(x => CancelSimulation(x));


    }

    private void CancelSimulation(object x)
    {
        IsSimulating = false;
    }

    private void StartSimulation(object x)
    {
        _simulationWorker.RunWorkerAsync();
    }

    private void UpdatePlayerImages(int playerCount, int winnerIndex)
    {
        PlayerViewModels.Clear();
        for (var i = 0; i < playerCount; ++i)
        {
            var image = new Image();
            image.Source = i == winnerIndex ? _winnerImageSource : _playerImageSource;
            var text = new Image();
            text.Source = i == winnerIndex ? _winnerTextImageSource : _blankTextImageSource;
            PlayerViewModels.Add(new PlayerViewModel(image, text, (i + 1).ToString()));
        }
        OnPropertyChanged("PlayerViewModels");
    }
    private void ProgressChanged(object? sender, ProgressChangedEventArgs? e)
    {
        if (e != null)
            CurrentProgress = e.ProgressPercentage;
    }

    private async void Simulate(object? sender, DoWorkEventArgs? e)
    {
        IsSimulating = false;
        if (!int.TryParse(GameCount, out var gameCount) || gameCount <= 0)
        {
            MessageBox.Show($"Invalid game count {gameCount}", "Error");
            return;
        }
        if (!int.TryParse(PlayerCount, out var playerCount) || playerCount <= 0)
        {
            MessageBox.Show($"Invalid player count {playerCount}", "Error");
            return;
        }

        _games.Clear();
        for (var i = 0; i < gameCount; ++i)
        {
            _games.Add(new LCRGame(new LCRDice()));
        }
        IsSimulating = true;
        GamesSimulated = 0;
        CurrentProgress = 0;
        var lastPercentage = 0;
        while (GamesSimulated < gameCount && IsSimulating)
        {
            var game = _games[GamesSimulated];
            game.PlayGame(playerCount);
            GamesSimulated++;
            var percentCompleted = GamesSimulated * 1000 / gameCount ;            
            _simulationWorker.ReportProgress(percentCompleted);
            if (percentCompleted > lastPercentage)
            {
                lastPercentage = percentCompleted;
                Thread.Sleep(1);
            }
        }
        IsSimulating = false;

        var model = new PlotModel();
        var lineData = new List<DataPoint>();
        var scatterData = new List<ScatterPoint>();
        var min = int.MaxValue;
        var max = int.MinValue;
        var total = 0;
        for (var i = 0; i < GamesSimulated; ++i)
        {
            var game = _games[i];
            var point = new DataPoint(i + 1, game.TurnCount);
            var scatter = new ScatterPoint(i + 1, game.TurnCount);
            if (game.TurnCount > max) max = game.TurnCount;
            if (game.TurnCount < min) min = game.TurnCount;
            total += game.TurnCount;
            lineData.Add(point);
            scatterData.Add(scatter);
        }
        var lineSeries = new LineSeries
        {
            Title = "Game",
            ItemsSource = lineData,
            DataFieldX = "Game Index",
            DataFieldY = "Turn Count",
            Color = OxyColor.Parse("#AF4C50"),
            MarkerSize = 3,
            MarkerFill = OxyColor.Parse("#AF4C50"),
            MarkerStroke = OxyColor.Parse("#AF4C50"),
            MarkerStrokeThickness = 1.5,
            MarkerType = MarkerType.Circle,
            StrokeThickness = 1,
        };
        var scatterSeries = new ScatterSeries
        {
            Title = "Game",
            ItemsSource = scatterData,
            DataFieldX = "Game Index",
            DataFieldY = "Turn Count",
            MarkerSize = 3,
            MarkerFill = OxyColor.Parse("#AF4C50"),
            MarkerStroke = OxyColor.Parse("#AF4C50"),
            MarkerStrokeThickness = 1.5,
            MarkerType = MarkerType.Circle,
        };

        // TODO: Get decision on line or scatter plot.
        // model.Series.Add(lineSeries);
        model.Series.Add(scatterSeries);

        var average = (double)total / (double)GamesSimulated;
        var averageSeries = new LineSeries
        {
            Title = "Average",
            ItemsSource = new List<DataPoint> { new DataPoint(0, average), new DataPoint(GamesSimulated * _xExtent, average) },
            DataFieldX = "Time",
            DataFieldY = "Value",
            Color = OxyColor.Parse("#4CAF50"),
            MarkerSize = 0,
            MarkerFill = OxyColor.Parse("#FFFFFFFF"),
            MarkerStroke = OxyColor.Parse("#4CAF50"),
            MarkerStrokeThickness = 0,
            MarkerType = MarkerType.Circle,
            StrokeThickness = 1,
        };
        model.Series.Add(averageSeries);

        model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = max * _yExtent, Title = "Turns" });
        model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = GamesSimulated * _xExtent, Title = "Games" });

        var legend = new Legend
        {
            LegendBorder = OxyColors.Black,
            LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
            LegendPosition = LegendPosition.TopRight,
            LegendPlacement = LegendPlacement.Inside,
            LegendOrientation = LegendOrientation.Vertical,
            LegendItemOrder = LegendItemOrder.Normal,
            LegendItemAlignment = OxyPlot.HorizontalAlignment.Left,
            LegendSymbolPlacement = LegendSymbolPlacement.Left,
        };
        model.Legends.Add(legend);
        Model = model;
    }

}