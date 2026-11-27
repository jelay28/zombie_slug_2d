// -----------------------------------------------------------------------------
// MeleeAuto.cs - Hacha automática de Ken (va en el hijo "ZonaHacha", con un
// CircleCollider2D disparador de radio 1 m en la capa Hacha).
// R-15: si hay un enemigo a 1 m o menos y pasaron 0,6 s, le hace 4 de daño.
// Se usan los eventos de disparador (entrar/salir) y una corrutina que solo
// corre mientras haya enemigos en rango: no hay vigilancia por cuadro.
// -----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSlug
{
    public class MeleeAuto : MonoBehaviour
    {
        [SerializeField] private float dano = 4f;
        [SerializeField] private float enfriamiento = 0.6f;
        // Iteración 4 (prueba del 26/11): sin este retardo el hacha mataba al
        // caminante antes de que su golpe tocara a Ken, y quedarse quieto era
        // gratis. Un caminante (1.2 m/s) tarda 0.48 s en ir del borde del
        // círculo (1.375 m entre centros) al contacto (0.8 m): con 0.5 s de
        // levantar el hacha, Ken recibe un golpe antes del primer tajo.
        [SerializeField] private float retardoPrimerGolpe = 0.5f;

        private readonly List<EnemyBase> enRango = new List<EnemyBase>();
        private Coroutine rutina;

        private void OnTriggerEnter2D(Collider2D otro)
        {
            EnemyBase e = otro.GetComponentInParent<EnemyBase>();
            if (e == null || enRango.Contains(e)) return;
            enRango.Add(e);
            if (rutina == null) rutina = StartCoroutine(Golpear());
        }

        private void OnTriggerExit2D(Collider2D otro)
        {
            EnemyBase e = otro.GetComponentInParent<EnemyBase>();
            if (e != null) enRango.Remove(e);
        }

        private void OnDisable()
        {
            enRango.Clear();
            rutina = null;
        }

        private IEnumerator Golpear()
        {
            yield return new WaitForSeconds(retardoPrimerGolpe);
            while (enRango.Count > 0)
            {
                enRango.RemoveAll(x => x == null || !x.gameObject.activeInHierarchy || !x.Vivo);
                if (enRango.Count == 0) break;
                enRango[0].RecibirDano(dano, true, Vector2.zero);
                yield return new WaitForSeconds(enfriamiento);
            }
            rutina = null;
        }
    }
}
