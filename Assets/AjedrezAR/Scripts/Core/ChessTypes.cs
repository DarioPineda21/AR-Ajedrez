using System;

// Tipos basicos del motor. Todo el Core es C# puro: no usa UnityEngine,
// asi puede correr en un hilo secundario (leccion de la practica 1).
namespace ChessCore
{
    public static class Piece
    {
        public const sbyte None = 0;
        public const sbyte Pawn = 1;
        public const sbyte Knight = 2;
        public const sbyte Bishop = 3;
        public const sbyte Rook = 4;
        public const sbyte Queen = 5;
        public const sbyte King = 6;

        // Las blancas son positivas y las negras negativas
        public static int Type(sbyte p) { return p < 0 ? -p : p; }

        public static sbyte Make(int type, bool white)
        {
            return (sbyte)(white ? type : -type);
        }

        public static char ToChar(sbyte p)
        {
            char c = ".PNBRQK"[Type(p)];
            return p < 0 ? char.ToLowerInvariant(c) : c;
        }

        public static sbyte FromChar(char c)
        {
            int t = "PNBRQK".IndexOf(char.ToUpperInvariant(c)) + 1;
            if (t == 0) return None;
            return Make(t, char.IsUpper(c));
        }
    }

    public static class MoveFlags
    {
        public const int Capture = 1;
        public const int EnPassant = 2;
        public const int CastleKing = 4;
        public const int CastleQueen = 8;
        public const int DoublePush = 16;
    }

    // Casillas: indice = fila * 8 + columna. a1 = 0, h1 = 7, a8 = 56, h8 = 63
    public static class Sq
    {
        public static string Name(int sq)
        {
            return "" + (char)('a' + (sq & 7)) + (char)('1' + (sq >> 3));
        }

        public static int Parse(string s)
        {
            if (s == null || s.Length != 2) return -1;
            int f = s[0] - 'a';
            int r = s[1] - '1';
            if (f < 0 || f > 7 || r < 0 || r > 7) return -1;
            return r * 8 + f;
        }
    }

    public struct Move
    {
        public byte From;
        public byte To;
        public byte Promo;   // 0 si no hay promocion, si no el tipo de pieza
        public byte Flags;

        public Move(int from, int to, int promo, int flags)
        {
            From = (byte)from;
            To = (byte)to;
            Promo = (byte)promo;
            Flags = (byte)flags;
        }

        public bool IsCapture { get { return (Flags & MoveFlags.Capture) != 0; } }

        public bool SameAs(Move o)
        {
            return From == o.From && To == o.To && Promo == o.Promo;
        }

        public override string ToString()
        {
            string s = Sq.Name(From) + Sq.Name(To);
            if (Promo != 0) s += char.ToLowerInvariant(Piece.ToChar((sbyte)Promo));
            return s;
        }
    }

    // Lo necesario para deshacer una jugada
    public struct UndoInfo
    {
        public Move Move;
        public sbyte Captured;
        public int Castling;
        public int Ep;
        public int Half;
    }

    public enum GameStatus
    {
        Playing,
        Checkmate,
        Stalemate,
        DrawFiftyMoves,
        DrawMaterial
    }

    // Bandera de paro y limite de tiempo para el hilo de la IA.
    // Es el mismo patron de stopTurbulencethread de la practica 1.
    public sealed class SearchControl
    {
        public volatile bool Stop;
        public long MaxMillis;   // 0 = sin limite
    }

    public struct SearchResult
    {
        public Move Best;
        public bool HasMove;
        public int Depth;       // profundidad terminada
        public int Score;       // en centipeones, desde quien mueve
        public long Nodes;
        public long Millis;
    }
}
