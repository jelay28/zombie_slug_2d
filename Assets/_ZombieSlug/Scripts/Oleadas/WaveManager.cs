// -----------------------------------------------------------------------------
// WaveManager.cs - Subsistema de oleadas y apariciones.
// R-23: aparece un enemigo cada "intervalo" s en uno de los 3 puntos (S1, S2, S3).
// R-24: nunca hay más de 20 zombis vivos a la vez (presupuesto de rendimiento).
// R-10 (aviso): porGenerar == 0 y vivos == 0 -> evento OleadaTerminada, con el
// tiempo de limpieza t = segundos entre la última aparición y la última baja.
// Los enemigos salen de piscinas (una por tipo) y vuelven con SetActive(false).
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSlug
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private OleadaData[] oleadas;
        [SerializeField] private Transform[] puntosAparicion;
        [SerializeField] private Transform jugador;
        [SerializeField] private ObjectPool piscinaCaminantes;
        [SerializeField] private ObjectPool piscinaCorredores;
        [SerializeField] private ObjectPool piscinaTanques;
        [SerializeField] private ObjectPool piscinaJefe;
        [SerializeField] private ArenaManager arenas;
        [SerializeField] private int maximoSimultaneo = 20;

        public int Vivos { get; private set; }
        public int PorGenerar { get; private set; }

        private int oleadaEnCurso;
        private float tiempoUltimaAparicion;
        private bool generando;

        private void OnEnable() { GameEvents.EnemigoEliminado += AlEliminar; }
        private void OnDisable() { GameEvents.EnemigoEliminado -= AlEliminar; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Herramienta de prueba (solo editor o build de desarrollo): F8 elimina a
        // todos los zombis activos para recorrer las 10 oleadas en minutos y
        // verificar arenas, jefe y victoria. No existe en el build de entrega.
        private void Update()
        {
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (k == null || !k.f8Key.wasPressedThisFrame) return;
            foreach (EnemyBase e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
                if (e.Vivo) e.RecibirDano(9999f, false, Vector2.zero);
        }
#endif

        public void IniciarOleada(int numero)
        {
            oleadaEnCurso = numero;
            OleadaData datos = oleadas[Mathf.Clamp(numero - 1, 0, oleadas.Length - 1)];
            if (arenas != null) arenas.MostrarArena(datos.arena);
            GameEvents.RaiseOleadaIniciada(numero);
            StartCoroutine(Generar(datos));
        }

        private IEnumerator Generar(OleadaData d)
        {
            generando = true;
            List<ObjectPool> cola = ArmarCola(d);
            PorGenerar = cola.Count;
            for (int i = 0; i < cola.Count; i++)
            {
                while (Vivos >= maximoSimultaneo) yield return null;   // R-24
                Transform punto = puntosAparicion[Random.Range(0, puntosAparicion.Length)];
                GameObject go = cola[i].Obtener(punto.position);
                go.GetComponent<EnemyBase>().Inicializar(jugador, d.multiplicadorVida);
                Vivos++;
                PorGenerar--;
                tiempoUltimaAparicion = Time.time;
                if (PorGenerar > 0) yield return new WaitForSeconds(d.intervalo);
            }
            generando = false;
            Comprobar();
        }

        // Mezcla los tipos para que los corredores y tanques no salgan todos juntos.
        private List<ObjectPool> ArmarCola(OleadaData d)
        {
            var cola = new List<ObjectPool>();
            for (int i = 0; i < d.caminantes; i++) cola.Add(piscinaCaminantes);
            for (int i = 0; i < d.corredores; i++) cola.Add(piscinaCorredores);
            for (int i = 0; i < d.tanques; i++) cola.Add(piscinaTanques);
            for (int i = cola.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                ObjectPool t = cola[i]; cola[i] = cola[j]; cola[j] = t;
            }
            if (d.jefe) cola.Insert(Mathf.Min(2, cola.Count), piscinaJefe);
            return cola;
        }

        private void AlEliminar(EnemyBase e, bool hacha)
        {
            Vivos = Mathf.Max(0, Vivos - 1);
            Comprobar();
        }

        private void Comprobar()
        {
            if (generando || PorGenerar > 0 || Vivos > 0 || oleadaEnCurso == 0) return;
            float limpieza = Time.time - tiempoUltimaAparicion;
            int n = oleadaEnCurso;
            oleadaEnCurso = 0;
            GameEvents.RaiseOleadaTerminada(n, limpieza);
        }
    }
}
