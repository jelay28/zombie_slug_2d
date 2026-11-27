// -----------------------------------------------------------------------------
// ScoreManager.cs - Subsistema de puntaje y economía.
// Puntaje = Σ base + Σ oleada limpia + 5 × eliminaciones con hacha
//           + Σ max(0, 20 − t) × 5      (t = segundos de limpieza de la oleada)
// Monedas = puntos ganados / 10 (el residuo se conserva para la siguiente suma).
// Reglas: R-05 (puntos base), R-06 (bono hacha), R-07 (bono oleada limpia),
// R-08 (bono de tiempo), R-09 (conversión a monedas y gasto).
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Variables de balance")]
        [SerializeField] private int puntosPorMoneda = 10;
        [SerializeField] private int bonoHacha = 5;
        [SerializeField] private float segundosReferenciaLimpieza = 20f;
        [SerializeField] private int puntosPorSegundoAhorrado = 5;

        public int Puntos { get; private set; }
        public int Monedas { get; private set; }

        private int residuoMonedas;      // puntos que aún no completan una moneda
        private int puntosBaseOleada;    // puntos base ganados en la oleada en curso
        private bool recibioDanoEnOleada;

        private void OnEnable()
        {
            GameEvents.EnemigoEliminado += AlEliminar;
            GameEvents.OleadaIniciada += AlIniciarOleada;
            GameEvents.OleadaTerminada += AlTerminarOleada;
            GameEvents.JugadorDanado += AlRecibirDano;
        }

        private void OnDisable()
        {
            GameEvents.EnemigoEliminado -= AlEliminar;
            GameEvents.OleadaIniciada -= AlIniciarOleada;
            GameEvents.OleadaTerminada -= AlTerminarOleada;
            GameEvents.JugadorDanado -= AlRecibirDano;
        }

        private void Start() { Notificar(); }

        private void AlIniciarOleada(int n) { puntosBaseOleada = 0; recibioDanoEnOleada = false; }
        private void AlRecibirDano() { recibioDanoEnOleada = true; }

        private void AlEliminar(int puntosEnemigo, bool conHacha)
        {
            int ganados = puntosEnemigo + (conHacha ? bonoHacha : 0);
            puntosBaseOleada += puntosEnemigo;
            Sumar(ganados);
        }

        private void AlTerminarOleada(int n, float limpieza)
        {
            int bono = 0;
            if (!recibioDanoEnOleada) bono += puntosBaseOleada;                                   // R-07
            bono += Mathf.Max(0, Mathf.RoundToInt(segundosReferenciaLimpieza - limpieza)) * puntosPorSegundoAhorrado; // R-08
            Sumar(bono);
        }

        private void Sumar(int pts)
        {
            if (pts <= 0) return;
            Puntos += pts;
            residuoMonedas += pts;
            Monedas += residuoMonedas / puntosPorMoneda;
            residuoMonedas %= puntosPorMoneda;
            Notificar();
        }

        public bool Gastar(int monedas)
        {
            if (Monedas < monedas) return false;
            Monedas -= monedas;
            Notificar();
            return true;
        }

        private void Notificar() { GameEvents.RaisePuntajeCambio(Puntos, Monedas); }
    }
}
