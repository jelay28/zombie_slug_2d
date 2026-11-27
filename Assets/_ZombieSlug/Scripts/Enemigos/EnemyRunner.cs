// -----------------------------------------------------------------------------
// EnemyRunner.cs - Zombi corredor (ENE-02). Hereda de EnemyBase.
// R-21: si Ken está más de 1,5 m por encima y el corredor pisa suelo, salta
// (como máximo cada 1,5 s) para alcanzar las plataformas.
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public class EnemyRunner : EnemyBase
    {
        [SerializeField] private float alturaSalto = 3.2f;
        [SerializeField] private float diferenciaParaSaltar = 1.5f;
        [SerializeField] private float enfriamientoSalto = 1.5f;
        [SerializeField] private LayerMask capasSuelo;

        private float proximoSalto;

        protected override void Mover()
        {
            base.Mover();
            bool enSuelo = Physics2D.Raycast(transform.position + Vector3.up * 0.05f, Vector2.down, 0.15f, capasSuelo);
            bool kenArriba = objetivo.position.y - transform.position.y > diferenciaParaSaltar;
            if (enSuelo && kenArriba && Time.time >= proximoSalto)
            {
                float g = Mathf.Abs(Physics2D.gravity.y) * cuerpo.gravityScale;
                cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, Mathf.Sqrt(2f * g * alturaSalto));
                proximoSalto = Time.time + enfriamientoSalto;
            }
        }
    }
}
