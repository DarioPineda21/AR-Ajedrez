using System;
using System.Text;

namespace ChessCore
{
    public sealed partial class Board
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public const int WhiteKingSide = 1;
        public const int WhiteQueenSide = 2;
        public const int BlackKingSide = 4;
        public const int BlackQueenSide = 8;

        public sbyte[] Sq = new sbyte[64];
        public bool WhiteToMove = true;
        public int Castling;        // suma de los derechos de enroque
        public int EpSquare = -1;   // casilla de captura al paso, o -1
        public int HalfMove;        // para la regla de las 50 jugadas
        public int FullMove = 1;

        static readonly int[] CastleMask = BuildCastleMask();

        static int[] BuildCastleMask()
        {
            int[] m = new int[64];
            for (int i = 0; i < 64; i++) m[i] = 15;
            m[0] = 15 & ~WhiteQueenSide;
            m[4] = 15 & ~(WhiteKingSide | WhiteQueenSide);
            m[7] = 15 & ~WhiteKingSide;
            m[56] = 15 & ~BlackQueenSide;
            m[60] = 15 & ~(BlackKingSide | BlackQueenSide);
            m[63] = 15 & ~BlackKingSide;
            return m;
        }

        // Direcciones: primero 4 ortogonales y luego 4 diagonales
        static readonly int[] DirDF = { 1, -1, 0, 0, 1, 1, -1, -1 };
        static readonly int[] DirDR = { 0, 0, 1, -1, 1, -1, 1, -1 };
        static readonly int[] KnightDF = { 1, 2, 2, 1, -1, -2, -2, -1 };
        static readonly int[] KnightDR = { 2, 1, -1, -2, -2, -1, 1, 2 };
        static readonly int[] KingDF = { 1, 1, 1, 0, 0, -1, -1, -1 };
        static readonly int[] KingDR = { 1, 0, -1, 1, -1, 1, 0, -1 };

        public static Board Start()
        {
            return FromFen(StartFen);
        }

        public Board Clone()
        {
            Board b = new Board();
            Array.Copy(Sq, b.Sq, 64);
            b.WhiteToMove = WhiteToMove;
            b.Castling = Castling;
            b.EpSquare = EpSquare;
            b.HalfMove = HalfMove;
            b.FullMove = FullMove;
            return b;
        }

        // ---------- FEN ----------

        public static Board FromFen(string fen)
        {
            if (fen == null) throw new ArgumentException("FEN vacio");
            string[] parts = fen.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) throw new ArgumentException("FEN incompleto: " + fen);

            Board b = new Board();
            int rank = 7, file = 0;
            foreach (char c in parts[0])
            {
                if (c == '/') { rank--; file = 0; }
                else if (c >= '1' && c <= '8') { file += c - '0'; }
                else
                {
                    sbyte p = Piece.FromChar(c);
                    if (p == Piece.None || rank < 0 || file > 7) throw new ArgumentException("FEN invalido: " + fen);
                    b.Sq[rank * 8 + file] = p;
                    file++;
                }
            }

            b.WhiteToMove = parts[1] == "w";

            foreach (char c in parts[2])
            {
                if (c == 'K') b.Castling |= WhiteKingSide;
                else if (c == 'Q') b.Castling |= WhiteQueenSide;
                else if (c == 'k') b.Castling |= BlackKingSide;
                else if (c == 'q') b.Castling |= BlackQueenSide;
            }

            b.EpSquare = parts[3] == "-" ? -1 : Chess.ParseSquare(parts[3]);
            if (parts.Length > 4) int.TryParse(parts[4], out b.HalfMove);
            if (parts.Length > 5) int.TryParse(parts[5], out b.FullMove);

            if (b.KingSquare(true) < 0 || b.KingSquare(false) < 0)
                throw new ArgumentException("FEN sin rey: " + fen);
            return b;
        }

        public string ToFen()
        {
            StringBuilder sb = new StringBuilder();
            for (int r = 7; r >= 0; r--)
            {
                int empty = 0;
                for (int f = 0; f < 8; f++)
                {
                    sbyte p = Sq[r * 8 + f];
                    if (p == 0) { empty++; continue; }
                    if (empty > 0) { sb.Append(empty); empty = 0; }
                    sb.Append(Piece.ToChar(p));
                }
                if (empty > 0) sb.Append(empty);
                if (r > 0) sb.Append('/');
            }
            sb.Append(WhiteToMove ? " w " : " b ");
            if (Castling == 0) sb.Append('-');
            else
            {
                if ((Castling & WhiteKingSide) != 0) sb.Append('K');
                if ((Castling & WhiteQueenSide) != 0) sb.Append('Q');
                if ((Castling & BlackKingSide) != 0) sb.Append('k');
                if ((Castling & BlackQueenSide) != 0) sb.Append('q');
            }
            sb.Append(' ');
            sb.Append(EpSquare < 0 ? "-" : ChessCore.Sq.Name(EpSquare));
            sb.Append(' ').Append(HalfMove).Append(' ').Append(FullMove);
            return sb.ToString();
        }

        // ---------- Jugar y deshacer ----------

        public UndoInfo MakeMove(Move m)
        {
            UndoInfo u = new UndoInfo();
            u.Move = m;
            u.Castling = Castling;
            u.Ep = EpSquare;
            u.Half = HalfMove;

            int from = m.From, to = m.To;
            bool white = WhiteToMove;
            sbyte piece = Sq[from];
            sbyte captured = Sq[to];

            if ((m.Flags & MoveFlags.EnPassant) != 0)
            {
                int capSq = white ? to - 8 : to + 8;
                captured = Sq[capSq];
                Sq[capSq] = 0;
            }
            u.Captured = captured;

            Sq[to] = piece;
            Sq[from] = 0;
            if (m.Promo != 0) Sq[to] = Piece.Make(m.Promo, white);

            if ((m.Flags & MoveFlags.CastleKing) != 0)
            {
                Sq[to - 1] = Sq[to + 1];
                Sq[to + 1] = 0;
            }
            else if ((m.Flags & MoveFlags.CastleQueen) != 0)
            {
                Sq[to + 1] = Sq[to - 2];
                Sq[to - 2] = 0;
            }

            Castling &= CastleMask[from] & CastleMask[to];
            EpSquare = (m.Flags & MoveFlags.DoublePush) != 0 ? (from + to) / 2 : -1;

            if (Piece.Type(piece) == Piece.Pawn || captured != 0) HalfMove = 0; else HalfMove++;
            if (!white) FullMove++;
            WhiteToMove = !white;
            return u;
        }

        public void UnmakeMove(UndoInfo u)
        {
            Move m = u.Move;
            int from = m.From, to = m.To;

            WhiteToMove = !WhiteToMove;
            bool white = WhiteToMove;
            if (!white) FullMove--;

            sbyte piece = Sq[to];
            if (m.Promo != 0) piece = Piece.Make(Piece.Pawn, white);
            Sq[from] = piece;

            if ((m.Flags & MoveFlags.EnPassant) != 0)
            {
                Sq[to] = 0;
                Sq[white ? to - 8 : to + 8] = u.Captured;
            }
            else
            {
                Sq[to] = u.Captured;
            }

            if ((m.Flags & MoveFlags.CastleKing) != 0)
            {
                Sq[to + 1] = Sq[to - 1];
                Sq[to - 1] = 0;
            }
            else if ((m.Flags & MoveFlags.CastleQueen) != 0)
            {
                Sq[to - 2] = Sq[to + 1];
                Sq[to + 1] = 0;
            }

            Castling = u.Castling;
            EpSquare = u.Ep;
            HalfMove = u.Half;
        }

        // ---------- Ataques y jaque ----------

        public int KingSquare(bool white)
        {
            sbyte k = Piece.Make(Piece.King, white);
            for (int i = 0; i < 64; i++) if (Sq[i] == k) return i;
            return -1;
        }

        // Verdadero si la casilla esta atacada por alguna pieza del color indicado
        public bool IsAttacked(int sq, bool byWhite)
        {
            int f = sq & 7, r = sq >> 3;

            sbyte pawn = Piece.Make(Piece.Pawn, byWhite);
            int pr = byWhite ? r - 1 : r + 1;
            if (pr >= 0 && pr < 8)
            {
                if (f > 0 && Sq[pr * 8 + f - 1] == pawn) return true;
                if (f < 7 && Sq[pr * 8 + f + 1] == pawn) return true;
            }

            sbyte knight = Piece.Make(Piece.Knight, byWhite);
            for (int i = 0; i < 8; i++)
            {
                int nf = f + KnightDF[i], nr = r + KnightDR[i];
                if (nf < 0 || nf > 7 || nr < 0 || nr > 7) continue;
                if (Sq[nr * 8 + nf] == knight) return true;
            }

            sbyte king = Piece.Make(Piece.King, byWhite);
            for (int i = 0; i < 8; i++)
            {
                int nf = f + KingDF[i], nr = r + KingDR[i];
                if (nf < 0 || nf > 7 || nr < 0 || nr > 7) continue;
                if (Sq[nr * 8 + nf] == king) return true;
            }

            sbyte queen = Piece.Make(Piece.Queen, byWhite);
            sbyte rook = Piece.Make(Piece.Rook, byWhite);
            sbyte bishop = Piece.Make(Piece.Bishop, byWhite);
            for (int d = 0; d < 8; d++)
            {
                sbyte slider = d < 4 ? rook : bishop;
                int nf = f + DirDF[d], nr = r + DirDR[d];
                while (nf >= 0 && nf < 8 && nr >= 0 && nr < 8)
                {
                    sbyte p = Sq[nr * 8 + nf];
                    if (p != 0)
                    {
                        if (p == queen || p == slider) return true;
                        break;
                    }
                    nf += DirDF[d];
                    nr += DirDR[d];
                }
            }
            return false;
        }

        public bool InCheck()
        {
            return IsAttacked(KingSquare(WhiteToMove), !WhiteToMove);
        }

        // ---------- Estado de la partida ----------

        public bool HasInsufficientMaterial()
        {
            int minors = 0;
            for (int i = 0; i < 64; i++)
            {
                int t = Piece.Type(Sq[i]);
                if (t == Piece.Pawn || t == Piece.Rook || t == Piece.Queen) return false;
                if (t == Piece.Knight || t == Piece.Bishop) minors++;
            }
            return minors <= 1;
        }

        public GameStatus GetStatus()
        {
            return GetStatus(LegalMoves());
        }

        public GameStatus GetStatus(System.Collections.Generic.List<Move> legal)
        {
            if (legal.Count == 0) return InCheck() ? GameStatus.Checkmate : GameStatus.Stalemate;
            if (HasInsufficientMaterial()) return GameStatus.DrawMaterial;
            if (HalfMove >= 100) return GameStatus.DrawFiftyMoves;
            return GameStatus.Playing;
        }
    }

    // Auxiliares que no caben en Board
    public static class Chess
    {
        public static int ParseSquare(string s) { return Sq.Parse(s); }
    }
}
