using UnityEngine;
using ChessCore;

// Una casilla del plano. El plano es el tablero; estas casillas solo sirven para
// marcar (resaltar) la seleccion, las jugadas posibles, el jaque, etc.
public class Casilla : MonoBehaviour
{
    public string nombre;      // "e4"
    public char archivo;       // 'a'..'h' (columna)
    public int fila;           // 1..8
    public int indice;         // 0 = a1 ... 63 = h8
    public bool clara;
    public MeshRenderer marca; // el cuadro que se enciende al resaltar

    public void Resaltar(Color c)
    {
        if (marca == null) return;
        c.a = 0.6f;
        marca.enabled = true;
        marca.material.color = c;
    }

    public void Limpiar()
    {
        if (marca != null) marca.enabled = false;
    }
}
