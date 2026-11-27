// -----------------------------------------------------------------------------
// HUDController.cs - Muestra salud, vidas, puntos, monedas, oleada y arma.
// No tiene Update: cada texto se actualiza solo cuando llega el evento que lo
// cambia. Con 6 textos a 60 cuadros/s, consultar por cuadro serían 360 lecturas
// por segundo; con eventos son unas pocas por oleada (ver cálculo en el informe).
// -----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace ZombieSlug
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Image barraSalud;     // Image tipo Filled
        [SerializeField] private Text textoSalud;
        [SerializeField] private Text textoVidas;
        [SerializeField] private Text textoPuntos;
        [SerializeField] private Text textoMonedas;
        [SerializeField] private Text textoOleada;
        [SerializeField] private Text textoArma;

        private void OnEnable()
        {
            GameEvents.SaludCambio += AlCambiarSalud;
            GameEvents.VidasCambio += AlCambiarVidas;
            GameEvents.PuntajeCambio += AlCambiarPuntaje;
            GameEvents.OleadaIniciada += AlIniciarOleada;
            GameEvents.ArmaCambio += AlCambiarArma;
        }

        private void OnDisable()
        {
            GameEvents.SaludCambio -= AlCambiarSalud;
            GameEvents.VidasCambio -= AlCambiarVidas;
            GameEvents.PuntajeCambio -= AlCambiarPuntaje;
            GameEvents.OleadaIniciada -= AlIniciarOleada;
            GameEvents.ArmaCambio -= AlCambiarArma;
        }

        private void AlCambiarSalud(int actual, int max)
        {
            barraSalud.fillAmount = max > 0 ? (float)actual / max : 0f;
            textoSalud.text = actual + " / " + max;
        }

        private void AlCambiarVidas(int vidas) { textoVidas.text = "Vidas extra: " + vidas; }
        private void AlCambiarPuntaje(int puntos, int monedas) { textoPuntos.text = "Puntos: " + puntos; textoMonedas.text = "Monedas: " + monedas; }
        private void AlIniciarOleada(int n) { textoOleada.text = "Oleada " + n + " / 10"; }
        private void AlCambiarArma(string nombre, int municion) { textoArma.text = nombre + (municion < 0 ? "  ∞" : "  " + municion); }
    }
}
