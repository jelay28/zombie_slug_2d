// -----------------------------------------------------------------------------
// Projectile.cs - Proyectil reutilizable (pfb_proyectil, sale de la piscina).
// Se desactiva al tocar un enemigo o el suelo, o al recorrer su alcance
// (tiempo de vida = alcance / velocidad; pistola: 20 m / 12 m/s = 1,67 s).
// El estado se reinicia en Lanzar/OnEnable para no arrastrar el uso anterior.
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody2D cuerpo;
        private float dano;
        private Vector2 empuje;

        private void Awake() { cuerpo = GetComponent<Rigidbody2D>(); }

        private void OnDisable() { CancelInvoke(); }

        public void Lanzar(Vector2 direccion, float velocidad, float alcance, float danoProyectil, float metrosEmpuje)
        {
            dano = danoProyectil;
            empuje = direccion.normalized * metrosEmpuje;
            transform.right = direccion;
            cuerpo.linearVelocity = direccion.normalized * velocidad;
            Invoke("Desactivar", alcance / velocidad);
        }

        private void OnTriggerEnter2D(Collider2D otro)
        {
            IDanable e = otro.GetComponentInParent<IDanable>();
            if (e != null && e.Vivo) { e.RecibirDano(dano, false, empuje); Desactivar(); return; }
            if (otro.gameObject.layer == LayerMask.NameToLayer("Suelo")) Desactivar();
        }

        private void Desactivar() { gameObject.SetActive(false); }
    }
}
