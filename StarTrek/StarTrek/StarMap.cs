using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarTrek
{
    [Serializable()]
    public class Quadrant
    {
        readonly Piece[,] _sectors;

        public Quadrant()
        {

            _sectors = new Piece[8, 8];

        }

        public Piece[,] Sector
        {
            get
            {
                return _sectors;
            }

        }
  
        public void Init(int qX, int qY)
        {
            for (var sX = 0; sX < 8; sX++)
            {
                for (var sY = 0; sY < 8; sY++)
                {
                    _sectors[sX, sY] = new Piece(Piece.Pieces.empty, qX, qY, sX, sY);
                }
            }

        }
    }

    [Serializable()]
    public class StarMap
    {
        readonly Quadrant[,] _quadrants;
        readonly Piece _enterprise;
        readonly List<Piece> _klingons = new List<Piece>();
        readonly List<Piece> _stars = new List<Piece>();
        readonly List<Piece> _stardocs = new List<Piece>();

        public StarMap()
        {
            _quadrants = new Quadrant[8, 8];

            InitMap();

            _enterprise = PutPiece(GetPiece(Piece.Pieces.enterprise));

            AddPieces(_stars, Piece.Pieces.star, 256);
            AddPieces(_klingons, Piece.Pieces.klingon, 30);
            AddPieces(_stardocs, Piece.Pieces.stardock, 3);

        }

        public Quadrant[,] Quadrants
        { 
            get {
                return _quadrants;
            }

        }

        public Piece Enterprise => _enterprise;

        public List<Piece> Klingons => _klingons;

        public List<Piece> Stars => _stars;

        public List<Piece> Stardocs => _stardocs;

        public void InitMap()
        {
            for (var qX = 0; qX < 8; qX++)
            {
                for (var qY = 0; qY < 8; qY++)
                {
                    _quadrants[qX, qY] = new Quadrant();
                    _quadrants[qX, qY].Init(qX, qY);

                }
            }
        }

        public void MoveEnterprise(int qX, int qY, int sX, int sY)
        {

            if (_quadrants[qX, qY].Sector[sX, sY].Type != Piece.Pieces.empty)
            {
                Random rnd = new Random();

                do
                {
                    sX = rnd.Next(0, 8);
                    sY = rnd.Next(0, 8);

                } while (_quadrants[qX, qY].Sector[sX, sY].Type != Piece.Pieces.empty);

            }

            PutPiece(new Piece(Piece.Pieces.empty,
            Enterprise.QX, Enterprise.QY, Enterprise.SX, Enterprise.SY));

            Enterprise.QX = qX;
            Enterprise.QY = qY;
            Enterprise.SX = sX;
            Enterprise.SY = sY;

            PutPiece(Enterprise);

        }

        public String GetQuadrantSummary(int qX, int qY)
        {
            int stardocks = 0;
            int klingons = 0;
            int stars = 0;

            for (var sX = 0; sX < 8; sX++)
            {

                for (var sY = 0; sY < 8; sY++)
                {
                    Piece piece = _quadrants[qX, qY].Sector[sX, sY];

                    switch (piece.Type)
                    {
                        case Piece.Pieces.klingon:
                            klingons += 1;
                            break;
                        case Piece.Pieces.star:
                            stars += 1;
                            break;
                        case Piece.Pieces.stardock:
                            stardocks += 1;
                            break;
                    }

                }

            }

            return $"{stars}{klingons}{stardocks}";

        }

        public int CheckQuadrant(Piece.Pieces type, int qX, int qY)
        {
            int counter = 0;
  
            for (var sX = 0; sX < 8; sX++)
            {

                for (var sY = 0; sY < 8; sY++)
                {
                    Piece piece = _quadrants[qX, qY].Sector[sX, sY];

                    if (piece.Type == type)
                    {
                        counter += 1;
                    }

                }

            }

            return counter;

        }

        private bool CheckSector(Piece piece)
        {

            return _quadrants[piece.QX, piece.QY].Sector[piece.SX, piece.SY].Type == Piece.Pieces.empty;

        }

        private void AddPieces(List<Piece> pieces, Piece.Pieces type, int max)
        {
            for (int iPiece = 0; iPiece < max; iPiece++)
            {
                Piece piece = GetPiece(type);

                pieces.Add(piece);
                PutPiece(GetPiece(type));

   
            }

        }

        private Piece GetPiece(Piece.Pieces type)
        {
            Random rnd = new Random();

            do
            {
                var qX = rnd.Next(0, 8);
                var qY = rnd.Next(0, 8);
                var sX = rnd.Next(0, 8);
                var sY = rnd.Next(0, 8);

                if ((_quadrants[qX, qY].Sector[sX, sY].Type == Piece.Pieces.empty) &&
                    (CheckQuadrant(type, qX, qY) <= 9))
                { 
 
                    Piece piece = new Piece(type, qX, qY, sX, sY);
                    return piece;
                }

            } while (true);

        }

        private Piece PutPiece(Piece piece)
        {

            _quadrants[piece.QX, piece.QY].Sector[piece.SX, piece.SY] = piece;
            Console.WriteLine($"PUT: {piece.Type} - {piece.QX}:{piece.QY} {piece.SX}:{piece.SY}");
            return piece;

        }

    }

}
