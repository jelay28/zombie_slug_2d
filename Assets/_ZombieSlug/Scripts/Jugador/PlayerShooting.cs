// -----------------------------------------------------------------------------
// PlayerShooting.cs - Disparo en 3 direcciones y cambio de arma.
// R-04: el proyectil sale a 12 m/s; adelante por defecto, arriba con W y
// abajo con S solo en el aire. Las armas compradas tienen munición por oleada.
// Los proyectiles salen de una piscina de objetos (no se instancian ni se
// destruyen durante la partida).
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSlug
{
    public class PlayerShooting : MonoBehaviour
    {
        [SerializeField] private ArmaData armaInicial;
        [SerializeField] private Transform bocaDelArma;
        [SerializeField] private ObjectPool piscinaProyectiles;
        [SerializeField] private PlayerController control;

        private readonly List<ArmaData> armas = new List<ArmaData>();
        private readonly Dictionary<ArmaData, int> municion = new Dictionary<ArmaData, int>();
        private int indice;
        private float proximoDisparo;

        public ArmaData ArmaActual { get { return armas.Count > 0 ? armas[indice] : null; } }

        private void Awake()
        {
            if (armaInicial != null) AgregarArma(armaInicial);
        }

        private void OnEnable() { GameEvents.OleadaIniciada += RecargarTodo; }
        private void OnDisable() { GameEvents.OleadaIniciada -= RecargarTodo; }

        private void Start() { Notificar(); }

        private void Update()
        {
            if (InputLector.CambiarArma()) CambiarArma();
            if (!InputLector.Disparar() || Time.time < proximoDisparo || ArmaActual == null) return;

            ArmaData a = ArmaActual;
            int restante = municion[a];
            if (restante == 0) { CambiarArma(); return; }

            Disparar(a);
            proximoDisparo = Time.time + 1f / a.cadencia;
            if (restante > 0) { municion[a] = restante - 1; Notificar(); }
        }

        private void Disparar(ArmaData a)
        {
            Vector2 dir = new Vector2(control.Direccion, 0f);
            if (InputLector.Arriba()) dir = Vector2.up;
            else if (InputLector.Abajo() && !control.EnSuelo) dir = Vector2.down;

            float anguloBase = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            for (int i = 0; i < a.perdigones; i++)
            {
                float desv = a.perdigones > 1
                    ? Mathf.Lerp(-a.dispersion * 0.5f, a.dispersion * 0.5f, i / (float)(a.perdigones - 1))
                    : Random.Range(-a.dispersion * 0.5f, a.dispersion * 0.5f);
                float ang = (anguloBase + desv) * Mathf.Deg2Rad;
                Vector2 d = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                GameObject go = piscinaProyectiles.Obtener(bocaDelArma.position);
                if (go == null) return;
                go.GetComponent<Projectile>().Lanzar(d, a.velocidadProyectil, a.alcance, a.dano, a.empuje);
            }
        }

        public bool TieneArma(ArmaData a) { return armas.Contains(a); }

        public void AgregarArma(ArmaData a)
        {
            if (armas.Contains(a)) return;
            armas.Add(a);
            municion[a] = a.municionPorOleada;
            indice = armas.Count - 1;
            Notificar();
        }

        private void CambiarArma()
        {
            if (armas.Count < 2) return;
            for (int k = 1; k <= armas.Count; k++)
            {
                int j = (indice + k) % armas.Count;
                if (municion[armas[j]] != 0) { indice = j; break; }
            }
            Notificar();
        }

        private void RecargarTodo(int oleada)
        {
            foreach (ArmaData a in armas) municion[a] = a.municionPorOleada;
            Notificar();
        }

        private void Notificar()
        {
            if (ArmaActual != null) GameEvents.RaiseArmaCambio(ArmaActual.nombre, municion[ArmaActual]);
        }
    }
}
