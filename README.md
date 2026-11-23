# Ajedrez AR

Proyecto de Temas Selectos de Programación (FI-UNAM, grupo 10). Ajedrez en realidad aumentada hecho en Unity para celular (Android). Se juega contra una IA o contra otra persona tocando la pantalla.

Equipo: Barcenas Torres David Farid y Pineda Sánchez Darío.

## Cómo abrirlo

1. Unity 6 (6000.0.36f1) con la plantilla URP.
2. En Unity Hub: Add > Add project from disk y elegir esta carpeta.
3. Abrir la escena `Assets/AjedrezAR/Escenas/AjedrezAR.unity` y darle Play.

También se puede importar `Entregables/AjedrezAR.unitypackage` en un proyecto 3D URP vacío.

## Estructura

```
Assets/AjedrezAR/
  Scripts/    Core (motor de ajedrez e IA), Game (tablero, piezas, interfaz), Editor (menú)
  Modelos/    modelos 3D de las piezas (.obj)
  Materiales/ Tablero, Marco, Pieza_Blanca, Pieza_Negra, Resaltado
  Texturas/   Tablero, Madera, Marmol
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
- P2 API: Vuforia (pendiente).
- P3 Base de datos: pendiente.

## Menú del editor

`Ajedrez > Preparar proyecto` regenera materiales, prefabs y escenas. `Ajedrez > Exportar .unitypackage` crea el paquete en `Entregables/`.
