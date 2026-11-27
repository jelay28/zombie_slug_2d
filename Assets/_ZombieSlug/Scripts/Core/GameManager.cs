// -----------------------------------------------------------------------------
// GameManager.cs  - Subsistema de flujo de partida (máquina de estados).
// Estados: Preparacion -> Oleada -> Tienda -> Oleada ... -> Victoria | Derrota,
// con Pausa como estado lateral que vuelve al estado anterior.
// Reglas implementadas: R-10 (fin de oleada abre tienda), R-11 (victoria),
// R-12 (derrota), R-13 (prioridad: victoria prevalece sobre derrota),
// R-14 (pausa). Es el único script que cambia el estado del juego.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieSlug
{
    [DefaultExecutionOrder(-100)]   // se inicializa antes que cualquier script que le reporte
    public class GameManager : MonoBehaviour
    {
        [Header("Referencias (arrastradas en el inspector)")]
        [SerializeField] private WaveManager oleadas;
        [SerializeField] private RewardBoard tablero;
        [SerializeField] private PlayerController controlJugador;
        [SerializeField] private PlayerShooting disparoJugador;
        [SerializeField] private ScoreManager puntaje;

        [Header("Variables de balance")]
        [SerializeField] private float segundosPreparacion = 3f;
        [SerializeField] private float segundosTiendaMax = 20f;
        [SerializeField] private int oleadaFinal = 10;
        [SerializeField] private string escenaResultado = "Resultado";

        public static GameManager Instancia { get; private set; }
        public EstadoJuego Estado { get; private set; }
        public int OleadaActual { get; private set; }

        private EstadoJuego estadoAntesDePausa;
        private float tiempoPartida;          // segundos jugados (sin contar pausa)
        private bool victoriaAlcanzada;
        private Coroutine rutinaTienda;

        private void Awake()
        {
            Instancia = this;
        }

        private void OnEnable()
        {
            GameEvents.OleadaTerminada += AlTerminarOleada;
            GameEvents.JugadorSinVidas += AlQuedarSinVidas;
        }

        private void OnDisable()
        {
            GameEvents.OleadaTerminada -= AlTerminarOleada;
            GameEvents.JugadorSinVidas -= AlQuedarSinVidas;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            OleadaActual = 0;
            CambiarEstado(EstadoJuego.Preparacion);
            StartCoroutine(Preparar());
        }

        private void Update()
        {
            // La entrada sí se lee por cuadro: no existe un evento del motor para "tecla pulsada".
            if (InputLector.Pausa()) AlternarPausa();
            if (Estado == EstadoJuego.Tienda && InputLector.Continuar()) TerminarTienda();
            if (Estado == EstadoJuego.Oleada || Estado == EstadoJuego.Tienda) tiempoPartida += Time.deltaTime;
        }

        // ---- Transiciones (cada una con una sola condición) ----------------------

        private IEnumerator Preparar()
        {
            yield return new WaitForSeconds(segundosPreparacion);
            IniciarSiguienteOleada();
        }

        private void IniciarSiguienteOleada()
        {
            OleadaActual++;
            if (tablero != null) tablero.Habilitar(false);
            CambiarEstado(EstadoJuego.Oleada);
            oleadas.IniciarOleada(OleadaActual);
        }

        private void AlTerminarOleada(int numero, float limpieza)
        {
            // R-11: Oleada -> Victoria si se completó la oleada final.
            if (numero >= oleadaFinal)
            {
                victoriaAlcanzada = true;
                Finalizar(true);
                return;
            }
            // R-10: Oleada -> Tienda.
            CambiarEstado(EstadoJuego.Tienda);
            if (tablero != null) tablero.Habilitar(true);
            rutinaTienda = StartCoroutine(TiempoDeTienda());
        }

        private IEnumerator TiempoDeTienda()
        {
            yield return new WaitForSeconds(segundosTiendaMax);
            TerminarTienda();
        }

        public void TerminarTienda()
        {
            if (Estado != EstadoJuego.Tienda) return;
            if (rutinaTienda != null) StopCoroutine(rutinaTienda);
            IniciarSiguienteOleada();
        }

        private void AlQuedarSinVidas()
        {
            Debug.Log("[ZS] GameManager recibió JugadorSinVidas (estado " + Estado + ")");
            // R-13: si en el mismo cuadro se completa la oleada 10, gana la victoria.
            StartCoroutine(DecidirDerrotaAlFinalDelCuadro());
        }

        private IEnumerator DecidirDerrotaAlFinalDelCuadro()
        {
            // Iteración 5 (prueba del 26/11): con WaitForEndOfFrame la corrutina no
            // se reanudaba en el editor y Ken quedaba en 0/100 sin panel de derrota.
            // Esperar al cuadro siguiente basta: la victoria se marca en el mismo
            // cuadro en que muere el último zombi, antes de esta comprobación.
            yield return null;
            if (victoriaAlcanzada) yield break;
            Finalizar(false);   // R-12: Oleada -> Derrota
        }

        private void Finalizar(bool gano)
        {
            CambiarEstado(gano ? EstadoJuego.Victoria : EstadoJuego.Derrota);
            controlJugador.enabled = false;
            disparoJugador.enabled = false;
            if (GameSession.Instancia != null)
                GameSession.Instancia.GuardarResultado(gano, puntaje.Puntos, OleadaActual, tiempoPartida);
        }

        // R-14: la pausa deshabilita los scripts de control (enabled = false) en
        // lugar de desactivar el objeto del jugador, para no disparar OnEnable al volver.
        public void AlternarPausa()
        {
            if (Estado == EstadoJuego.Pausa)
            {
                Time.timeScale = 1f;
                controlJugador.enabled = true;
                disparoJugador.enabled = true;
                CambiarEstado(estadoAntesDePausa);
            }
            else if (Estado == EstadoJuego.Oleada || Estado == EstadoJuego.Tienda)
            {
                estadoAntesDePausa = Estado;
                Time.timeScale = 0f;
                controlJugador.enabled = false;
                disparoJugador.enabled = false;
                CambiarEstado(EstadoJuego.Pausa);
            }
        }

        private void CambiarEstado(EstadoJuego nuevo)
        {
            Estado = nuevo;
            GameEvents.RaiseEstadoCambio(nuevo);
        }

        // ---- Botones de los paneles ----------------------------------------------
        public void IrAResultado() { Time.timeScale = 1f; SceneManager.LoadScene(escenaResultado); }
        public void Reintentar() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        public void IrAMenu() { Time.timeScale = 1f; SceneManager.LoadScene("Menu"); }
    }
}
