// -----------------------------------------------------------------------------
// UIManager.cs - Gestión de la interfaz (patrón "un solo punto de decisión").
// Los paneles HUD, Tienda, Pausa, Victoria y Derrota son hijos de "Lienzo" y
// este script es el único que los activa o desactiva con SetActive, según el
// estado que anuncia el GameManager. Así nunca quedan dos paneles encendidos.
// La interfaz no calcula reglas: solo muestra y reenvía clics.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieSlug
{
    public class UIManager : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelHUD;
        [SerializeField] private GameObject panelTienda;
        [SerializeField] private GameObject panelPausa;
        [SerializeField] private GameObject panelVictoria;
        [SerializeField] private GameObject panelDerrota;
        [SerializeField] private GameObject ayudaTablero;

        [Header("Textos")]
        [SerializeField] private Text textoAviso;
        [SerializeField] private Text textoMensajeTienda;

        [SerializeField] private ShopController tienda;

        private void OnEnable()
        {
            GameEvents.EstadoCambio += AlCambiarEstado;
            GameEvents.OleadaIniciada += AlIniciarOleada;
        }

        private void OnDisable()
        {
            GameEvents.EstadoCambio -= AlCambiarEstado;
            GameEvents.OleadaIniciada -= AlIniciarOleada;
        }

        private void AlCambiarEstado(EstadoJuego e)
        {
            panelHUD.SetActive(e != EstadoJuego.Victoria && e != EstadoJuego.Derrota);
            panelPausa.SetActive(e == EstadoJuego.Pausa);
            panelVictoria.SetActive(e == EstadoJuego.Victoria);
            panelDerrota.SetActive(e == EstadoJuego.Derrota);
            if (e != EstadoJuego.Tienda && e != EstadoJuego.Pausa) CerrarTienda();
            if (e == EstadoJuego.Tienda) Avisar("TIENDA ABIERTA", 3f);
            if (e == EstadoJuego.Preparacion) Avisar("PREPARATE", 2.5f);
        }

        private void AlIniciarOleada(int n) { Avisar("OLEADA " + n, 2f); }

        public void Avisar(string texto, float segundos)
        {
            StopAllCoroutines();
            StartCoroutine(MostrarAviso(texto, segundos));
        }

        private IEnumerator MostrarAviso(string texto, float segundos)
        {
            textoAviso.text = texto;
            textoAviso.gameObject.SetActive(true);
            yield return new WaitForSeconds(segundos);
            textoAviso.gameObject.SetActive(false);
        }

        public void MostrarAyudaTablero(bool si) { ayudaTablero.SetActive(si); }

        public void AbrirTienda() { panelTienda.SetActive(true); textoMensajeTienda.text = ""; }
        public void CerrarTienda() { if (panelTienda != null) panelTienda.SetActive(false); }

        // Botones del panel Tienda (persistentes, asignados por el constructor del proyecto)
        public void BotonSalud() { textoMensajeTienda.text = tienda.ComprarSalud(); }
        public void BotonVida() { textoMensajeTienda.text = tienda.ComprarVida(); }
        public void BotonEscopeta() { textoMensajeTienda.text = tienda.ComprarEscopeta(); }
        public void BotonAmetralladora() { textoMensajeTienda.text = tienda.ComprarAmetralladora(); }
        public void BotonContinuar() { if (GameManager.Instancia != null) GameManager.Instancia.TerminarTienda(); }
        public void BotonReanudar() { if (GameManager.Instancia != null) GameManager.Instancia.AlternarPausa(); }
        public void BotonReintentar() { if (GameManager.Instancia != null) GameManager.Instancia.Reintentar(); }
        public void BotonMenu() { if (GameManager.Instancia != null) GameManager.Instancia.IrAMenu(); }
        public void BotonResultado() { if (GameManager.Instancia != null) GameManager.Instancia.IrAResultado(); }
    }
}
