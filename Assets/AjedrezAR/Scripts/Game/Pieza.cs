using UnityEngine;
using ChessCore;

// Tipos de pieza. Los numeros son los mismos que usa el motor (ChessCore.Piece).
public enum TipoPieza { Peon = 1, Caballo = 2, Alfil = 3, Torre = 4, Reina = 5, Rey = 6 }

// Datos de una pieza del tablero. Cada pieza es un objeto de la escena con su
// modelo 3D como hijo y este componente con sus parametros.
public class Pieza : MonoBehaviour
{
    [Header("Datos de la pieza")]
    public string nombre;
    public TipoPieza tipo;
    public bool blanca;
    public int valor;                 // peon 1, caballo 3, alfil 3, torre 5, reina 9, rey 0
    public int casillaInicial;        // 0 = a1 ... 63 = h8

    [Header("Estado en la partida (se actualiza al jugar)")]
    public string casilla;            // por ejemplo "e4"
    public int indiceCasilla;
    public bool capturada;

    // Codigo que usa el motor: positivo blancas, negativo negras
    public sbyte Codigo { get { return (sbyte)(blanca ? (int)tipo : -(int)tipo); } }

    public static int ValorDe(TipoPieza t)
    {
        switch (t)
        {
            case TipoPieza.Peon: return 1;
            case TipoPieza.Caballo: return 3;
            case TipoPieza.Alfil: return 3;
            case TipoPieza.Torre: return 5;
            case TipoPieza.Reina: return 9;
            default: return 0;
        }
    }

    public void Colocar(int sq)
    {
        indiceCasilla = sq;
        casilla = Sq.Name(sq);
        capturada = false;
    }
}
