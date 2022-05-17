using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCRLogic.Tests
{
    internal class LCRFixedDice : ILCRDice
    {
        public string[] _results;
        public int _nextResult;

        public LCRFixedDice(string[] results)
        {
            _results = results.ToArray();
            _nextResult = 0;
        }

        public string[] Roll(int count)
        {
            var newResults = new List<string>();
            for (var i = 0; i < count; ++i)
            {
                if (_nextResult >= _results.Length)
                    throw new InvalidOperationException("Not enough results to return");
                newResults.Add(_results[_nextResult]);
                _nextResult++;
            }
            return newResults.ToArray();
        }
    }
}
