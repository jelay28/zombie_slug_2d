// -----------------------------------------------------------------------------
// PlayerController.cs - Controlador FÍSICO de Ken (Rigidbody2D dinámico).
// Se eligió física y no un controlador cinemático porque las plataformas usan
// PlatformEffector2D (atravesar desde abajo) y la gravedad del motor.
// Orden: la entrada se lee en Update (no se pierden pulsaciones) y la velocidad
// se aplica en FixedUpdate (paso fijo de 0,02 s), así el desplazamiento no
// depende de los cuadros por segundo.
// Reglas: R-01 (moverse a 5 m/s), R-02 (salto de 3,5 m, sin doble salto),
// R-03 (bajar de plataforma con S + espacio).
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

namespace ZombieSlug
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Variables de balance")]
        [SerializeField] private float velocidad = 5f;          // m/s
        [SerializeField] private float alturaSalto = 3.5f;      // m
        [SerializeField] private float segundosAtravesar = 0.3f;

        [Header("Detección de suelo")]
        [SerializeField] private Transform pies;
        [SerializeField] private Vector2 tamanoPies = new Vector2(0.6f, 0.1f);
        [SerializeField] private LayerMask capasSuelo;
        [SerializeField] private LayerMask capaPlataforma;

        [SerializeField] private Animator animador;

        public bool EnSuelo { get; private set; }
        public int Direccion { get; private set; }   // 1 derecha, -1 izquierda

        private Rigidbody2D cuerpo;
        private Collider2D colision;
        private float entradaH;
        private bool saltoPendiente;

        private void Awake()
        {
            cuerpo = GetComponent<Rigidbody2D>();       // referencias cacheadas una sola vez
            colision = GetComponent<Collider2D>();
            Direccion = 1;
        }

        private void OnDisable()
        {
            entradaH = 0f;
            saltoPendiente = false;
            if (cuerpo != null) cuerpo.linearVelocity = new Vector2(0f, cuerpo.linearVelocity.y);
        }

        private void Update()
        {
            entradaH = InputLector.Horizontal();
            if (InputLector.Salto() && EnSuelo)
            {
                if (InputLector.Abajo()) StartCoroutine(BajarDePlataforma());   // R-03
                else saltoPendiente = true;                                       // R-02
            }
            if (entradaH > 0.01f) Direccion = 1;
            else if (entradaH < -0.01f) Direccion = -1;
            transform.localScale = new Vector3(Direccion, 1f, 1f);

            if (animador != null)
            {
                animador.SetFloat("Velocidad", Mathf.Abs(cuerpo.linearVelocity.x));
                animador.SetBool("EnSuelo", EnSuelo);
            }
        }

        private void FixedUpdate()
        {
            EnSuelo = Physics2D.OverlapBox(pies.position, tamanoPies, 0f, capasSuelo) != null;
            float vy = cuerpo.linearVelocity.y;
            if (saltoPendiente)
            {
                // v0 = raíz(2 g h): con g = 9,81 × 3 = 29,43 m/s² y h = 3,5 m, v0 ≈ 14,35 m/s
                float g = Mathf.Abs(Physics2D.gravity.y) * cuerpo.gravityScale;
                vy = Mathf.Sqrt(2f * g * alturaSalto);
                saltoPendiente = false;
            }
            cuerpo.linearVelocity = new Vector2(entradaH * velocidad, vy);   // R-01
        }

        private IEnumerator BajarDePlataforma()
        {
            Collider2D plataforma = Physics2D.OverlapBox(pies.position, tamanoPies, 0f, capaPlataforma);
            if (plataforma == null) yield break;
            Physics2D.IgnoreCollision(colision, plataforma, true);
            yield return new WaitForSeconds(segundosAtravesar);
            Physics2D.IgnoreCollision(colision, plataforma, false);
        }

        private void OnDrawGizmosSelected()
        {
            if (pies == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(pies.position, tamanoPies);
        }
    }
}
