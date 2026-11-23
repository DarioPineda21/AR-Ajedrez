using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla 1: instrucciones. No hay RA aqui; las instrucciones y la actividad
// van en escenas distintas para que nunca aparezcan al mismo tiempo.
public class InstruccionesUI : MonoBehaviour
{
    void Start()
    {
        UiKit.EnsureEventSystem();
        Canvas canvas = UiKit.ScreenCanvas("Canvas Instrucciones");
        Transform c = canvas.transform;

        UiKit.Panel(c, Vector2.zero, Vector2.one, new Color(0.08f, 0.09f, 0.13f, 1f));

        UiKit.Label(c, "Ajedrez AR", new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.97f),
                    TextAnchor.MiddleCenter, Color.white, 110);

        UiKit.Label(c, "Objetivo: da jaque mate a la IA antes de que ella te lo dé a ti. Juegas con las blancas.",
                    new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.85f),
                    TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f), 56);

        UiKit.Label(c, "Cómo se juega", new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.72f),
                    TextAnchor.MiddleLeft, Color.white, 56);

        string pasos =
            "1. Toca una pieza blanca. Sus movimientos posibles se marcan en verde y las capturas en rojo.\n" +
            "2. Toca la casilla a la que quieres moverla.\n" +
            "3. La IA contesta con las negras. Espera a que mueva.\n" +
            "4. El panel junto al tablero sirve para empezar otra partida, deshacer y cambiar el nivel de la IA.\n" +
            "5. Si una casilla se pone roja, ese rey está en jaque. Gana quien da jaque mate.";
        UiKit.Label(c, pasos, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.66f),
                    TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.9f), 48);

        Text t;
        UiKit.Btn(c, "Jugar", new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.18f), Jugar, out t);
    }

    void Jugar()
    {
        SceneManager.LoadScene(UiKit.EscenaJuego);
    }
}
