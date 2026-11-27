// -----------------------------------------------------------------------------
// ResultadoController.cs - Escena Resultado: muestra el resultado guardado en
// GameSession (persistente) y, si el puntaje entra al top 5, pide 3 iniciales.
// -----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZombieSlug
{
    public class ResultadoController : MonoBehaviour
    {
        [SerializeField] private Text textoResultado;
        [SerializeField] private InputField campoIniciales;
        [SerializeField] private Button botonGuardar;

        private bool guardado;

        private void Start()
        {
            GameSession s = GameSession.Instancia;
            if (s == null) { textoResultado.text = "Sin datos de partida"; return; }
            int min = Mathf.FloorToInt(s.UltimoTiempo / 60f);
            int seg = Mathf.FloorToInt(s.UltimoTiempo % 60f);
            textoResultado.text = (s.UltimaGano ? "¡VICTORIA!" : "DERROTA") +
                "\nPuntaje: " + s.UltimoPuntaje +
                "\nOleada alcanzada: " + s.UltimaOleada +
                "\nTiempo: " + min + ":" + seg.ToString("00");
            bool entra = s.EntraEnRanking(s.UltimoPuntaje);
            campoIniciales.gameObject.SetActive(entra);
            botonGuardar.gameObject.SetActive(entra);
        }

        public void Guardar()
        {
            if (guardado || GameSession.Instancia == null) return;
            string ini = string.IsNullOrEmpty(campoIniciales.text) ? "AAA" : campoIniciales.text;
            if (ini.Length > 3) ini = ini.Substring(0, 3);
            GameSession.Instancia.RegistrarEnRanking(ini, GameSession.Instancia.UltimoPuntaje);
            guardado = true;
            botonGuardar.interactable = false;
        }

        public void VolverAlMenu() { SceneManager.LoadScene("Menu"); }
    }
}
