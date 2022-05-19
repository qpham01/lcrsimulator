namespace LCRLogic
{
    public interface ILCRGame
    {
        LCRPlayer? Winner { get; }
        int TurnCount { get; }
        void PlayGame(int playercCount);
        bool IsWinner(LCRPlayer player);
    }

    public class LCRGame : ILCRGame
    {
        private const int _startingChips = 3;
        private int _playerCount;
        private List<LCRPlayer> _players = new List<LCRPlayer>();
        private int _turnCount = -1;
        private int _centerPot = 0;
        private int _chipTotal = 0;
        private readonly ILCRDice _dice;

        public LCRPlayer? Winner { get; private set; }

        public int TurnCount => _turnCount;
        public int CenterPot => _centerPot;

        public LCRGame(ILCRDice dice)
        {
            _dice = dice;        
        }

        public void PlayGame(int playerCount)
        {
            Winner = null;
            _turnCount = 0;
            _playerCount = playerCount;
            _centerPot = 0;
            _chipTotal = playerCount * _startingChips;
            _players.Clear();

            for (var i = 0; i < _playerCount; ++i)
            {
                var player = new LCRPlayer(i);
                player.AddChips(_startingChips);
                _players.Add(player);
            }

            var playerIndex = 0;

            while (true)
            {
                _turnCount++;
                var player = _players[playerIndex];
                if (IsWinner(player))
                    break;

                if (player.ChipCount > 0)
                {
                    var rollCount = Math.Min(player.ChipCount, 3);
                    var results = _dice.Roll(rollCount);
                    foreach (var result in results)
                    {
                        if (result == LCRDice.Dot) continue;
                        player.RemoveChip();
                        switch (result)
                        {
                            case LCRDice.L: 
                                GetLeftPlayer(playerIndex).AddChips(1);
                                break;
                            case LCRDice.R:
                                GetRightPlayer(playerIndex).AddChips(1);
                                break;
                            case LCRDice.C:
                                _centerPot++;
                                break;
                        }
                    }
                }
                playerIndex++;
                if (playerIndex >= _playerCount) playerIndex = 0;
            }
        }

        private LCRPlayer GetRightPlayer(int playerIndex)
        {
            var rightIndex = playerIndex + 1;
            if (rightIndex >= _playerCount) rightIndex = 0;
            return _players[rightIndex];
        }

        private LCRPlayer GetLeftPlayer(int playerIndex)
        {
            var leftIndex = playerIndex - 1;
            if (leftIndex < 0) leftIndex = _playerCount - 1;
            return _players[leftIndex];
        }

        public bool IsWinner(LCRPlayer player)
        {
            if (player.ChipCount == _chipTotal - _centerPot)
            {
                Winner = player;
                return true;
            }
            return false;
        }
    }
}