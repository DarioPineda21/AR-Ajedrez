#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vuforia;

// Practica 2 (API Vuforia): deja la escena AjedrezAR lista para realidad aumentada.
// Menu: Ajedrez > Configurar AR con Vuforia (P2)
//  - la imagen del marcador queda legible (hace falta para crear el Image Target)
//  - la Main Camera pasa a ser la ARCamera de Vuforia (VuforiaBehaviour)
//  - se agrega el objeto "Marcador AR" con el script MarcadorAR
//  - ajustes de Android que pide Vuforia (IL2CPP, ARM64, OpenGLES3)
public static class ConfigurarAR
{
    const string RutaMarcador = "Assets/AjedrezAR/Texturas/Marcador.png";
    const string RutaEscena = "Assets/AjedrezAR/Escenas/AjedrezAR.unity";

    [MenuItem("Ajedrez/Configurar AR con Vuforia (P2)")]
    static void Configurar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // 1) Imagen del marcador: sin compresion y legible
        TextureImporter ti = AssetImporter.GetAtPath(RutaMarcador) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError("Falta la imagen " + RutaMarcador);
            return;
        }
        ti.textureType = TextureImporterType.Default;
        ti.isReadable = true;
        ti.mipmapEnabled = false;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 2048;
        TextureImporterPlatformSettings android = ti.GetPlatformTextureSettings("Android");
        android.overridden = true;
        android.format = TextureImporterFormat.RGBA32;
        android.maxTextureSize = 2048;
        ti.SetPlatformTextureSettings(android);
        ti.SaveAndReimport();
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(RutaMarcador);

        // 2) Escena del juego
        EditorSceneManager.OpenScene(RutaEscena);
        ChessGame juego = Object.FindFirstObjectByType<ChessGame>();
        if (juego == null)
        {
            Debug.LogError("No hay un objeto Ajedrez en la escena. Corre primero Ajedrez > Preparar proyecto.");
            return;
        }

        // 3) La camara principal se vuelve la ARCamera de Vuforia
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No hay Main Camera en la escena.");
            return;
        }
        cam.gameObject.name = "ARCamera";
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 100f;
        if (cam.GetComponent<VuforiaBehaviour>() == null) cam.gameObject.AddComponent<VuforiaBehaviour>();

        // 4) Objeto con el script del marcador
        MarcadorAR m = Object.FindFirstObjectByType<MarcadorAR>();
        if (m == null) m = new GameObject("Marcador AR").AddComponent<MarcadorAR>();
        m.marcador = tex;
        m.juego = juego;
        m.usarAR = true;
        if (m.anchoMarcadorMetros <= 0f) m.anchoMarcadorMetros = 0.20f;
        EditorUtility.SetDirty(m);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        // 5) Ajustes de Android para Vuforia
        try
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se pudieron poner los ajustes de Android: " + e.Message);
        }

        // 6) Abre la configuracion de Vuforia para pegar la licencia
        try { Selection.activeObject = VuforiaConfiguration.Instance; }
        catch (System.Exception e) { Debug.LogWarning("No se pudo abrir VuforiaConfiguration: " + e.Message); }

        AssetDatabase.SaveAssets();
        Debug.Log("AR lista. Falta pegar la licencia de Vuforia en VuforiaConfiguration > License Key (no se sube a GitHub).");
    }
}
#endif
