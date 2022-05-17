using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCRLogic
{
    public class LCRPlayer
    {
        public int Index => _index;
        public int ChipCount => _chipCount;

        public void AddChips(int chips)
        {
            _chipCount += chips;
        }

        public void RemoveChip()
        {
            if (_chipCount > 0) _chipCount--;
            else throw new InvalidOperationException("No chip to remove");
        }

        private readonly int _index;
        private int _chipCount;

        public LCRPlayer(int index)
        {
            _index = index;
        }

    }
}
