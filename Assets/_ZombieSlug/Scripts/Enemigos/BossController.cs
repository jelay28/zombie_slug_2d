// -----------------------------------------------------------------------------
// BossController.cs - Jefe de la oleada 10 (ENE-04, variante del tanque).
// Responde al problema P2 del módulo 1 (jefe sin especificación):
// estados Caminar (3 s) -> Aviso (1 s, parpadea en rojo y se detiene)
// -> Embestida (1,2 s a 4 veces su velocidad) -> Caminar.
// R-22: la embestida solo empieza después del aviso (el jugador siempre la ve venir).
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public class BossController : EnemyBase
    {
        private enum Fase { Caminar, Aviso, Embestida }

        [SerializeField] private float segundosCaminar = 3f;
        [SerializeField] private float segundosAviso = 1f;
        [SerializeField] private float segundosEmbestida = 1.2f;
        [SerializeField] private float multiplicadorEmbestida = 4f;
        [SerializeField] private Color colorAviso = new Color(1f, 0.25f, 0.25f);

        private Fase fase;
        private float finFase;
        private float direccionEmbestida;

        protected override void OnEnable()
        {
            base.OnEnable();
            Entrar(Fase.Caminar, segundosCaminar);
        }

        private void Entrar(Fase f, float duracion)
        {
            fase = f;
            finFase = Time.time + duracion;
            if (dibujo != null) dibujo.color = (f == Fase.Aviso) ? colorAviso : Color.white;
        }

        protected override void Mover()
        {
            switch (fase)
            {
                case Fase.Caminar:
                    base.Mover();
                    if (Time.time >= finFase) Entrar(Fase.Aviso, segundosAviso);
                    break;
                case Fase.Aviso:
                    cuerpo.linearVelocity = new Vector2(0f, cuerpo.linearVelocity.y);
                    if (Time.time >= finFase)
                    {
                        direccionEmbestida = Mathf.Sign(objetivo.position.x - transform.position.x);
                        Entrar(Fase.Embestida, segundosEmbestida);
                    }
                    break;
                case Fase.Embestida:
                    cuerpo.linearVelocity = new Vector2(direccionEmbestida * velocidad * multiplicadorEmbestida, cuerpo.linearVelocity.y);
                    if (Time.time >= finFase) Entrar(Fase.Caminar, segundosCaminar);
                    break;
            }
        }
    }
}
