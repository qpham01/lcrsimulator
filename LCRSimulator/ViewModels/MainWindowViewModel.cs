using LCRLogic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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

    public MainWindowViewModel()
    {
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
}