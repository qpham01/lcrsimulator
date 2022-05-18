using System;
using System.Windows.Controls;

namespace LCRSimulator.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private Image _winnerText;
        public Image WinnerText
        {
            get { return _winnerText; }
            set { _winnerText = value; OnPropertyChanged("WinnerText"); }    
        }

        private Image _playerImage;
        public Image PlayerImage
        {
            get { return _playerImage; }
            set { _playerImage = value; OnPropertyChanged("PlayerImage"); }
        }

        private string _playerIndex;
        public string PlayerIndex
        {
            get { return _playerIndex; }    
            set { _playerIndex = value; OnPropertyChanged("PlayerIndex"); }
        }

        public PlayerViewModel(Image playerImage, Image winnerText, string playerIndex)
        {
            PlayerImage = playerImage;
            WinnerText = winnerText;
            PlayerIndex = playerIndex;
        }
    }
}
