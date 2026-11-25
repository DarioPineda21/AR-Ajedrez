# Ajedrez AR

Proyecto de Temas Selectos de Programación (FI-UNAM, grupo 10). Ajedrez en realidad aumentada hecho en Unity para celular (Android). Se juega contra una IA o contra otra persona tocando la pantalla.

Equipo: Barcenas Torres David Farid y Pineda Sánchez Darío.

## Cómo abrirlo

1. Unity 6 (6000.0.36f1) con la plantilla URP.
2. En Unity Hub: Add > Add project from disk y elegir esta carpeta. Unity baja Vuforia Engine 11.4.4 desde el registro de Vuforia que está en `Packages/manifest.json`.
3. Poner la licencia de Vuforia (ver abajo).
4. Abrir la escena `Assets/AjedrezAR/Escenas/AjedrezAR.unity` y darle Play.

Si se importa `Entregables/AjedrezAR.unitypackage` en un proyecto 3D URP vacío, primero hay que instalar Vuforia Engine 11.4.4 (Package Manager), porque los scripts de AR lo usan.

## Vuforia y el marcador (P2)

La licencia de Vuforia no está en el repositorio (`VuforiaConfiguration.asset` está en el `.gitignore`). Cada quien pone la suya:

1. Crear una licencia gratis en developer.vuforia.com (License Manager).
2. En Unity: Ajedrez > Configurar AR con Vuforia (P2). Esto deja la cámara AR, el marcador y los ajustes de Android listos y selecciona `VuforiaConfiguration`.
3. Pegar la clave en el campo License Key de `VuforiaConfiguration`.
4. Imprimir `Entregables/Marcador_para_imprimir.pdf` al 100% (cuadrado de 20 cm) y apuntarle la cámara. El tablero aparece sobre el marcador.

Sin licencia o sin marcador el juego sigue funcionando en modo plano, con una cámara fija y un aviso en pantalla.

## Estructura

```
Assets/AjedrezAR/
  Scripts/    Core (motor de ajedrez e IA), Game (tablero, piezas, interfaz), AR (marcador con Vuforia), Editor (menús)
  Modelos/    modelos 3D de las piezas (.obj)
  Materiales/ Tablero, Marco, Pieza_Blanca, Pieza_Negra, Resaltado
  Texturas/   Tablero, Madera, Marmol, Marcador
docs/         PROMPTS_MESHY.md (prompts de Meshy AI para el arte y los modelos)
  Prefabs/    Ajedrez y Piezas (12 prefabs, uno por tipo y color)
  Escenas/    Instrucciones y AjedrezAR
```

Jerarquía de la escena del juego:

```
Ajedrez
└─ Tablero
   ├─ Plano       el plano con la textura del tablero
   ├─ Marco
   ├─ Casillas    64 casillas (a1 ... h8) para resaltar jugadas
   └─ Piezas
      ├─ Blancas  16 piezas, cada una con su script Pieza
      └─ Negras   16 piezas
```

## Pantallas

- Instrucciones: sin AR, explica el objetivo (dar jaque mate a la IA) y cómo jugar.
- AjedrezAR: la actividad. Panel táctil con Nueva partida, Deshacer, nivel de la IA y modo de juego.

## Prácticas de la clase

- P1 Concurrencia: la IA calcula en un hilo secundario (`AIPlayer`) para no congelar el juego.
- P2 API: Vuforia Engine. `MarcadorAR` crea un Image Target en tiempo de ejecución con `ObserverFactory.CreateImageTarget`, cuelga el tablero de él y lo muestra o esconde con `OnTargetStatusChanged`. La comunicación es por eventos de `VuforiaApplication` y del observer.
- P3 Base de datos: pendiente.

## Menú del editor

`Ajedrez > Preparar proyecto` regenera materiales, prefabs y escenas. `Ajedrez > Configurar AR con Vuforia (P2)` prepara la cámara AR, el marcador y Android. `Ajedrez > Exportar .unitypackage` crea el paquete en `Entregables/`.

## Siguiente

El plan es llamar al juego The Last Gambit, con estilo cyberpunk retro, una pantalla de inicio animada (juego libre contra la IA o campaña de 3 niveles con jefe final) y pantallas de carga. Los prompts para generar modelos y arte están en `docs/PROMPTS_MESHY.md`. Todavía falta la base de datos (P3), que no está implementada.
