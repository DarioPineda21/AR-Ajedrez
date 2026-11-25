using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChessCore;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Prueba 1: ajedrez en pantalla, sin AR y sin camara del telefono.
// Se juega con el raton (o con toque) contra la IA o contra otra persona.
// Uso: crea un GameObject vacio en una escena vacia y agrega este componente
// (o usa el menu Ajedrez > Crear partida en la escena).
[RequireComponent(typeof(AIPlayer))]
public class ChessGame : MonoBehaviour
{
    [Range(1, 4)] public int level = 2;        // profundidad de la IA
    public bool vsAI = true;                   // las blancas son de la persona
    public int maxThinkMs = 5000;              // tope de tiempo por jugada de la IA

    Board board;
    Tablero view;
    AIPlayer ai;
    Camera cam;

    List<Move> legal = new List<Move>();
    readonly List<UndoInfo> history = new List<UndoInfo>();
    int selected = -1;
    bool animating;
    bool aiRequested;
    bool hasLastMove;
    Move lastMove;
    GameStatus status;
    bool inCheck;
    SearchResult lastStats;
    ChessHud hud;

    public bool legacyGui = false;             // el panel viejo con OnGUI (ya no se usa)

    // Con AR la camara es la de Vuforia (la mueve el telefono): no se toca ni se acerca
    [HideInInspector] public bool modoAR = false;

    int lastScreenW, lastScreenH;

    void Awake()
    {
        ai = GetComponent<AIPlayer>();
        SetupScene();
        view = GetComponentInChildren<Tablero>();
        if (view == null)
        {
            Debug.LogError("Falta el objeto Tablero. Usa el menu Ajedrez > Preparar proyecto.");
            enabled = false;
            return;
        }
        view.Init();
        hud = gameObject.AddComponent<ChessHud>();
        hud.Init(this, cam);
        NewGame();
    }

    // ---------- Escena ----------

    void SetupScene()
    {
        cam = Camera.main;
        if (cam == null)
        {
            GameObject c = new GameObject("Main Camera");
            c.tag = "MainCamera";
            cam = c.AddComponent<Camera>();
        }
        if (!modoAR) ConfigurarCamaraPlana();

        if (FindFirstObjectByType<Light>() != null) return;   // la escena ya trae su luz
        GameObject l = new GameObject("Luz del tablero");
        l.transform.SetParent(transform, false);
        l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = l.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
    }

    void ConfigurarCamaraPlana()
    {
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.10f, 0.13f);
        cam.fieldOfView = 40f;
        AdjustCamera(true);
    }

    // Lo llama MarcadorAR cuando Vuforia no arranca: el juego vuelve a verse como en la prueba 1
    public void ModoPlano()
    {
        modoAR = false;
        if (cam == null) cam = Camera.main;
        if (cam != null) { cam.nearClipPlane = 0.3f; ConfigurarCamaraPlana(); }
    }

    // En pantallas verticales aleja la camara para que quepa todo el tablero
    void AdjustCamera(bool force)
    {
        if (!force && lastScreenW == Screen.width && lastScreenH == Screen.height) return;
        lastScreenW = Screen.width;
        lastScreenH = Screen.height;

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float k = Mathf.Max(1f, 1.1f / aspect);
        Vector3 dir = new Vector3(0f, 9.5f, -7.5f).normalized;
        // se mira un poco hacia las blancas para que quepa el panel de botones
        Vector3 look = new Vector3(0f, 0f, -1.3f);
        cam.transform.position = look + dir * 19f * k;
        cam.transform.LookAt(look);
    }

    // ---------- Partida ----------

    public void NewGame()
    {
        StopAllCoroutines();
        ai.Cancel();
        aiRequested = false;
        animating = false;

        board = Board.Start();
        history.Clear();
        selected = -1;
        hasLastMove = false;
        lastStats = default(SearchResult);

        view.Rebuild(board);
        Refresh();
    }

    // Recalcula jugadas legales, estado y resaltados
    void Refresh()
    {
        legal = board.LegalMoves();
        status = board.GetStatus(legal);
        inCheck = board.InCheck();

        view.ClearHighlights();
        if (hasLastMove)
        {
            view.Highlight(lastMove.From, Tablero.LastMoveColor);
            view.Highlight(lastMove.To, Tablero.LastMoveColor);
        }
        if (inCheck) view.Highlight(board.KingSquare(board.WhiteToMove), Tablero.CheckColor);
        if (selected >= 0)
        {
            view.Highlight(selected, Tablero.SelectedColor);
            for (int i = 0; i < legal.Count; i++)
            {
                if (legal[i].From != selected) continue;
                view.Highlight(legal[i].To, legal[i].IsCapture ? Tablero.CaptureColor : Tablero.MoveColor);
            }
        }
    }

    bool IsAITurn()
    {
        return vsAI && !board.WhiteToMove && status == GameStatus.Playing;
    }

    IEnumerator PlayMove(Move m)
    {
        animating = true;
        selected = -1;

        UndoInfo u = board.MakeMove(m);
        history.Add(u);
        hasLastMove = true;
        lastMove = m;

        yield return view.AnimateMove(m, 0.25f);

        view.Rebuild(board);
        animating = false;
        Refresh();
    }

    public void UndoTurn()
    {
        if (animating) return;
        ai.Cancel();
        aiRequested = false;
        if (history.Count == 0) return;

        PopMove();
        // contra la IA se deshace hasta que vuelva a mover la persona (blancas)
        if (vsAI)
            while (history.Count > 0 && !board.WhiteToMove) PopMove();

        hasLastMove = history.Count > 0;
        if (hasLastMove) lastMove = history[history.Count - 1].Move;
        selected = -1;
        view.Rebuild(board);
        Refresh();
    }

    void PopMove()
    {
        board.UnmakeMove(history[history.Count - 1]);
        history.RemoveAt(history.Count - 1);
    }

    // ---------- Cada cuadro ----------

    void Update()
    {
        if (!modoAR && cam != null) AdjustCamera(false);
        if (animating || status != GameStatus.Playing) return;

        if (IsAITurn())
        {
            if (!aiRequested)
            {
                ai.RequestMove(board, level, maxThinkMs);
                aiRequested = true;
            }
            else
            {
                SearchResult r;
                // TryEnter: si el hilo esta escribiendo, se vuelve a intentar el siguiente cuadro
                if (ai.TryTakeResult(out r))
                {
                    aiRequested = false;
                    lastStats = r;
                    if (r.HasMove) StartCoroutine(PlayMove(r.Best));
                }
            }
            return;
        }

        Vector2 pos;
        if (!PointerDown(out pos)) return;
        if (OverGui(pos)) return;
        OnSquareClicked(view.RayToSquare(cam.ScreenPointToRay(pos)));
    }

    void OnSquareClicked(int sq)
    {
        if (sq < 0)
        {
            selected = -1;
            Refresh();
            return;
        }

        sbyte p = board.Sq[sq];
        bool own = p != 0 && (p > 0) == board.WhiteToMove;
        if (own)
        {
            selected = sq;
            Refresh();
            return;
        }

        if (selected >= 0)
        {
            bool found = false;
            Move chosen = default(Move);
            for (int i = 0; i < legal.Count; i++)
            {
                Move m = legal[i];
                if (m.From != selected || m.To != sq) continue;
                // si hay promocion se corona dama
                if (!found || m.Promo == Piece.Queen) { chosen = m; found = true; }
            }
            if (found)
            {
                StartCoroutine(PlayMove(chosen));
                return;
            }
        }

        selected = -1;
        Refresh();
    }

    // ---------- Entrada: raton o toque, con el sistema nuevo o el viejo ----------

    bool PointerDown(out Vector2 pos)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            pos = mouse.position.ReadValue();
            return true;
        }
        Touchscreen ts = Touchscreen.current;
        if (ts != null && ts.primaryTouch.press.wasPressedThisFrame)
        {
            pos = ts.primaryTouch.position.ReadValue();
            return true;
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            pos = Input.GetTouch(0).position;
            return true;
        }
#endif
        pos = Vector2.zero;
        return false;
    }

    // ---------- Datos y acciones que usa la interfaz (ChessHud) ----------

    public bool Busy { get { return animating; } }
    public bool Thinking { get { return ai != null && ai.IsThinking; } }

    public string AiInfo()
    {
        if (!lastStats.HasMove) return "";
        return "Última jugada de la IA: " + lastStats.Best + " · profundidad " + lastStats.Depth +
               " · " + lastStats.Nodes + " nodos · " + lastStats.Millis + " ms";
    }

    public void SetVsAI(bool v)
    {
        if (v == vsAI) return;
        vsAI = v;
        ai.Cancel();
        aiRequested = false;
    }

    public void ChangeLevel(int delta)
    {
        level = Mathf.Clamp(level + delta, 1, 4);
    }

    // ---------- Panel viejo con OnGUI (se deja por si hace falta depurar) ----------

    const float PanelW = 290f;
    const float PanelH = 250f;

    float GuiScale()
    {
        return Mathf.Max(1f, Screen.height / 600f);
    }

    bool OverGui(Vector2 screenPos)
    {
        if (!legacyGui) return hud != null && hud.OverUi(screenPos);
        float s = GuiScale();
        Vector2 p = new Vector2(screenPos.x, Screen.height - screenPos.y);
        return new Rect(0f, 0f, (PanelW + 10f) * s, (PanelH + 10f) * s).Contains(p);
    }

    public string StatusText()
    {
        switch (status)
        {
            case GameStatus.Checkmate: return "Jaque mate. Ganan las " + (board.WhiteToMove ? "negras" : "blancas");
            case GameStatus.Stalemate: return "Tablas por ahogado";
            case GameStatus.DrawFiftyMoves: return "Tablas por la regla de las 50 jugadas";
            case GameStatus.DrawMaterial: return "Tablas por material insuficiente";
        }
        return "Turno de las " + (board.WhiteToMove ? "blancas" : "negras") + (inCheck ? " (jaque)" : "");
    }

    void OnGUI()
    {
        if (!legacyGui || board == null) return;
        float s = GuiScale();
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));

        GUILayout.BeginArea(new Rect(10f, 10f, PanelW, PanelH), GUI.skin.box);
        GUILayout.Label(StatusText());
        if (ai.IsThinking) GUILayout.Label("La IA esta pensando...");

        GUI.enabled = !animating;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Nueva partida")) NewGame();
        if (GUILayout.Button("Deshacer")) UndoTurn();
        GUILayout.EndHorizontal();

        bool v = GUILayout.Toggle(vsAI, "Contra la IA (tu juegas blancas)");
        if (v != vsAI)
        {
            vsAI = v;
            ai.Cancel();
            aiRequested = false;
        }

        GUILayout.Label("Nivel de la IA (profundidad): " + level);
        level = Mathf.RoundToInt(GUILayout.HorizontalSlider(level, 1f, 4f));
        GUI.enabled = true;

        if (lastStats.HasMove)
        {
            GUILayout.Label("Ultima jugada de la IA: " + lastStats.Best + "\nprofundidad " + lastStats.Depth +
                            ", " + lastStats.Nodes + " nodos, " + lastStats.Millis + " ms");
        }
        GUILayout.EndArea();
    }
}
