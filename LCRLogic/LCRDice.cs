using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCRLogic
{
    public interface ILCRDice
    {
        string[] Roll(int count);
    }

    public class LCRDice : ILCRDice
    {
        public const string Dot = ".";
        public const string L = "L";
        public const string C = "C";
        public const string R = "R";

        private string[] _diceValues = new string[] { Dot, Dot, Dot, L, C, R };
        private Random _randomizer;

        public LCRDice()
        {
            _randomizer = new Random();
        }

        public string[] Roll(int count)
        {
            var results = new List<string>();
            for (var i = 0; i < count; ++i)
            {
                var roll = _randomizer.Next(0, 6);
                results.Add(_diceValues[roll]);
            }
            return results.ToArray();
        }
    }
}
