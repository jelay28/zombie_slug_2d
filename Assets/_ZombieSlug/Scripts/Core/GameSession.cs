// -----------------------------------------------------------------------------
// GameSession.cs - Persistencia entre escenas (Menu -> Juego -> Resultado).
// Objeto marcado DontDestroyOnLoad con protección contra duplicados.
// Guarda el resultado de la partida y una tabla local de 5 mejores puntajes
// (PlayerPrefs) para que varios jugadores compitan en el mismo computador.
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSlug
{
    [DefaultExecutionOrder(-200)]
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instancia { get; private set; }

        public bool UltimaGano { get; private set; }
        public int UltimoPuntaje { get; private set; }
        public int UltimaOleada { get; private set; }
        public float UltimoTiempo { get; private set; }

        private const string Clave = "zs_ranking";
        private const int Puestos = 5;

        private void Awake()
        {
            if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }

        public void GuardarResultado(bool gano, int puntaje, int oleada, float segundos)
        {
            UltimaGano = gano; UltimoPuntaje = puntaje; UltimaOleada = oleada; UltimoTiempo = segundos;
        }

        // Formato almacenado: "INI|puntaje;INI|puntaje;..."
        public List<KeyValuePair<string, int>> LeerRanking()
        {
            var lista = new List<KeyValuePair<string, int>>();
            string raw = PlayerPrefs.GetString(Clave, "");
            if (string.IsNullOrEmpty(raw)) return lista;
            foreach (string fila in raw.Split(';'))
            {
                string[] p = fila.Split('|');
                int pts;
                if (p.Length == 2 && int.TryParse(p[1], out pts)) lista.Add(new KeyValuePair<string, int>(p[0], pts));
            }
            return lista;
        }

        public bool EntraEnRanking(int puntaje)
        {
            var r = LeerRanking();
            return r.Count < Puestos || puntaje > r[r.Count - 1].Value;
        }

        public void RegistrarEnRanking(string iniciales, int puntaje)
        {
            var r = LeerRanking();
            r.Add(new KeyValuePair<string, int>(iniciales.ToUpper(), puntaje));
            r.Sort((a, b) => b.Value.CompareTo(a.Value));
            if (r.Count > Puestos) r.RemoveRange(Puestos, r.Count - Puestos);
            var partes = new List<string>();
            foreach (var kv in r) partes.Add(kv.Key + "|" + kv.Value);
            PlayerPrefs.SetString(Clave, string.Join(";", partes.ToArray()));
            PlayerPrefs.Save();
        }
    }
}
