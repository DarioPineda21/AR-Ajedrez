using System.Threading;
using UnityEngine;
using ChessCore;

// La IA piensa en un hilo secundario para que la pantalla no se congele.
// Es el patron de la practica 1 (FlightThreadSinc):
//   - bandera de paro (SearchControl.Stop) en lugar de stopTurbulencethread
//   - lock para escribir el resultado
//   - Monitor.TryEnter para leerlo desde Update() sin quedarse esperando
//   - OnDestroy detiene el hilo y espera con Join
// El hilo solo usa datos de C# (Board, Move), nunca la API de Unity.
public class AIPlayer : MonoBehaviour
{
    readonly object moveLock = new object();

    Thread thread;
    SearchControl current;
    SearchResult pending;
    bool hasPending;
    int ticket;                  // identifica la busqueda vigente
    volatile bool thinking;

    public bool IsThinking { get { return thinking; } }

    // Empieza a calcular la mejor jugada sobre una copia del tablero
    public void RequestMove(Board board, int depth, int maxMillis)
    {
        Cancel();

        Board copy = board.Clone();
        SearchControl ctrl = new SearchControl();
        ctrl.MaxMillis = maxMillis;

        int myTicket;
        lock (moveLock)
        {
            ticket++;
            myTicket = ticket;
            hasPending = false;
        }

        current = ctrl;
        thinking = true;

        Thread t = new Thread(() =>
        {
            SearchResult r = ChessAI.Search(copy, depth, ctrl);
            lock (moveLock)
            {
                // si mientras tanto se cancelo o se pidio otra, se descarta
                if (myTicket == ticket)
                {
                    pending = r;
                    hasPending = true;
                    thinking = false;
                }
            }
        });
        t.IsBackground = true;
        t.Name = "ChessAI";
        thread = t;
        t.Start();
    }

    // El hilo principal lo intenta cada cuadro; si el candado esta ocupado no espera
    public bool TryTakeResult(out SearchResult result)
    {
        result = default(SearchResult);
        if (!Monitor.TryEnter(moveLock)) return false;
        try
        {
            if (!hasPending) return false;
            result = pending;
            hasPending = false;
            return true;
        }
        finally
        {
            Monitor.Exit(moveLock);
        }
    }

    public void Cancel()
    {
        if (current != null) current.Stop = true;
        lock (moveLock)
        {
            ticket++;
            hasPending = false;
        }
        thinking = false;
    }

    void OnDestroy()
    {
        if (current != null) current.Stop = true;
        if (thread != null && thread.IsAlive) thread.Join(500);
    }
}
