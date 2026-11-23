using System.Collections.Generic;

namespace ChessCore
{
    public sealed partial class Board
    {
        // Jugadas legales: se generan las pseudo-legales y se descartan
        // las que dejan al propio rey en jaque.
        public List<Move> LegalMoves()
        {
            return LegalMoves(false);
        }

        // capturesOnly: solo capturas y promociones (para la busqueda de quietud de la IA)
        public List<Move> LegalMoves(bool capturesOnly)
        {
            List<Move> list = new List<Move>(48);
            GenPseudo(list, capturesOnly);

            bool mover = WhiteToMove;
            int kingSq = KingSquare(mover);
            int w = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Move m = list[i];
                int k = Piece.Type(Sq[m.From]) == Piece.King ? m.To : kingSq;
                UndoInfo u = MakeMove(m);
                bool ok = !IsAttacked(k, !mover);
                UnmakeMove(u);
                if (ok) list[w++] = m;
            }
            if (w < list.Count) list.RemoveRange(w, list.Count - w);
            return list;
        }

        // Cuenta nodos hoja a cierta profundidad. Sirve para probar el generador.
        public long Perft(int depth)
        {
            if (depth == 0) return 1;
            List<Move> moves = LegalMoves();
            if (depth == 1) return moves.Count;
            long n = 0;
            for (int i = 0; i < moves.Count; i++)
            {
                UndoInfo u = MakeMove(moves[i]);
                n += Perft(depth - 1);
                UnmakeMove(u);
            }
            return n;
        }

        void GenPseudo(List<Move> list, bool capsOnly)
        {
            bool white = WhiteToMove;
            for (int sq = 0; sq < 64; sq++)
            {
                sbyte p = Sq[sq];
                if (p == 0 || (p > 0) != white) continue;
                int t = Piece.Type(p);
                int f = sq & 7, r = sq >> 3;

                switch (t)
                {
                    case Piece.Pawn:
                        GenPawn(list, sq, f, r, white, capsOnly);
                        break;

                    case Piece.Knight:
                        for (int i = 0; i < 8; i++)
                        {
                            int nf = f + KnightDF[i], nr = r + KnightDR[i];
                            if (nf < 0 || nf > 7 || nr < 0 || nr > 7) continue;
                            AddStep(list, sq, nr * 8 + nf, white, capsOnly);
                        }
                        break;

                    case Piece.Bishop:
                        GenSlide(list, sq, f, r, white, 4, 8, capsOnly);
                        break;
                    case Piece.Rook:
                        GenSlide(list, sq, f, r, white, 0, 4, capsOnly);
                        break;
                    case Piece.Queen:
                        GenSlide(list, sq, f, r, white, 0, 8, capsOnly);
                        break;

                    case Piece.King:
                        for (int i = 0; i < 8; i++)
                        {
                            int nf = f + KingDF[i], nr = r + KingDR[i];
                            if (nf < 0 || nf > 7 || nr < 0 || nr > 7) continue;
                            AddStep(list, sq, nr * 8 + nf, white, capsOnly);
                        }
                        if (!capsOnly) GenCastling(list, sq, white);
                        break;
                }
            }
        }

        // Un paso a una casilla: vacia o con pieza rival
        void AddStep(List<Move> list, int from, int to, bool white, bool capsOnly)
        {
            sbyte q = Sq[to];
            if (q == 0)
            {
                if (!capsOnly) list.Add(new Move(from, to, 0, 0));
            }
            else if ((q > 0) != white)
            {
                list.Add(new Move(from, to, 0, MoveFlags.Capture));
            }
        }

        void GenSlide(List<Move> list, int sq, int f, int r, bool white, int d0, int d1, bool capsOnly)
        {
            for (int d = d0; d < d1; d++)
            {
                int nf = f + DirDF[d], nr = r + DirDR[d];
                while (nf >= 0 && nf < 8 && nr >= 0 && nr < 8)
                {
                    int to = nr * 8 + nf;
                    sbyte q = Sq[to];
                    if (q == 0)
                    {
                        if (!capsOnly) list.Add(new Move(sq, to, 0, 0));
                    }
                    else
                    {
                        if ((q > 0) != white) list.Add(new Move(sq, to, 0, MoveFlags.Capture));
                        break;
                    }
                    nf += DirDF[d];
                    nr += DirDR[d];
                }
            }
        }

        void GenPawn(List<Move> list, int sq, int f, int r, bool white, bool capsOnly)
        {
            int dir = white ? 1 : -1;
            int startRank = white ? 1 : 6;
            int beforePromo = white ? 6 : 1;   // fila desde la que se promociona
            int nr = r + dir;
            if (nr < 0 || nr > 7) return;

            int one = nr * 8 + f;
            if (Sq[one] == 0)
            {
                if (r == beforePromo) AddPromotions(list, sq, one, 0);
                else if (!capsOnly)
                {
                    list.Add(new Move(sq, one, 0, 0));
                    int two = one + 8 * dir;
                    if (r == startRank && Sq[two] == 0)
                        list.Add(new Move(sq, two, 0, MoveFlags.DoublePush));
                }
            }

            for (int df = -1; df <= 1; df += 2)
            {
                int nf = f + df;
                if (nf < 0 || nf > 7) continue;
                int to = nr * 8 + nf;
                sbyte q = Sq[to];
                if (q != 0 && (q > 0) != white)
                {
                    if (r == beforePromo) AddPromotions(list, sq, to, MoveFlags.Capture);
                    else list.Add(new Move(sq, to, 0, MoveFlags.Capture));
                }
                else if (q == 0 && to == EpSquare)
                {
                    list.Add(new Move(sq, to, 0, MoveFlags.Capture | MoveFlags.EnPassant));
                }
            }
        }

        static void AddPromotions(List<Move> list, int from, int to, int flags)
        {
            list.Add(new Move(from, to, Piece.Queen, flags));
            list.Add(new Move(from, to, Piece.Rook, flags));
            list.Add(new Move(from, to, Piece.Bishop, flags));
            list.Add(new Move(from, to, Piece.Knight, flags));
        }

        void GenCastling(List<Move> list, int sq, bool white)
        {
            if (white && sq == 4)
            {
                if ((Castling & WhiteKingSide) != 0 && Sq[5] == 0 && Sq[6] == 0 && Sq[7] == Piece.Rook
                    && !IsAttacked(4, false) && !IsAttacked(5, false))
                    list.Add(new Move(4, 6, 0, MoveFlags.CastleKing));
                if ((Castling & WhiteQueenSide) != 0 && Sq[3] == 0 && Sq[2] == 0 && Sq[1] == 0 && Sq[0] == Piece.Rook
                    && !IsAttacked(4, false) && !IsAttacked(3, false))
                    list.Add(new Move(4, 2, 0, MoveFlags.CastleQueen));
            }
            else if (!white && sq == 60)
            {
                if ((Castling & BlackKingSide) != 0 && Sq[61] == 0 && Sq[62] == 0 && Sq[63] == -Piece.Rook
                    && !IsAttacked(60, true) && !IsAttacked(61, true))
                    list.Add(new Move(60, 62, 0, MoveFlags.CastleKing));
                if ((Castling & BlackQueenSide) != 0 && Sq[59] == 0 && Sq[58] == 0 && Sq[57] == 0 && Sq[56] == -Piece.Rook
                    && !IsAttacked(60, true) && !IsAttacked(59, true))
                    list.Add(new Move(60, 58, 0, MoveFlags.CastleQueen));
            }
        }
    }
}
