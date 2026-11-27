// -----------------------------------------------------------------------------
// EnemyBase.cs - Comportamiento común de los zombis (va en el prefab base
// pfb_enemigo_base; caminante, corredor, tanque y jefe son variantes).
// El cuerpo (capa Enemigo) choca con suelo y plataformas; el hijo "Golpe"
// (disparador, capa EnemigoGolpe) solo toca al jugador.
// R-18: avanza horizontalmente hacia Ken. R-19: el contacto hace daño cada 1 s.
// R-20: vida <= 0 -> muere, avisa el evento y vuelve a la piscina (SetActive false).
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

namespace ZombieSlug
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBase : MonoBehaviour, IDanable
    {
        [Header("Variables de balance (las redefine cada variante)")]
        [SerializeField] protected float vidaBase = 3f;
        [SerializeField] protected float velocidad = 1.2f;
        [SerializeField] protected int danoContacto = 10;
        [SerializeField] protected int puntos = 10;
        [SerializeField] protected float intervaloGolpe = 1f;
        [SerializeField] [Range(0f, 1f)] protected float resistenciaEmpuje = 0f;

        [SerializeField] protected SpriteRenderer dibujo;

        public int Puntos { get { return puntos; } }
        public bool Vivo { get; private set; }

        protected Rigidbody2D cuerpo;
        protected Transform objetivo;
        protected float vida;
        private Coroutine rutinaContacto;
        private Color colorOriginal;

        protected virtual void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();
            if (dibujo == null) dibujo = GetComponentInChildren<SpriteRenderer>();
            colorOriginal = dibujo != null ? dibujo.color : Color.white;
        }

        // El reinicio va en OnEnable porque el objeto se reutiliza desde la piscina.
        protected virtual void OnEnable()
        {
            Vivo = true;
            vida = vidaBase;
            if (dibujo != null) dibujo.color = colorOriginal;
        }

        protected virtual void OnDisable()
        {
            if (rutinaContacto != null) { StopCoroutine(rutinaContacto); rutinaContacto = null; }
        }

        public virtual void Inicializar(Transform jugador, float multiplicadorVida)
        {
            objetivo = jugador;
            dirActual = Mathf.Sign(jugador.position.x - transform.position.x);
            vida = vidaBase * multiplicadorVida;
        }

        protected virtual void FixedUpdate()
        {
            if (!Vivo || objetivo == null) return;
            Mover();
        }

        protected float dirActual = 1f;

        // Iteración 3 (prueba de juego del 26/11): si Ken estaba justo debajo de una
        // plataforma, el zombi oscilaba encima de él y la oleada no terminaba.
        // Primer arreglo (conservar la dirección solo a menos de 1.5 m en x) falló:
        // el zombi rebotaba entre -1.5 y +1.5 sin llegar al borde (la plataforma
        // alta mide 6 m). Arreglo final: mientras Ken esté más de 1.5 m por debajo,
        // el zombi no corrige su dirección y termina cayendo por un borde.
        protected virtual void Mover()
        {
            Vector2 d = objetivo.position - transform.position;
            if (d.y >= -1.5f) dirActual = Mathf.Sign(d.x);
            float dir = dirActual;
            cuerpo.linearVelocity = new Vector2(dir * velocidad, cuerpo.linearVelocity.y);
            transform.localScale = new Vector3(dir, 1f, 1f);
        }

        // Llega desde el hijo disparador "Golpe" (en 2D el mensaje también se envía
        // al objeto que tiene el Rigidbody2D).
        protected virtual void OnTriggerEnter2D(Collider2D otro)
        {
            if (!otro.CompareTag("Player")) return;
            IDanable ph = otro.GetComponent<IDanable>();
            if (ph != null && rutinaContacto == null) rutinaContacto = StartCoroutine(Contacto(ph));
        }

        protected virtual void OnTriggerExit2D(Collider2D otro)
        {
            if (!otro.CompareTag("Player")) return;
            if (rutinaContacto != null) { StopCoroutine(rutinaContacto); rutinaContacto = null; }
        }

        private IEnumerator Contacto(IDanable ph)
        {
            while (Vivo)
            {
                ph.RecibirDano(danoContacto, false, Vector2.zero);
                yield return new WaitForSeconds(intervaloGolpe);
            }
            rutinaContacto = null;
        }

        public virtual void RecibirDano(float d, bool conHacha, Vector2 empuje)
        {
            if (!Vivo) return;
            vida -= d;
            if (empuje.sqrMagnitude > 0f)
                cuerpo.position += empuje * (1f - resistenciaEmpuje);
            if (dibujo != null) StartCoroutine(Destello());
            if (vida <= 0f) Morir(conHacha);
        }

        private IEnumerator Destello()
        {
            dibujo.color = Color.white * 2f;
            yield return new WaitForSeconds(0.08f);
            if (dibujo != null) dibujo.color = colorOriginal;
        }

        protected virtual void Morir(bool conHacha)
        {
            Vivo = false;
            GameEvents.RaiseEnemigoEliminado(puntos, conHacha);
            gameObject.SetActive(false);   // vuelve a la piscina; Destroy generaría basura
        }
    }
}
