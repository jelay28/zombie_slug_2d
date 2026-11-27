// -----------------------------------------------------------------------------
// PlayerHealth.cs - Salud, vidas extra e invulnerabilidad de Ken.
// R-16: el contacto resta salud y da 1 s de invulnerabilidad.
// R-17: salud <= 0 y vidasExtra > 0 -> vidasExtra - 1, salud = máxima, 2 s invulnerable.
// R-12 (aviso): salud <= 0 y vidasExtra == 0 -> evento JugadorSinVidas.
// El parpadeo de invulnerabilidad apaga solo el SpriteRenderer (enabled), no el
// objeto: Ken sigue moviéndose, colisionando y disparando mientras parpadea.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

namespace ZombieSlug
{
    public class PlayerHealth : MonoBehaviour, IDanable
    {
        [SerializeField] private int saludMaxima = 100;
        [SerializeField] private int vidasExtraIniciales = 1;
        [SerializeField] private float invulnerableTrasGolpe = 1f;
        [SerializeField] private float invulnerableTrasVida = 2f;
        [SerializeField] private SpriteRenderer dibujo;

        public int Salud { get; private set; }
        public int VidasExtra { get; private set; }
        public bool Vivo { get { return Salud > 0 || VidasExtra > 0; } }

        private bool invulnerable;
        private bool sinVidas;

        private void Start()
        {
            Salud = saludMaxima;
            VidasExtra = vidasExtraIniciales;
            GameEvents.RaiseSaludCambio(Salud, saludMaxima);
            GameEvents.RaiseVidasCambio(VidasExtra);
        }

        // Puerta de IDanable: los zombis no conocen a PlayerHealth, solo el contrato.
        public void RecibirDano(float cantidad, bool conHacha, Vector2 empuje)
        {
            RecibirDano(Mathf.RoundToInt(cantidad));
        }

        public void RecibirDano(int cantidad)
        {
            if (invulnerable || sinVidas || cantidad <= 0) return;
            Salud = Mathf.Max(0, Salud - cantidad);
            GameEvents.RaiseJugadorDanado();
            GameEvents.RaiseSaludCambio(Salud, saludMaxima);

            if (Salud > 0) { StartCoroutine(Invulnerable(invulnerableTrasGolpe)); return; }

            if (VidasExtra > 0)
            {
                VidasExtra--;
                Salud = saludMaxima;
                GameEvents.RaiseVidasCambio(VidasExtra);
                GameEvents.RaiseSaludCambio(Salud, saludMaxima);
                StartCoroutine(Invulnerable(invulnerableTrasVida));
            }
            else
            {
                sinVidas = true;
                Debug.Log("[ZS] Ken sin vidas: se emite JugadorSinVidas");
                GameEvents.RaiseJugadorSinVidas();
            }
        }

        private IEnumerator Invulnerable(float segundos)
        {
            invulnerable = true;
            float fin = Time.time + segundos;
            while (Time.time < fin)
            {
                if (dibujo != null) dibujo.enabled = !dibujo.enabled;
                yield return new WaitForSeconds(0.1f);
            }
            if (dibujo != null) dibujo.enabled = true;
            invulnerable = false;
        }

        // Mejoras del tablero
        public void AumentarSaludMaxima(float porcentaje)
        {
            saludMaxima = Mathf.RoundToInt(saludMaxima * (1f + porcentaje));
            Salud = saludMaxima;
            GameEvents.RaiseSaludCambio(Salud, saludMaxima);
        }

        public void AgregarVida()
        {
            VidasExtra++;
            GameEvents.RaiseVidasCambio(VidasExtra);
        }
    }
}
