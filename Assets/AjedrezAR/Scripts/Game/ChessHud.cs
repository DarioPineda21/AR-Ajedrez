using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Interfaz de la partida.
//  - Canvas en pantalla: turno, objetivo y boton para volver a las instrucciones.
//  - Canvas en el mundo, hijo del tablero (UI en RA): nueva partida, deshacer, nivel
//    de la IA y modo de juego. Se maneja con toques sobre la pantalla; al ser hijo
//    del tablero, cuando el tablero siga al marcador el panel se va con el.
public class ChessHud : MonoBehaviour
{
    ChessGame game;
    Text status, aiInfo, levelLabel, modeLabel;
    readonly List<Button> panelButtons = new List<Button>();
    static readonly List<RaycastResult> hits = new List<RaycastResult>();

    public void Init(ChessGame g, Camera cam)
    {
        game = g;
        UiKit.EnsureEventSystem();
        BuildScreenCanvas();
        BuildWorldPanel(cam);
    }

    void BuildScreenCanvas()
    {
        Canvas canvas = UiKit.ScreenCanvas("Canvas Pantalla");
        canvas.transform.SetParent(transform, false);
        Transform c = canvas.transform;

        status = UiKit.Label(c, "", new Vector2(0.02f, 0.90f), new Vector2(0.68f, 0.99f),
                             TextAnchor.MiddleLeft, Color.white, 60);
        UiKit.Label(c, "Objetivo: da jaque mate a la IA", new Vector2(0.02f, 0.84f), new Vector2(0.68f, 0.90f),
                    TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.3f), 44);
        aiInfo = UiKit.Label(c, "", new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.06f),
                             TextAnchor.MiddleLeft, new Color(0.8f, 0.8f, 0.8f), 36);

        Text t;
        UiKit.Btn(c, "Instrucciones", new Vector2(0.70f, 0.90f), new Vector2(0.98f, 0.99f),
                  () => SceneManager.LoadScene(UiKit.EscenaInstrucciones), out t);
    }

    // UI en RA: Canvas en el mundo, tumbado frente al lado de las blancas
    void BuildWorldPanel(Camera cam)
    {
        GameObject go = new GameObject("Panel RA");
        go.transform.SetParent(game.transform, false);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        go.AddComponent<GraphicRaycaster>();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 300f);
        rt.localScale = Vector3.one * 0.01f;       // 8 x 3 unidades, igual de ancho que el tablero
        rt.localPosition = new Vector3(0f, 0.05f, -6.3f);
        rt.localRotation = Quaternion.Euler(50f, 0f, 0f);

        Transform c = go.transform;
        UiKit.Panel(c, Vector2.zero, Vector2.one, new Color(0.05f, 0.06f, 0.10f, 0.85f));

        Text t;
        panelButtons.Add(UiKit.Btn(c, "Nueva partida", new Vector2(0.03f, 0.55f), new Vector2(0.49f, 0.95f),
                                   () => game.NewGame(), out t));
        panelButtons.Add(UiKit.Btn(c, "Deshacer", new Vector2(0.51f, 0.55f), new Vector2(0.97f, 0.95f),
                                   () => game.UndoTurn(), out t));
        panelButtons.Add(UiKit.Btn(c, "-", new Vector2(0.03f, 0.08f), new Vector2(0.17f, 0.48f),
                                   () => game.ChangeLevel(-1), out t));
        levelLabel = UiKit.Label(c, "", new Vector2(0.18f, 0.08f), new Vector2(0.50f, 0.48f),
                                 TextAnchor.MiddleCenter, Color.white, 48);
        panelButtons.Add(UiKit.Btn(c, "+", new Vector2(0.51f, 0.08f), new Vector2(0.65f, 0.48f),
                                   () => game.ChangeLevel(1), out t));
        panelButtons.Add(UiKit.Btn(c, "", new Vector2(0.68f, 0.08f), new Vector2(0.97f, 0.48f),
                                   () => game.SetVsAI(!game.vsAI), out modeLabel));
    }

    void Update()
    {
        if (game == null) return;
        status.text = game.StatusText();
        if (game.Thinking) status.text += "\nLa IA está pensando...";
        aiInfo.text = game.AiInfo();
        levelLabel.text = "Nivel IA: " + game.level;
        modeLabel.text = game.vsAI ? "Contra IA" : "2 personas";

        bool on = !game.Busy;
        for (int i = 0; i < panelButtons.Count; i++) panelButtons[i].interactable = on;
    }

    // Para que un toque sobre un boton no cuente tambien como toque sobre el tablero
    public bool OverUi(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        PointerEventData d = new PointerEventData(EventSystem.current);
        d.position = screenPos;
        hits.Clear();
        EventSystem.current.RaycastAll(d, hits);
        return hits.Count > 0;
    }
}
