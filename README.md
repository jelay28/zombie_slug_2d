# Zombie Slug (zombie_slug_2d)

Arcade de disparos 2D por oleadas. Proyecto de seguimiento de Laboratorio de Software
(Corporación Universitaria Americana, Ingeniería de Sistemas), módulo 2. Autor: Jhon Layton.

## Requisitos
- Unity **6000.3.26f1 (Unity 6.3 LTS)**, plantilla Universal 2D (URP 2D). No abrir con otra versión.
- Paquetes: los de `Packages/manifest.json` (Input System, 2D, URP, uGUI).

## Reproducir el proyecto en otra máquina
1. Clonar el repositorio y abrir la carpeta con Unity Hub (versión indicada arriba).
2. Esperar la importación inicial (la carpeta `Library` no se versiona; Unity la regenera).
3. Abrir `Assets/_ZombieSlug/Scenes/Menu.unity` y pulsar Play.
4. Si algo falta o se quiere regenerar desde cero: menú **Zombie Slug > Construir todo**.
   Configura capas, etiquetas, matriz de colisiones, importación de sprites (32 px por unidad,
   filtro Point, sin compresión), datos de balance, animaciones, prefabs y las escenas
   Menu, Juego y Resultado.

## Controles
A/D o flechas: mover · Espacio: saltar · S + Espacio: bajar de plataforma · J o clic: disparar ·
W/S: apuntar arriba/abajo · Q: cambiar arma · E: tablero de Don Kuro · Enter: siguiente oleada ·
Esc/P: pausa.

Herramienta de prueba (solo editor o build de desarrollo): **F8** elimina a los zombis activos
para recorrer las 10 oleadas en pocos minutos.

## Estructura
```
Assets/_ZombieSlug/
  Animations/  Art/ (Fondos, Sprites, Tiles)  Editor/  Prefabs/
  Scenes/ (Menu, Juego, Resultado)  ScriptableObjects/ (Armas, Oleadas)
  Scripts/ (Core, Jugador, Enemigos, Combate, Oleadas, Escenario, Tienda, UI, Datos)
Tools/generar_placeholders.py   genera los 36 PNG provisionales con sus medidas finales
```

## Historial de iteraciones (prueba de juego del 26/11/2026)
1. Ken y los zombis caían a través del suelo: se cambió el colisionador del Tilemap por cajas
   primitivas (suelo 20 x 1 m y 3 plataformas de un solo sentido) y se forzó la recompilación.
2. Aviso de tienda demasiado largo: se acortó a "TIENDA ABIERTA".
3. Zombis atascados encima de Ken en la plataforma alta: conservan la dirección mientras Ken
   esté más de 1,5 m por debajo y caen por un borde.
4. El hacha mataba al caminante antes de que tocara a Ken: retardo de 0,5 s antes del primer tajo.
5. Sin panel de derrota: `WaitForEndOfFrame` se cambió por esperar al cuadro siguiente.
6. Dependencias en un solo sentido: interfaz `IDanable` (Combate) y eventos `AyudaTablero` y
   `TiendaVisible`; se rompieron los ciclos Jugador-Enemigos y Tienda-Interfaz.

## Arte
Todo el arte de personajes es provisional (placeholders con las dimensiones finales).
Inventario, licencias y fechas de sustitución en el informe del módulo 2.
