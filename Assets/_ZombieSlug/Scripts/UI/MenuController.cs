// -----------------------------------------------------------------------------
// MenuController.cs - Escena Menu: jugar, salir y tabla local de 5 mejores.
// -----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZombieSlug
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private Text textoRanking;
        [SerializeField] private string escenaJuego = "Juego";

        private void Start()
        {
            if (textoRanking == null || GameSession.Instancia == null) return;
            var r = GameSession.Instancia.LeerRanking();
            string s = "MEJORES PUNTAJES\n";
            if (r.Count == 0) s += "(aún no hay partidas)";
            for (int i = 0; i < r.Count; i++) s += (i + 1) + ". " + r[i].Key + "   " + r[i].Value + "\n";
            textoRanking.text = s;
        }

        public void Jugar() { SceneManager.LoadScene(escenaJuego); }
        public void Salir() { Application.Quit(); }
    }
}
