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
  Modelos/    Blancas y Negras: las 12 piezas de Meshy (.obj). Los .obj sueltos son los modelos viejos de la prueba 1
  Materiales/ una por pieza y color (Peon_Blanca ... Rey_Negra), Resaltado y los de la prueba 1
  Texturas/   Piezas (12 jpg de 2048 px), Marcador y las de la prueba 1
  Resources/Tablero3D/  el tablero de Meshy (.obj, textura y material); Tablero.cs lo carga al iniciar
docs/         PROMPTS_MESHY.md (prompts de Meshy AI para el arte y los modelos)
  Prefabs/    Ajedrez y Piezas (12 prefabs, uno por tipo y color)
  Escenas/    Instrucciones y AjedrezAR
```

Jerarquía de la escena del juego:

```
Ajedrez
└─ Tablero
   ├─ Tablero 3D  se crea al iniciar con el modelo de Resources/Tablero3D
   ├─ Plano       el plano de la prueba 1 (se apaga si existe el tablero 3D)
   ├─ Marco       igual, se apaga
   ├─ Casillas    64 casillas (a1 ... h8) para resaltar jugadas
   └─ Piezas
      ├─ Blancas  16 piezas, cada una con su script Pieza
      └─ Negras   16 piezas
```

## Modelos 3D de Meshy

Las piezas y el tablero salen de Meshy y se redujeron de caras para que corran en el celular: piezas de 5 000 a 8 000 caras y tablero de 60 000. Cada casilla mide 1 unidad. Alturas: peón 0.8, torre 1.0, caballo 1.1, alfil 1.2, reina 1.4, rey 1.5. Todas tienen el pivote en el centro de la base. Las blancas son las verde neón y las negras las naranja y rosa. El tablero queda con su superficie de juego en y = 0 y se sube 1.28 para que la base apoye en el marcador. Al abrirlo, la casilla a1 debe verse oscura.

## Pantallas

- Instrucciones: sin AR, explica el objetivo (dar jaque mate a la IA) y cómo jugar.
- AjedrezAR: la actividad. Panel táctil con Nueva partida, Deshacer, nivel de la IA y modo de juego.

## Prácticas de la clase

- P1 Concurrencia: la IA calcula en un hilo secundario (`AIPlayer`) para no congelar el juego.
- P2 API: Vuforia Engine. `MarcadorAR` crea un Image Target en tiempo de ejecución con `ObserverFactory.CreateImageTarget`, cuelga el tablero de él y lo muestra o esconde con `OnTargetStatusChanged`. La comunicación es por eventos de `VuforiaApplication` y del observer.
- P3 Base de datos: pendiente.

## Menú del editor

`Ajedrez > Preparar proyecto` regenera materiales, prefabs y escenas con los modelos viejos de la prueba 1, así que ya no conviene usarlo: borraría las piezas y materiales de Meshy. `Ajedrez > Configurar AR con Vuforia (P2)` prepara la cámara AR, el marcador y Android. `Ajedrez > Exportar .unitypackage` crea el paquete en `Entregables/`.

## Siguiente

El plan es llamar al juego The Last Gambit, con estilo cyberpunk retro, una pantalla de inicio animada (juego libre contra la IA o campaña de 3 niveles con jefe final) y pantallas de carga. Los prompts para generar modelos y arte están en `docs/PROMPTS_MESHY.md`. Todavía falta la base de datos (P3), que no está implementada.
