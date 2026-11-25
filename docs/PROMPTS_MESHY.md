# The Last Gambit: prompts de Meshy AI

Prompts para generar las piezas, el escenario y las pantallas del juego en Meshy AI. Estilo: cómic entintado a mano con neón cyberpunk y detalles retro de los 80. La tripulación (blancas) usa aro cian y los insectos marcianos (negras) aro magenta. Cada prompt mide 600 caracteres o menos.

## Flujo en Meshy

1. Texto a imagen con el prompt de la pieza. Pedir vista múltiple y elegir la imagen con una sola pieza sólida sobre fondo liso.
2. Imagen a 3D con textura PBR.
3. Retopología con Remesh o Smart Topology: 6,000 caras por pieza, 10,000 para la Reina Madre, 12,000 para el tablero y 3,000 por prop.
4. Exportar en FBX con textura y guardarlo en `Assets/AjedrezAR/Modelos/`.

## Bloque de estilo

```text
Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches
```

## Pantallas

1. Inicio animado: elige juego libre contra la IA o campaña.
2. Instrucciones: objetivo y controles, sin AR.
3. Actividad AR: tablero sobre el marcador. La campaña (nivel 1, nivel 2, nivel 3 y jefe final) ocurre aquí, con una pantalla de carga antes de cada nivel.

## Prompts de las piezas y el jefe final

### Tripulación (blancas)

**Peón: cadete niño** (585 caracteres)

```text
A cheerful boy space cadet about eight years old, short brown hair, glowing cyan goggles pushed up on his forehead, navy jumpsuit with cyan neon trim, small backpack with a glowing cable, one hand waving, the smallest piece. Single solid character, upright, centered on a low round base with a cyan neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Torre: cohete de pie** (568 caracteres)

```text
An upright weathered white rocket with a rounded nose cone, cyan neon stripes, four stubby fins and a scaffold ring with small LED lights around its middle, holographic patch, shaped like a chess rook tower. Single solid character, upright, centered on a low round base with a cyan neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Caballo: corgi astronauta** (549 caracteres)

```text
A happy corgi dog in a clear bubble helmet with a faint cyan HUD glow, small white spacesuit with a neon light collar, rearing up on its hind legs like a chess knight, tail up, tongue out. Single solid character, upright, centered on a low round base with a cyan neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Alfil: astronauta con casco de mitra** (562 caracteres)

```text
A calm astronaut in a bulky white spacesuit with cyan neon trim, helmet topped with a tall pointed mitre-shaped crest with a glowing diagonal visor slit, hands folded in front, holographic chest patch. Single solid character, upright, centered on a low round base with a cyan neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Reina: ingeniera** (581 caracteres)

```text
A confident woman engineer with long brown hair and a cyan neon streak, dark jumpsuit with glowing seams, tool belt, big wrench like a scepter, a ring of small glowing antennae on her head like a crown, tall and elegant. Single solid character, upright, centered on a low round base with a cyan neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Rey: capitán** (560 caracteres)

```text
A wild curly-haired captain in a dark jacket with cyan neon trim and tan trousers, laughing and pointing forward, small glowing cross-shaped antenna on a cap, the tallest piece, friendly heroic pose. Single solid character, upright, centered on a low round base with a cyan neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

### Insectos marcianos (negras)

**Peón: grillo** (568 caracteres)

```text
A small friendly cricket with big round eyes and long antennae, black-violet shell with glowing magenta circuit lines, tiny clear bubble helmet with a magenta HUD glow, one arm waving, the smallest piece. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Torre: termitero** (557 caracteres)

```text
A tall termite mound tower in dark purple clay with arched windows glowing magenta, a battlement top with small antenna spires and two tiny termite guards at the door, shaped like a chess rook. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Caballo: saltamontes** (566 caracteres)

```text
A grasshopper in black and violet with magenta neon stripes on its legs, rearing up on long bent hind legs like a chess knight, horse-like head with large eyes and curved antennae, springy dynamic pose. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Alfil: mantis** (557 caracteres)

```text
A tall praying mantis in black and violet with glowing magenta veins, folded forelegs in front, pointed mitre-shaped head crest with a glowing diagonal slit, big calm eyes, small cape of wings. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Reina: escarabajo reina** (544 caracteres)

```text
A regal beetle queen with a glossy black shell cape edged in magenta neon, a crown of glowing curved antennae, six small legs, haughty eyes, tall and elegant, cartoon villain look. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Rey: escarabajo rinoceronte** (535 caracteres)

```text
A large rhinoceros beetle king in black armor with magenta neon seams, a big curved horn glowing like a crown, heavy shoulders, stern but friendly eyes, the tallest piece. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

### Jefe final

**Jefe final: Reina Madre** (584 caracteres)

```text
A giant Martian insect queen, twice the size of a normal piece, black-violet carapace with magenta neon cracks, spiked glowing crown, bright magenta egg sac on her back, four arms, long antennae, imposing but cartoonish. Single solid character, upright, centered on a low round base with a magenta neon ring, plain gray background, full body, front view, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

## Marcador, marco del tablero y props

**Imagen del marcador de seguimiento** (364 caracteres)

```text
Flat top-down view of a hand-drawn Martian ground map filling the whole square, many craters of varied sizes, rocks, cracks glowing with neon magenta and cyan lines like circuits, small plants and footprints, irregular asymmetric layout with no repeated patterns, no empty areas, strong light and dark contrast, thin dark ink lines, no text, no border, no horizon.
```

**Marco decorativo del tablero** (562 caracteres)

```text
A flat square picture-frame border for a chess board, chunky weathered metal plates and rocks with rivets, neon cyan light strips and small warning stripes, empty square opening in the center, low profile, the sides tilted like a crashed spaceship hull. Single solid object, centered, three-quarter view, plain light gray background, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches
```

**Prop 1: nave estrellada** (484 caracteres)

```text
A crashed white rocket lying tilted in a pile of dark red dirt, rusty panels with cyan neon stripes, a broken scaffold with flickering lights and a cracked capsule next to it. Single solid object, centered, three-quarter view, plain light gray background, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches
```

**Prop 2: rover** (459 caracteres)

```text
A small rugged six-wheel Mars rover with a tall antenna, boxy beige body, big chunky tires, cyan neon underglow, a solar panel on top and dust stains. Single solid object, centered, three-quarter view, plain light gray background, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches
```

**Prop 3: rocas y cristales** (448 caracteres)

```text
A small cluster of dark volcanic rocks around a round crater rim with glowing magenta crystals, spiky alien plants and tiny neon mushrooms. Single solid object, centered, three-quarter view, plain light gray background, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches
```

**Prop 4: baliza de extracción** (459 caracteres)

```text
A tall extraction beacon: a metal pole on a heavy base with a cyan neon light on top, a small holographic dish and striped flags, weathered and dusty. Single solid object, centered, three-quarter view, plain light gray background, no scene. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches
```

## Portada, pantallas de carga, victoria y derrota

**Portada de la pantalla de inicio** (595 caracteres)

```text
A young space crew (curly-haired captain, woman engineer, boy cadet, corgi in bubble helmet) runs toward the viewer on a neon-lit Mars plain, a giant glowing chess knight hologram behind them, a synthwave sunset over a grid horizon, a crashed rocket, bugs peeking from rocks. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Pantalla de carga genérica** (509 caracteres)

```text
A tiny cricket in a bubble helmet waves from a rock beside a crashed rocket on an empty dark plain, a big synthwave sun with a faint grid on the horizon, two pale moons, lots of open space. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Carga del nivel 1: Cráter** (523 caracteres)

```text
A boy cadet and a corgi astronaut at the rim of a wide crater at night, cyan glow from their gear, scout crickets and a grasshopper with magenta eyes watching from boulders, a lone rover in the distance. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Carga del nivel 2: Hábitat abandonado** (507 caracteres)

```text
An abandoned glass dome habitat on Mars with termite mounds growing through its walls, flickering cyan and magenta neon signs, broken solar panels, dusty floor, small bugs in the windows. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Carga del nivel 3: Tormenta de polvo** (537 caracteres)

```text
A swirling dust storm lit purple and magenta over a dark plain, black beetle silhouettes with glowing eyes marching through it, a cyan extraction beacon glowing in the distance, a crew member bracing against the wind. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Carga del jefe final** (541 caracteres)

```text
A giant insect queen with a spiked glowing crown rises from a huge nest crater with bright magenta eggs, the small crew with cyan lights standing in the foreground looking up, dramatic purple sky and retro grid lightning. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Victoria** (503 caracteres)

```text
The crew cheers beside a planted flag at a neon synthwave sunrise, the corgi jumping, a friendly cricket in a bubble helmet joining them, both moons glowing, warm pink and gold light. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```

**Derrota** (505 caracteres)

```text
The tired crew sits beside the rover at night, the corgi resting its head on the boy's lap, bugs far away on a ridge with magenta eyes, a hopeful cyan light on the horizon, gentle mood. Vertical 9:16 composition, calm empty space at the top and bottom for interface, no text, no logos. Hand-inked cartoon style, thick dark outlines, soft watercolor shading, paper grain, cyberpunk neon glow in magenta and cyan on dark violet, retro 80s synthwave details, chunky friendly proportions, holographic patches.
```
