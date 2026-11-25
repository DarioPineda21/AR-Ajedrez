using UnityEngine;
using Vuforia;

// =====================================================================================
//  PRACTICA 2: API (Vuforia Engine)
//
//  Proceso de la API que se usa en este proyecto:
//   1) SDK: el paquete "Vuforia Engine AR" (com.ptc.vuforia.engine 11.4.4) esta instalado
//      desde el Package Manager. Todo lo que empieza con "Vuforia." viene de ese SDK.
//   2) Licencia (codigo HASH): se escribe en Resources/VuforiaConfiguration.asset
//      (Window > Vuforia Configuration > License Key). Sin la licencia la API no arranca.
//      Ese archivo NO se sube a GitHub (esta en el .gitignore).
//   3) Comunicacion: el componente VuforiaBehaviour (en la camara) abre la camara del
//      telefono y hace el rastreo. Nuestro codigo solo habla con la API por tres lados:
//        - VuforiaApplication.Instance     -> eventos de inicio y de error
//        - VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(...)
//                                          -> crea el marcador a partir de una imagen
//        - ObserverBehaviour.OnTargetStatusChanged -> evento "se ve / se perdio el marcador"
//
//  Lo que hace este script: crea el Image Target del tablero con la imagen Marcador.png,
//  pega el juego (objeto Ajedrez) al marcador y lo muestra solo cuando la camara lo ve.
//  Si Vuforia no arranca (sin licencia, sin camara) el juego se queda en modo normal,
//  sin AR, como en la prueba 1.
// =====================================================================================
[DefaultExecutionOrder(-100)]
public class MarcadorAR : MonoBehaviour
{
    [Header("Marcador")]
    public Texture2D marcador;                   // imagen que se imprime (Texturas/Marcador.png)
    public float anchoMarcadorMetros = 0.20f;    // lo que mide la hoja impresa, en metros

    [Header("Juego")]
    public ChessGame juego;                      // objeto Ajedrez
    public bool usarAR = true;                   // si esta apagado se juega como en la prueba 1

    // El marcador tiene 10 cuadros de ancho: 1 de margen, 8 del tablero y 1 de margen
    const float CuadrosMarcador = 10f;
    const float Oculto = 0.0001f;

    ImageTargetBehaviour target;
    bool visible;
    float perdidoDesde = -1f;
    string mensaje = "";
    bool enPlano;

    void Awake()
    {
        if (juego == null) juego = FindFirstObjectByType<ChessGame>();
        if (juego == null) { enabled = false; return; }

        if (!usarAR)
        {
            // Se apaga Vuforia para que no abra la camara
            VuforiaBehaviour vb = FindFirstObjectByType<VuforiaBehaviour>();
            if (vb != null) vb.enabled = false;
            PasarAModoPlano("Modo sin AR");
            return;
        }

        juego.modoAR = true;
        Ocultar();
        mensaje = "Iniciando la camara...";

        // Camara de AR: el tablero mide unos centimetros, el plano cercano tiene que ser chico
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
        }

        VuforiaApplication.Instance.OnVuforiaInitialized += AlIniciarVuforia;
        VuforiaApplication.Instance.OnVuforiaStarted += AlArrancarVuforia;
    }

    void OnDestroy()
    {
        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaInitialized -= AlIniciarVuforia;
            VuforiaApplication.Instance.OnVuforiaStarted -= AlArrancarVuforia;
        }
        if (target != null) target.OnTargetStatusChanged -= AlCambiarEstado;
    }

    // Vuforia avisa si pudo inicializar el SDK (licencia, camara, permisos)
    void AlIniciarVuforia(VuforiaInitError error)
    {
        if (error == VuforiaInitError.NONE) return;
        Debug.LogWarning("Vuforia no pudo iniciar: " + error + ". Se juega sin AR.");
        PasarAModoPlano("Vuforia no inicio (" + error + "). Modo sin AR");
    }

    // Ya con la camara corriendo se crea el marcador y se pega el juego a el
    void AlArrancarVuforia()
    {
        if (marcador == null)
        {
            PasarAModoPlano("Falta la imagen del marcador. Modo sin AR");
            return;
        }

        try
        {
            target = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(marcador, anchoMarcadorMetros, "TableroAjedrez");
        }
        catch (System.Exception e)
        {
            // Pasa sobre todo cuando falta la licencia en VuforiaConfiguration
            Debug.LogWarning("Vuforia no pudo crear el marcador: " + e.Message);
            target = null;
        }
        if (target == null)
        {
            PasarAModoPlano("Falta la licencia de Vuforia o el marcador. Modo sin AR");
            return;
        }

        target.OnTargetStatusChanged += AlCambiarEstado;
        juego.transform.SetParent(target.transform, false);
        juego.transform.localPosition = Vector3.zero;
        juego.transform.localRotation = Quaternion.identity;
        mensaje = "Apunta la camara al marcador";
        Ocultar();
    }

    // La API avisa cada vez que el marcador se encuentra o se pierde
    void AlCambiarEstado(ObserverBehaviour behaviour, TargetStatus estado)
    {
        bool visto = estado.Status == Status.TRACKED || estado.Status == Status.EXTENDED_TRACKED;
        if (visto)
        {
            perdidoDesde = -1f;
            if (!visible) Mostrar();
        }
        else if (visible && perdidoDesde < 0f)
        {
            perdidoDesde = Time.time;   // se oculta si pasa medio segundo sin verlo
        }
    }

    void Update()
    {
        if (visible && perdidoDesde >= 0f && Time.time - perdidoDesde > 0.5f)
        {
            Ocultar();
            mensaje = "Marcador perdido: apunta de nuevo";
        }
    }

    // El tablero mide 8 unidades; se escala para que mida lo mismo que los 8 cuadros del marcador
    void Mostrar()
    {
        visible = true;
        perdidoDesde = -1f;
        mensaje = "";
        float mundo = anchoMarcadorMetros / CuadrosMarcador;
        float padre = target != null ? Mathf.Max(1e-6f, target.transform.lossyScale.x) : 1f;
        juego.transform.localScale = Vector3.one * (mundo / padre);
    }

    void Ocultar()
    {
        visible = false;
        juego.transform.localScale = Vector3.one * Oculto;
    }

    // Sin AR: el juego se ve normal, como en la prueba 1
    void PasarAModoPlano(string texto)
    {
        enPlano = true;
        mensaje = texto;
        visible = false;
        if (target != null) target.OnTargetStatusChanged -= AlCambiarEstado;
        juego.transform.SetParent(null, true);
        juego.transform.position = Vector3.zero;
        juego.transform.rotation = Quaternion.identity;
        juego.transform.localScale = Vector3.one;
        juego.ModoPlano();
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(mensaje)) return;
        float s = Mathf.Max(1f, Screen.height / 600f);
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.fontSize = Mathf.RoundToInt(16 * s);
        st.alignment = TextAnchor.MiddleCenter;
        st.normal.textColor = enPlano ? new Color(1f, 0.8f, 0.3f) : Color.white;
        Rect r = new Rect(0f, Screen.height * 0.45f, Screen.width, Screen.height * 0.1f);
        GUI.Box(r, GUIContent.none);
        GUI.Label(r, mensaje, st);
    }
}
