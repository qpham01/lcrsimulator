using LCRLogic;
using LCRSimulator.Helpers;
using OxyPlot;
using OxyPlot.Annotations;
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
            UpdatePlayerImages(count, -1); 
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

    private PlotController _controller;
    public PlotController Controller
    {
        get { return _controller; }
        set { _controller = value; OnPropertyChanged("Controller"); }
    }

    public ICommand PlayCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }
    public DelegatePlotCommand<OxyMouseEventArgs> LeftMouseCommand { get; private set; }

    private LineSeries _lineSeries;

    private OxyColor _maxColor;
    private OxyColor _minColor;
    private OxyColor _gameColor;
    private OxyColor _averageColor;

    private const double _yExtent = 1.15;
    private const double _xExtent = 1.1;
    private const string _maxRgb = "#F9BB17";
    private const string _minRgb = "#A21BCD";
    private const string _gameRgb = "#E51021";
    private const string _averageRgb = "#148F3E";
    private const int _minMaxMarkerSize = 5;
    private const int _averageThickness = 2;
    private const int _lineThickness = 4;
    private const double _annotationSize = 20;

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
        LeftMouseCommand = new DelegatePlotCommand<OxyMouseEventArgs>(HandleLeftMouseClick);

        _maxColor = OxyColor.Parse(_maxRgb);
        _minColor = OxyColor.Parse(_minRgb);
        _gameColor = OxyColor.Parse(_gameRgb);
        _averageColor = OxyColor.Parse(_averageRgb);
    }

    private void HandleLeftMouseClick(IView view, IController controller, OxyMouseEventArgs eventArgs)
    {
        var point = _lineSeries.InverseTransform(eventArgs.Position);
        var gameIndex = Math.Clamp((int)point.X, 0, GamesSimulated - 1);
        var game = _games[gameIndex];
        if (game.Winner != null)
        {
            UpdatePlayerImages(int.Parse(_playerCount), game.Winner.Index);
        }            
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
            var percentCompleted = GamesSimulated * 1000 / gameCount;
            _simulationWorker.ReportProgress(percentCompleted);
            if (percentCompleted > lastPercentage)
            {
                lastPercentage = percentCompleted;
                Thread.Sleep(1);
            }
        }
        IsSimulating = false;
        UpdatePlot();
    }

    private void UpdatePlot()
    {
        var model = new PlotModel();
        var lineData = new List<DataPoint>();
        var scatterData = new List<ScatterPoint>();
        var min = int.MaxValue;
        var max = int.MinValue;
        var maxIndex = -1;
        var minIndex = -1;
        var total = 0;
        for (var i = 0; i < GamesSimulated; ++i)
        {
            var game = _games[i];
            var point = new DataPoint(i + 1, game.TurnCount);
            var scatter = new ScatterPoint(i + 1, game.TurnCount);
            if (game.TurnCount > max)
            {
                max = game.TurnCount;
                maxIndex = i;
            }
            if (game.TurnCount < min)
            {
                min = game.TurnCount;
                minIndex = i;
            } 
            total += game.TurnCount;
            lineData.Add(point);
            scatterData.Add(scatter);
        }
        _lineSeries = new LineSeries
        {
            Title = "Game",
            ItemsSource = lineData,
            DataFieldX = "Game Index",
            DataFieldY = "Turn Count",
            Color = _gameColor,
            MarkerSize = 0,
            MarkerType = MarkerType.Circle,
            StrokeThickness = _lineThickness,
        };
        var maxPoint = new ScatterPoint(maxIndex + 1, max);
        var maxSeries = new ScatterSeries
        {
            Title = "Longest",
            ItemsSource = new List<ScatterPoint> { maxPoint },
            DataFieldX = "Game Index",
            DataFieldY = "Turn Count",
            MarkerSize = _minMaxMarkerSize,
            MarkerFill = _maxColor,
            MarkerStroke = _maxColor,
            MarkerStrokeThickness = 1,
            MarkerType = MarkerType.Circle,
        };
        var minPoint = new ScatterPoint(minIndex + 1, min);
        var minSeries = new ScatterSeries
        {
            Title = "Shortest",
            ItemsSource = new List<ScatterPoint> { minPoint } ,
            DataFieldX = "Game Index",
            DataFieldY = "Turn Count",
            MarkerSize = _minMaxMarkerSize,
            MarkerFill = _minColor,
            MarkerStroke = _minColor,
            MarkerStrokeThickness = 1,
            MarkerType = MarkerType.Circle,
        };        
        var average = (double)total / (double)GamesSimulated;
        var minX = -GamesSimulated * (_xExtent - 1);
        var maxX = GamesSimulated * _xExtent;

        var averageSeries = new LineSeries
        {
            Title = "Average",
            ItemsSource = new List<DataPoint> { new DataPoint(minX, average), new DataPoint(maxX, average) },
            DataFieldX = "Time",
            DataFieldY = "Value",
            Color = _averageColor,
            MarkerSize = 0,
            StrokeThickness = _averageThickness,
        };

        model.Series.Add(_lineSeries);
        model.Series.Add(averageSeries);
        model.Series.Add(maxSeries);
        model.Series.Add(minSeries);

        var yMin = Math.Min(min - 6, 0);

        var yAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = yMin, Maximum = max * _yExtent, Title = "Turns" };
        model.Axes.Add(yAxis);
        model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = minX, Maximum = maxX, Title = "Games" });

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
        var range = max - min;
        var maxOffset = 0.03 * range;
        var minOffset = 0.03 * range;
        var labelMax = new TextAnnotation
        {
            Text = $"Longest ({max})",
            FontSize = _annotationSize,
            FontWeight = OxyPlot.FontWeights.Bold,
            TextPosition = new DataPoint(maxIndex, max + maxOffset),
            TextColor = _maxColor,
            StrokeThickness = 0,
            Stroke = OxyColor.FromArgb(0, 240, 240, 40)
        };
        var labelMin = new TextAnnotation
        {
            Text = $"Shortest ({min})",
            FontSize = _annotationSize,
            FontWeight = OxyPlot.FontWeights.Bold,
            TextPosition = new DataPoint(minIndex, min + minOffset),
            TextColor = _minColor,
            StrokeThickness = 0,
            Stroke = OxyColor.FromArgb(0, 240, 40, 240)
        };
        model.Annotations.Add(labelMax);
        model.Annotations.Add(labelMin);

        Model = model;

        var controller = new PlotController();
        controller.UnbindAll();
        controller.BindMouseDown(OxyMouseButton.Left, LeftMouseCommand);
        Controller = controller;
    }

}