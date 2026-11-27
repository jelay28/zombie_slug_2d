// -----------------------------------------------------------------------------
// GameEvents.cs  (Zombie Slug - Laboratorio de Software, módulo 2)
// Bus de eventos del juego. Los subsistemas avisan "hacia adelante"
// (combate -> puntaje -> interfaz) y nadie consulta hacia atrás, de modo que
// las dependencias van en una sola dirección (ver diagrama de subsistemas).
// Usar eventos evita que la interfaz o el puntaje revisen datos cada cuadro.
// -----------------------------------------------------------------------------
using System;

namespace ZombieSlug
{
    public enum EstadoJuego { Preparacion, Oleada, Tienda, Pausa, Victoria, Derrota }

    public static class GameEvents
    {
        // Combate -> Puntaje / Oleadas
        public static event Action<EnemyBase, bool> EnemigoEliminado;   // enemigo, fue con hacha
        // Jugador -> Puntaje / Interfaz
        public static event Action<int, int> SaludCambio;              // salud actual, salud máxima
        public static event Action<int> VidasCambio;                   // vidas extra
        public static event Action JugadorDanado;
        public static event Action JugadorSinVidas;
        public static event Action<string, int> ArmaCambio;            // nombre del arma, munición (-1 = infinita)
        // Oleadas -> Puntaje / GameManager / Interfaz
        public static event Action<int> OleadaIniciada;                // número de oleada
        public static event Action<int, float> OleadaTerminada;        // número, segundos de limpieza
        // Puntaje -> Interfaz
        public static event Action<int, int> PuntajeCambio;            // puntos, monedas
        // GameManager -> Interfaz / Tienda
        public static event Action<EstadoJuego> EstadoCambio;

        public static void RaiseEnemigoEliminado(EnemyBase e, bool hacha) { if (EnemigoEliminado != null) EnemigoEliminado(e, hacha); }
        public static void RaiseSaludCambio(int actual, int max) { if (SaludCambio != null) SaludCambio(actual, max); }
        public static void RaiseVidasCambio(int vidas) { if (VidasCambio != null) VidasCambio(vidas); }
        public static void RaiseJugadorDanado() { if (JugadorDanado != null) JugadorDanado(); }
        public static void RaiseJugadorSinVidas() { if (JugadorSinVidas != null) JugadorSinVidas(); }
        public static void RaiseArmaCambio(string nombre, int municion) { if (ArmaCambio != null) ArmaCambio(nombre, municion); }
        public static void RaiseOleadaIniciada(int n) { if (OleadaIniciada != null) OleadaIniciada(n); }
        public static void RaiseOleadaTerminada(int n, float limpieza) { if (OleadaTerminada != null) OleadaTerminada(n, limpieza); }
        public static void RaisePuntajeCambio(int puntos, int monedas) { if (PuntajeCambio != null) PuntajeCambio(puntos, monedas); }
        public static void RaiseEstadoCambio(EstadoJuego e) { if (EstadoCambio != null) EstadoCambio(e); }
    }
}
