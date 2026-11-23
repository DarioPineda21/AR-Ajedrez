#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ChessCore;

// Menu Ajedrez en la barra de Unity
public static class ChessMenu
{
    const string Raiz = "Assets/AjedrezAR";
    const string DirMat = Raiz + "/Materiales";
    const string DirTex = Raiz + "/Texturas";
    const string DirEsc = Raiz + "/Escenas";
    const string DirPre = Raiz + "/Prefabs";
    const string DirPzs = Raiz + "/Prefabs/Piezas";
    const string DirMod = Raiz + "/Modelos";

    static readonly string[] NombreTipo = { "", "Peon", "Caballo", "Alfil", "Torre", "Reina", "Rey" };

    // Arma todo el proyecto: texturas, materiales, prefabs de las piezas (a partir de los
    // modelos 3D de la carpeta Modelos) y las dos escenas con el tablero y las 32 piezas
    // ya puestas en el Hierarchy.
    [MenuItem("Ajedrez/Preparar proyecto (carpeta AjedrezAR completa)")]
    static void PrepararProyecto()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        foreach (string d in new string[] { DirMat, DirTex, DirEsc, DirPre, DirPzs, DirMod })
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);
        AssetDatabase.Refresh();

        // Texturas generadas por codigo
        Texture2D texMarmol = HacerTexturaMarmol(DirTex + "/Marmol.png");
        Texture2D texMadera = HacerTexturaMadera(DirTex + "/Madera.png");
        Texture2D texTablero = HacerTexturaTablero(DirTex + "/Tablero.png");

        // Materiales
        Material mTablero = HacerMaterial("Tablero", Color.white, texTablero, 0.25f);
        Material mBlanca = HacerMaterial("Pieza_Blanca", new Color(0.95f, 0.93f, 0.85f), texMarmol, 0.6f);
        Material mNegra = HacerMaterial("Pieza_Negra", new Color(0.16f, 0.16f, 0.19f), texMarmol, 0.6f);
        Material mMarco = HacerMaterial("Marco", new Color(0.22f, 0.14f, 0.10f), texMadera, 0.3f);
        Material mResalte = HacerMaterialResalte("Resaltado");

        // los materiales de la version anterior ya no se usan
        foreach (string viejo in new string[] { "Casilla_Clara", "Casilla_Oscura" })
            AssetDatabase.DeleteAsset(DirMat + "/" + viejo + ".mat");

        // Prefabs de las piezas: 6 tipos x 2 colores
        Pieza[] modelos = new Pieza[12];
        string[] rutas = new string[12];
        for (int t = 1; t <= 6; t++)
        {
            GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(DirMod + "/" + NombreTipo[t] + ".obj");
            if (modelo == null)
            {
                Debug.LogError("Falta el modelo " + DirMod + "/" + NombreTipo[t] + ".obj");
                return;
            }
            for (int c = 0; c < 2; c++)
            {
                bool blanca = c == 0;
                string nombre = NombreTipo[t] + (blanca ? " Blanco" : " Negro");
                if (t == 3 || t == 4 || t == 5) nombre = NombreTipo[t] + (blanca ? " Blanca" : " Negra");
                if (t == 3) nombre = "Alfil " + (blanca ? "Blanco" : "Negro");
                modelos[(t - 1) * 2 + c] = HacerPrefabPieza(nombre, modelo, (TipoPieza)t, blanca, blanca ? mBlanca : mNegra);
                rutas[(t - 1) * 2 + c] = DirPzs + "/" + nombre + ".prefab";
            }
        }

        // Escena de instrucciones
        string pathInstr = DirEsc + "/" + UiKit.EscenaInstrucciones + ".unity";
        string pathJuego = DirEsc + "/" + UiKit.EscenaJuego + ".unity";
        UnityEngine.SceneManagement.Scene s1 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera();
        new GameObject("Instrucciones").AddComponent<InstruccionesUI>();
        EditorSceneManager.SaveScene(s1, pathInstr);

        // Escena del juego con todo el arbol de objetos
        UnityEngine.SceneManagement.Scene s2 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera();
        GameObject luz = new GameObject("Luz del tablero");
        luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light l = luz.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.1f;
        // Al crear la escena nueva Unity descarga los assets sueltos: se vuelven a cargar desde su ruta
        for (int i = 0; i < 12; i++) modelos[i] = AssetDatabase.LoadAssetAtPath<Pieza>(rutas[i]);
        mTablero = AssetDatabase.LoadAssetAtPath<Material>(DirMat + "/Tablero.mat");
        mMarco = AssetDatabase.LoadAssetAtPath<Material>(DirMat + "/Marco.mat");
        mResalte = AssetDatabase.LoadAssetAtPath<Material>(DirMat + "/Resaltado.mat");
        GameObject juego = ConstruirJuego(modelos, mTablero, mMarco, mResalte);
        EditorSceneManager.SaveScene(s2, pathJuego);

        // El juego completo tambien como prefab (para ponerlo bajo el Image Target de Vuforia)
        PrefabUtility.SaveAsPrefabAsset(juego, DirPre + "/Ajedrez.prefab");

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(pathInstr, true),
            new EditorBuildSettingsScene(pathJuego, true),
        };

        if (AssetDatabase.LoadAssetAtPath<Object>("Assets/Scenes/SampleScene.unity") != null)
            AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(pathJuego);
        Debug.Log("Proyecto listo: abre la escena AjedrezAR y mira el Hierarchy (Ajedrez > Tablero > Plano, Casillas, Piezas).");
    }

    // ---------- Jerarquia del juego ----------
    //  Ajedrez (ChessGame, AIPlayer)
    //   +- Tablero (Tablero)
    //       +- Plano         el plano con la textura del tablero
    //       +- Marco
    //       +- Casillas      64 objetos con el script Casilla (a1 ... h8)
    //       +- Piezas
    //           +- Blancas   16 piezas con el script Pieza
    //           +- Negras    16 piezas con el script Pieza
    static GameObject ConstruirJuego(Pieza[] modelos, Material mTablero, Material mMarco, Material mResalte)
    {
        GameObject root = new GameObject("Ajedrez");
        root.AddComponent<ChessGame>();

        GameObject tab = new GameObject("Tablero");
        tab.transform.SetParent(root.transform, false);
        Tablero tablero = tab.AddComponent<Tablero>();

        // El plano: 10x10 de fabrica, se escala a 8x8 (una unidad por casilla)
        GameObject plano = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plano.name = "Plano";
        plano.transform.SetParent(tab.transform, false);
        plano.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Object.DestroyImmediate(plano.GetComponent<Collider>());
        plano.GetComponent<MeshRenderer>().sharedMaterial = mTablero;

        GameObject marco = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marco.name = "Marco";
        marco.transform.SetParent(tab.transform, false);
        marco.transform.localPosition = new Vector3(0f, -0.12f, 0f);
        marco.transform.localScale = new Vector3(8.8f, 0.2f, 8.8f);
        Object.DestroyImmediate(marco.GetComponent<Collider>());
        marco.GetComponent<MeshRenderer>().sharedMaterial = mMarco;

        // Casillas
        GameObject cas = new GameObject("Casillas");
        cas.transform.SetParent(tab.transform, false);
        for (int sq = 0; sq < 64; sq++)
        {
            string nombre = Sq.Name(sq);
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Casilla " + nombre;
            q.transform.SetParent(cas.transform, false);
            q.transform.localPosition = Tablero.SquareToLocal(sq) + new Vector3(0f, 0.01f, 0f);
            q.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            q.transform.localScale = new Vector3(0.96f, 0.96f, 1f);
            Object.DestroyImmediate(q.GetComponent<Collider>());
            MeshRenderer mr = q.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mResalte;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.enabled = false;
            Casilla c = q.AddComponent<Casilla>();
            c.nombre = nombre;
            c.archivo = (char)('a' + (sq & 7));
            c.fila = (sq >> 3) + 1;
            c.indice = sq;
            c.clara = (((sq & 7) + (sq >> 3)) % 2) == 1;
            c.marca = mr;
            tablero.casillas[sq] = c;
        }

        // Piezas
        GameObject piezas = new GameObject("Piezas");
        piezas.transform.SetParent(tab.transform, false);
        Transform blancas = new GameObject("Blancas").transform;
        Transform negras = new GameObject("Negras").transform;
        blancas.SetParent(piezas.transform, false);
        negras.SetParent(piezas.transform, false);

        tablero.piezas = piezas.transform;
        tablero.modelos = modelos;

        int[] fondo = { 4, 2, 3, 5, 6, 3, 2, 4 };   // torre, caballo, alfil, reina, rey, alfil, caballo, torre
        for (int f = 0; f < 8; f++)
        {
            Poner(modelos, blancas, fondo[f], true, f, 0);
            Poner(modelos, blancas, 1, true, f, 1);
            Poner(modelos, negras, 1, false, f, 6);
            Poner(modelos, negras, fondo[f], false, f, 7);
        }

        Selection.activeGameObject = root;
        return root;
    }

    static void Poner(Pieza[] modelos, Transform padre, int tipo, bool blanca, int archivo, int fila)
    {
        Pieza modelo = modelos[(tipo - 1) * 2 + (blanca ? 0 : 1)];
        Pieza p = (Pieza)PrefabUtility.InstantiatePrefab(modelo, padre);
        int sq = fila * 8 + archivo;
        p.name = modelo.name + " " + Sq.Name(sq);
        p.nombre = p.name;
        p.casillaInicial = sq;
        p.Colocar(sq);
        p.transform.localPosition = Tablero.SquareToLocal(sq);
        p.transform.localRotation = Quaternion.Euler(0f, blanca ? 0f : 180f, 0f);
    }

    static Pieza HacerPrefabPieza(string nombre, GameObject modelo, TipoPieza tipo, bool blanca, Material mat)
    {
        GameObject go = new GameObject(nombre);
        Pieza p = go.AddComponent<Pieza>();
        p.nombre = nombre;
        p.tipo = tipo;
        p.blanca = blanca;
        p.valor = Pieza.ValorDe(tipo);

        GameObject m = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
        m.name = "Modelo 3D";
        m.transform.SetParent(go.transform, false);
        foreach (MeshRenderer r in m.GetComponentsInChildren<MeshRenderer>())
            r.sharedMaterial = mat;

        string path = DirPzs + "/" + nombre + ".prefab";
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return saved.GetComponent<Pieza>();
    }

    // Guarda el proyecto como paquete para entregarlo
    [MenuItem("Ajedrez/Exportar .unitypackage")]
    static void Exportar()
    {
        if (!Directory.Exists("Entregables")) Directory.CreateDirectory("Entregables");
        string path = "Entregables/AjedrezAR.unitypackage";
        AssetDatabase.ExportPackage(Raiz, path, ExportPackageOptions.Recurse);
        Debug.Log("Paquete exportado: " + Path.GetFullPath(path));
    }

    // ---------- Texturas y materiales ----------

    static Texture2D GuardarTextura(Texture2D t, string path)
    {
        t.Apply();
        File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Texture2D HacerTexturaMadera(string path)
    {
        const int n = 256;
        Texture2D t = new Texture2D(n, n, TextureFormat.RGB24, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float v = 0.80f + 0.20f * Mathf.PerlinNoise(x * 0.015f, y * 0.35f);
                t.SetPixel(x, y, new Color(v, v, v));
            }
        return GuardarTextura(t, path);
    }

    static Texture2D HacerTexturaMarmol(string path)
    {
        const int n = 256;
        Texture2D t = new Texture2D(n, n, TextureFormat.RGB24, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float w = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                float vein = Mathf.Abs(Mathf.Sin((x + y) * 0.04f + w * 8f));
                float v = 0.88f + 0.12f * Mathf.Pow(vein, 0.3f);
                t.SetPixel(x, y, new Color(v, v, v));
            }
        return GuardarTextura(t, path);
    }

    // Tablero de 8x8 casillas claras y oscuras con veta de madera. a1 es oscura.
    static Texture2D HacerTexturaTablero(string path)
    {
        const int n = 1024;
        Color clara = new Color(0.86f, 0.78f, 0.62f);
        Color oscura = new Color(0.42f, 0.28f, 0.20f);
        Texture2D t = new Texture2D(n, n, TextureFormat.RGB24, true);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int f = x * 8 / n;
                int r = y * 8 / n;
                Color b = (((f + r) % 2) == 0) ? oscura : clara;
                float g = 0.85f + 0.15f * Mathf.PerlinNoise(x * 0.01f + f * 3.1f, y * 0.12f + r * 1.7f);
                // lineita fina entre casillas
                int mx = x % (n / 8), my = y % (n / 8);
                float borde = (mx < 2 || my < 2) ? 0.88f : 1f;
                t.SetPixel(x, y, new Color(b.r * g * borde, b.g * g * borde, b.b * g * borde));
            }
        return GuardarTextura(t, path);
    }

    static Material HacerMaterial(string nombre, Color color, Texture2D tex, float suavidad)
    {
        string path = DirMat + "/" + nombre + ".mat";
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, path);
        }
        m.shader = sh;
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (tex != null)
        {
            m.mainTexture = tex;
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", suavidad);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", suavidad);
        EditorUtility.SetDirty(m);
        return m;
    }

    // Material semitransparente para marcar casillas (shader sin luz)
    static Material HacerMaterialResalte(string nombre)
    {
        string path = DirMat + "/" + nombre + ".mat";
        Shader sh = Shader.Find("Sprites/Default");
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, path);
        }
        m.shader = sh;
        m.color = Color.white;
        EditorUtility.SetDirty(m);
        return m;
    }

    static void MakeCamera()
    {
        GameObject c = new GameObject("Main Camera");
        c.tag = "MainCamera";
        Camera cam = c.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.13f);
        c.AddComponent<AudioListener>();
    }

    // Prueba rapida (solo en Play): pone 3 reinas blancas para comprobar que el
    // tablero crea piezas extra cuando un peon corona.
    [MenuItem("Ajedrez/Probar piezas extra (solo en Play)")]
    static void ProbarExtra()
    {
        Tablero t = Object.FindFirstObjectByType<Tablero>();
        if (t == null || !Application.isPlaying) { Debug.Log("Primero dale Play a la escena AjedrezAR."); return; }
        t.Rebuild(Board.FromFen("4k3/8/8/8/8/8/8/QQQ1K3 w - - 0 1"));
        int reinas = t.GetComponentsInChildren<Pieza>().Count(p => p.tipo == TipoPieza.Reina && p.blanca);
        Debug.Log("Reinas blancas activas: " + reinas + " (deben ser 3)");
    }

    // Cuenta jugadas legales y las compara con los valores conocidos.
    // Si alguna no coincide, el motor de reglas tiene un error.
    [MenuItem("Ajedrez/Correr perft (prueba del motor)")]
    static void RunPerft()
    {
        object[][] tests = new object[][]
        {
            new object[] { "inicial", Board.StartFen, new long[] { 20, 400, 8902, 197281 } },
            new object[] { "kiwipete", "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", new long[] { 48, 2039, 97862 } },
            new object[] { "posicion 3", "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", new long[] { 14, 191, 2812, 43238 } },
            new object[] { "posicion 4", "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", new long[] { 6, 264, 9467 } },
            new object[] { "posicion 5", "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", new long[] { 44, 1486, 62379 } },
        };

        int bad = 0;
        foreach (object[] t in tests)
        {
            Board b = Board.FromFen((string)t[1]);
            long[] exp = (long[])t[2];
            for (int d = 1; d <= exp.Length; d++)
            {
                long n = b.Perft(d);
                bool ok = n == exp[d - 1];
                if (!ok) bad++;
                Debug.Log((ok ? "OK    " : "FALLA ") + t[0] + " profundidad " + d + ": " + n + " (esperado " + exp[d - 1] + ")");
            }
        }
        Debug.Log(bad == 0 ? "Perft: todo bien" : "Perft: hay " + bad + " fallas");
    }
}
#endif
