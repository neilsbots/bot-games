using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarTrek
{
    [Serializable()]
    public class Piece
    {
        public enum Pieces { empty, enterprise, klingon, stardock, star}
        int _qX = 0;
        int _qY = 0;
        int _sX = 0;
        int _sY = 0;

        int _energy = -1;
        int _status = 1;
        int _scanned = 1;
        readonly Pieces _type;

        public Piece(Pieces type, int qX, int qY, int sX, int sY)
        {
            _type = type;
            QX = qX;
            QY = qY;
            SX = sX;
            SY = sY;

        }

        public int Energy { get => _energy; set => _energy = value; }
        public int Status { get => _status; set => _status = value; }
        public int Scanned { get => _scanned; set => _scanned = value; }
        public int QX { get => _qX; set => _qX = value; }
        public int QY { get => _qY; set => _qY = value; }
        public int SX { get => _sX; set => _sX = value; }
        public int SY { get => _sY; set => _sY = value; }

        public Pieces Type => _type;
    }

}
