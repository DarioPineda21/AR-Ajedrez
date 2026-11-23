using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChessCore
{
    // IA: negamax con poda alfa-beta, profundidad iterativa y busqueda de quietud.
    // No toca nada de Unity, asi corre en un hilo secundario.
    public static class ChessAI
    {
        const int Mate = 100000;
        const int Inf = 1000000;

        static readonly int[] Value = { 0, 100, 320, 330, 500, 900, 20000 };

        // Valor por casilla, vistos desde las blancas (fila 8 arriba)
        static readonly int[][] Pst = new int[][]
        {
            new int[64],
            // Peon
            new int[] {
                 0,  0,  0,  0,  0,  0,  0,  0,
                50, 50, 50, 50, 50, 50, 50, 50,
                10, 10, 20, 30, 30, 20, 10, 10,
                 5,  5, 10, 25, 25, 10,  5,  5,
                 0,  0,  0, 20, 20,  0,  0,  0,
                 5, -5,-10,  0,  0,-10, -5,  5,
                 5, 10, 10,-20,-20, 10, 10,  5,
                 0,  0,  0,  0,  0,  0,  0,  0 },
            // Caballo
            new int[] {
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -30,  5, 15, 20, 20, 15,  5,-30,
                -30,  0, 15, 20, 20, 15,  0,-30,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50 },
            // Alfil
            new int[] {
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0, 10, 10, 10, 10,  0,-10,
                -10, 10, 10, 10, 10, 10, 10,-10,
                -10,  5,  0,  0,  0,  0,  5,-10,
                -20,-10,-10,-10,-10,-10,-10,-20 },
            // Torre
            new int[] {
                  0,  0,  0,  0,  0,  0,  0,  0,
                  5, 10, 10, 10, 10, 10, 10,  5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                  0,  0,  0,  5,  5,  0,  0,  0 },
            // Dama
            new int[] {
                -20,-10,-10, -5, -5,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5,  5,  5,  5,  0,-10,
                 -5,  0,  5,  5,  5,  5,  0, -5,
                  0,  0,  5,  5,  5,  5,  0, -5,
                -10,  5,  5,  5,  5,  5,  0,-10,
                -10,  0,  5,  0,  0,  0,  0,-10,
                -20,-10,-10, -5, -5,-10,-10,-20 },
            // Rey
            new int[] {
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -30,-40,-40,-50,-50,-40,-40,-30,
                -20,-30,-30,-40,-40,-30,-30,-20,
                -10,-20,-20,-20,-20,-20,-20,-10,
                 20, 20,  0,  0,  0,  0, 20, 20,
                 20, 30, 10,  0,  0, 10, 30, 20 }
        };

        // Evaluacion desde el punto de vista de quien mueve
        public static int Evaluate(Board b)
        {
            int score = 0;
            for (int sq = 0; sq < 64; sq++)
            {
                sbyte p = b.Sq[sq];
                if (p == 0) continue;
                int t = Piece.Type(p);
                int f = sq & 7, r = sq >> 3;
                if (p > 0) score += Value[t] + Pst[t][(7 - r) * 8 + f];
                else score -= Value[t] + Pst[t][r * 8 + f];
            }
            return b.WhiteToMove ? score : -score;
        }

        public static SearchResult Search(Board board, int maxDepth, SearchControl ctrl)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Board b = board.Clone();
            Searcher s = new Searcher(b, ctrl, sw);
            Random rng = new Random();
            SearchResult res = new SearchResult();

            List<Move> root = b.LegalMoves();
            if (root.Count == 0) return res;

            res.HasMove = true;
            res.Best = root[rng.Next(root.Count)];
            bool havePrev = false;
            Move prev = res.Best;

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                int[] keys = new int[root.Count];
                for (int i = 0; i < root.Count; i++)
                {
                    keys[i] = s.Score(root[i]);
                    if (havePrev && root[i].SameAs(prev)) keys[i] += 1000000;
                }

                int bestScore = -Inf;
                List<Move> cands = new List<Move>();
                for (int i = 0; i < root.Count; i++)
                {
                    s.Pick(root, keys, i);
                    Move m = root[i];
                    UndoInfo u = b.MakeMove(m);
                    // ventana (bestScore - 1) para que las jugadas igual de buenas tengan puntaje exacto
                    int alpha = bestScore == -Inf ? -Inf : bestScore - 1;
                    int score = -s.Negamax(depth - 1, 1, -Inf, -alpha);
                    b.UnmakeMove(u);
                    if (s.Aborted) break;

                    if (score > bestScore) { bestScore = score; cands.Clear(); cands.Add(m); }
                    else if (score == bestScore) cands.Add(m);
                }
                if (s.Aborted || cands.Count == 0) break;

                res.Best = cands[rng.Next(cands.Count)];
                res.Depth = depth;
                res.Score = bestScore;
                prev = res.Best;
                havePrev = true;
                if (bestScore > Mate - 100) break;   // ya hay mate forzado
            }

            res.Nodes = s.Nodes;
            res.Millis = sw.ElapsedMilliseconds;
            return res;
        }

        sealed class Searcher
        {
            readonly Board b;
            readonly SearchControl ctrl;
            readonly Stopwatch sw;
            public long Nodes;
            public bool Aborted;

            public Searcher(Board b, SearchControl ctrl, Stopwatch sw)
            {
                this.b = b; this.ctrl = ctrl; this.sw = sw;
            }

            void CheckStop()
            {
                if ((++Nodes & 1023) != 0 || ctrl == null) return;
                if (ctrl.Stop) Aborted = true;
                else if (ctrl.MaxMillis > 0 && sw.ElapsedMilliseconds > ctrl.MaxMillis) Aborted = true;
            }

            public int Score(Move m)
            {
                int s = 0;
                if (m.IsCapture)
                {
                    int victim = (m.Flags & MoveFlags.EnPassant) != 0 ? Piece.Pawn : Piece.Type(b.Sq[m.To]);
                    int attacker = Piece.Type(b.Sq[m.From]);
                    s = 10000 + 10 * Value[victim] - Value[attacker] / 10;
                }
                if (m.Promo != 0) s += 9500 + Value[m.Promo];
                return s;
            }

            // Pone la mejor jugada restante en la posicion i
            public void Pick(List<Move> moves, int[] keys, int i)
            {
                int bi = i;
                for (int j = i + 1; j < moves.Count; j++) if (keys[j] > keys[bi]) bi = j;
                if (bi == i) return;
                int tk = keys[i]; keys[i] = keys[bi]; keys[bi] = tk;
                Move tm = moves[i]; moves[i] = moves[bi]; moves[bi] = tm;
            }

            public int Negamax(int depth, int ply, int alpha, int beta)
            {
                CheckStop();
                if (Aborted) return 0;
                if (b.HalfMove >= 100) return 0;
                if (depth <= 0) return Quiesce(alpha, beta, 0);

                List<Move> moves = b.LegalMoves();
                if (moves.Count == 0) return b.InCheck() ? -Mate + ply : 0;

                int[] keys = new int[moves.Count];
                for (int i = 0; i < moves.Count; i++) keys[i] = Score(moves[i]);

                for (int i = 0; i < moves.Count; i++)
                {
                    Pick(moves, keys, i);
                    UndoInfo u = b.MakeMove(moves[i]);
                    int score = -Negamax(depth - 1, ply + 1, -beta, -alpha);
                    b.UnmakeMove(u);
                    if (Aborted) return 0;
                    if (score >= beta) return beta;
                    if (score > alpha) alpha = score;
                }
                return alpha;
            }

            int Quiesce(int alpha, int beta, int qd)
            {
                CheckStop();
                if (Aborted) return 0;

                int stand = Evaluate(b);
                if (stand >= beta) return beta;
                if (stand > alpha) alpha = stand;
                if (qd >= 6) return alpha;

                List<Move> moves = b.LegalMoves(true);
                int[] keys = new int[moves.Count];
                for (int i = 0; i < moves.Count; i++) keys[i] = Score(moves[i]);

                for (int i = 0; i < moves.Count; i++)
                {
                    Pick(moves, keys, i);
                    UndoInfo u = b.MakeMove(moves[i]);
                    int score = -Quiesce(-beta, -alpha, qd + 1);
                    b.UnmakeMove(u);
                    if (Aborted) return 0;
                    if (score >= beta) return beta;
                    if (score > alpha) alpha = score;
                }
                return alpha;
            }
        }
    }
}
