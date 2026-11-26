using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChessCore;

// El tablero: un plano con 64 casillas y las 32 piezas como objetos de la escena.
// Este script no crea nada, solo mueve las piezas que ya estan en el Hierarchy.
// Todo cuelga de este objeto; mas adelante "Ajedrez" sera hijo del Image Target de Vuforia.
public class Tablero : MonoBehaviour
{
    public static readonly Color SelectedColor = new Color(0.95f, 0.85f, 0.20f);
    public static readonly Color MoveColor = new Color(0.35f, 0.80f, 0.40f);
    public static readonly Color CaptureColor = new Color(0.90f, 0.40f, 0.30f);
    public static readonly Color CheckColor = new Color(0.85f, 0.15f, 0.15f);
    public static readonly Color LastMoveColor = new Color(0.55f, 0.70f, 0.90f);

    [Header("Se asignan solos al armar el proyecto")]
    public Transform piezas;                       // objeto "Piezas" (con Blancas y Negras)
    public Casilla[] casillas = new Casilla[64];
    public Pieza[] modelos = new Pieza[12];        // prefabs, indice (tipo-1)*2 + (blanca?0:1)

    // Altura de la base del tablero 3D (Resources/Tablero3D) por debajo de su superficie de juego.
    // Se sube todo el tablero esa altura para que la base apoye en el marcador.
    public const float AlturaBase = 1.28f;

    readonly List<Pieza> todas = new List<Pieza>();
    Pieza[] enCasilla = new Pieza[64];
    bool listo;
    int extras;

    public void Init()
    {
        if (listo) return;
        listo = true;
        PonerModelo3D();
        todas.Clear();
        if (piezas != null) todas.AddRange(piezas.GetComponentsInChildren<Pieza>(true));
        for (int i = 0; i < 64; i++) enCasilla[i] = null;
        for (int i = 0; i < todas.Count; i++)
            if (todas[i].gameObject.activeSelf) enCasilla[todas[i].indiceCasilla] = todas[i];
    }

    // Si existe el modelo 3D del tablero (Resources/Tablero3D) se usa en lugar del plano y el marco.
    // Su superficie de juego queda en y = 0 y cada casilla mide 1, igual que el plano.
    void PonerModelo3D()
    {
        GameObject prefab = Resources.Load<GameObject>("Tablero3D/Tablero");
        if (prefab == null) return;

        GameObject m = Instantiate(prefab, transform);
        m.name = "Tablero 3D";
        m.transform.localPosition = Vector3.zero;
        m.transform.localRotation = Quaternion.identity;
        m.transform.localScale = Vector3.one;

        Material mat = Resources.Load<Material>("Tablero3D/Tablero3D");
        if (mat != null)
        {
            foreach (MeshRenderer r in m.GetComponentsInChildren<MeshRenderer>())
                r.sharedMaterial = mat;
        }

        Transform plano = transform.Find("Plano");
        if (plano != null) plano.gameObject.SetActive(false);
        Transform marco = transform.Find("Marco");
        if (marco != null) marco.gameObject.SetActive(false);

        transform.localPosition = new Vector3(0f, AlturaBase, 0f);
    }

    // ---------- Coordenadas ----------

    public static Vector3 SquareToLocal(int sq)
    {
        return new Vector3((sq & 7) - 3.5f, 0f, (sq >> 3) - 3.5f);
    }

    // Cruza un rayo (del raton, del toque o, mas adelante, del dedo) con el plano
    // del tablero y devuelve la casilla, o -1 si cae fuera.
    public int RayToSquare(Ray ray)
    {
        Vector3 p0 = transform.InverseTransformPoint(ray.origin);
        Vector3 p1 = transform.InverseTransformPoint(ray.origin + ray.direction);
        Vector3 d = p1 - p0;
        if (Mathf.Abs(d.y) < 1e-6f) return -1;
        float t = -p0.y / d.y;
        if (t < 0f) return -1;
        Vector3 p = p0 + d * t;
        int f = Mathf.FloorToInt(p.x + 4f);
        int r = Mathf.FloorToInt(p.z + 4f);
        if (f < 0 || f > 7 || r < 0 || r > 7) return -1;
        return r * 8 + f;
    }

    // ---------- Resaltados ----------

    public void ClearHighlights()
    {
        for (int sq = 0; sq < 64; sq++) if (casillas[sq] != null) casillas[sq].Limpiar();
    }

    public void Highlight(int sq, Color c)
    {
        if (sq >= 0 && sq < 64 && casillas[sq] != null) casillas[sq].Resaltar(c);
    }

    // ---------- Piezas ----------

    // Deja las piezas exactamente como dice el tablero
    public void Rebuild(Board b)
    {
        Init();
        HashSet<Pieza> usadas = new HashSet<Pieza>();
        Pieza[] nuevo = new Pieza[64];

        // 1) las que ya estan en su casilla se quedan
        for (int sq = 0; sq < 64; sq++)
        {
            sbyte code = b.Sq[sq];
            if (code == 0) continue;
            Pieza cur = enCasilla[sq];
            if (cur != null && cur.Codigo == code && !usadas.Contains(cur))
            {
                nuevo[sq] = cur;
                usadas.Add(cur);
            }
        }
        // 2) las demas casillas ocupadas toman una pieza libre del mismo tipo
        for (int sq = 0; sq < 64; sq++)
        {
            sbyte code = b.Sq[sq];
            if (code == 0 || nuevo[sq] != null) continue;
            Pieza p = BuscarLibre(code, sq, usadas);
            if (p == null) p = CrearExtra(code);
            nuevo[sq] = p;
            usadas.Add(p);
        }
        // 3) aplicar
        for (int sq = 0; sq < 64; sq++)
        {
            Pieza p = nuevo[sq];
            if (p == null) continue;
            p.gameObject.SetActive(true);
            p.Colocar(sq);
            p.transform.localPosition = SquareToLocal(sq);
            p.transform.localRotation = Quaternion.Euler(0f, p.blanca ? 0f : 180f, 0f);
        }
        for (int i = 0; i < todas.Count; i++)
        {
            Pieza p = todas[i];
            if (usadas.Contains(p)) continue;
            p.capturada = true;
            p.casilla = "--";
            p.gameObject.SetActive(false);
        }
        enCasilla = nuevo;
    }

    Pieza BuscarLibre(sbyte code, int sq, HashSet<Pieza> usadas)
    {
        Pieza activa = null, inactiva = null;
        for (int i = 0; i < todas.Count; i++)
        {
            Pieza p = todas[i];
            if (p.Codigo != code || usadas.Contains(p)) continue;
            if (p.casillaInicial == sq) return p;
            if (p.gameObject.activeSelf) { if (activa == null) activa = p; }
            else if (inactiva == null) inactiva = p;
        }
        return activa != null ? activa : inactiva;
    }

    // Por ejemplo cuando un peon corona y hace falta otra reina
    Pieza CrearExtra(sbyte code)
    {
        bool blanca = code > 0;
        int tipo = Piece.Type(code);
        Pieza modelo = modelos[(tipo - 1) * 2 + (blanca ? 0 : 1)];
        Transform padre = piezas.Find(blanca ? "Blancas" : "Negras");
        Pieza p = Instantiate(modelo, padre != null ? padre : piezas);
        extras++;
        p.name = modelo.name + " (extra " + extras + ")";
        p.nombre = p.name;
        p.casillaInicial = -1;
        todas.Add(p);
        return p;
    }

    // Anima la jugada. Se llama con el tablero todavia sin actualizar en la vista,
    // y al terminar el que llama hace Rebuild para dejar todo exacto.
    public IEnumerator AnimateMove(Move m, float duration)
    {
        Init();
        Pieza g = enCasilla[m.From];
        if (g == null) yield break;

        Vector3 a = SquareToLocal(m.From);
        Vector3 b = SquareToLocal(m.To);

        Pieza rook = null;
        Vector3 ra = Vector3.zero, rb = Vector3.zero;
        if ((m.Flags & MoveFlags.CastleKing) != 0)
        {
            rook = enCasilla[m.To + 1];
            ra = SquareToLocal(m.To + 1);
            rb = SquareToLocal(m.To - 1);
        }
        else if ((m.Flags & MoveFlags.CastleQueen) != 0)
        {
            rook = enCasilla[m.To - 2];
            ra = SquareToLocal(m.To - 2);
            rb = SquareToLocal(m.To + 1);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.Clamp01(t);
            g.transform.localPosition = Vector3.Lerp(a, b, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.5f);
            if (rook != null) rook.transform.localPosition = Vector3.Lerp(ra, rb, k);
            yield return null;
        }
    }
}
