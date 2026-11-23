using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

// Ayudas para armar la interfaz (Canvas, textos y botones) desde codigo.
// Asi las escenas quedan casi vacias y no hay que acomodar nada a mano.
public static class UiKit
{
    public const string EscenaInstrucciones = "Instrucciones";
    public const string EscenaJuego = "AjedrezAR";

    static Font font;

    static Font GetFont()
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font;
    }

    // Sin EventSystem los botones no reciben toques. Se usa el modulo que corresponda
    // al sistema de entrada activo del proyecto.
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        InputSystemUIInputModule m = go.AddComponent<InputSystemUIInputModule>();
        m.AssignDefaultActions();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    public static Canvas ScreenCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1080f, 1920f);
        s.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    // Rectangulo definido por anclas (fracciones del padre), sin margenes
    public static RectTransform Box(Transform parent, string name, Vector2 aMin, Vector2 aMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Image Panel(Transform parent, Vector2 aMin, Vector2 aMax, Color color)
    {
        RectTransform rt = Box(parent, "Panel", aMin, aMax);
        Image im = rt.gameObject.AddComponent<Image>();
        im.color = color;
        return im;
    }

    // El texto se ajusta solo al tamano del rectangulo, asi cabe en cualquier pantalla
    public static Text Label(Transform parent, string text, Vector2 aMin, Vector2 aMax,
                             TextAnchor anchor, Color color, int maxSize)
    {
        RectTransform rt = Box(parent, "Texto", aMin, aMax);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = GetFont();
        t.text = text;
        t.alignment = anchor;
        t.color = color;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 10;
        t.resizeTextMaxSize = maxSize;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;   // solo los botones y fondos reciben toques
        return t;
    }

    public static Button Btn(Transform parent, string label, Vector2 aMin, Vector2 aMax,
                             UnityAction onClick, out Text text)
    {
        Image bg = Panel(parent, aMin, aMax, new Color(0.20f, 0.45f, 0.75f, 1f));
        bg.gameObject.name = "Boton " + label;
        Button b = bg.gameObject.AddComponent<Button>();
        b.targetGraphic = bg;
        ColorBlock cb = b.colors;
        cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
        b.colors = cb;
        text = Label(bg.transform, label, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f),
                     TextAnchor.MiddleCenter, Color.white, 60);
        b.onClick.AddListener(onClick);
        return b;
    }
}
