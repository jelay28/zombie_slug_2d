// -----------------------------------------------------------------------------
// RewardBoard.cs - Tablero de recompensas (PNJ funcional: la máquina de Don Kuro).
// Patrón "llave y cofre": el script y su disparador arrancan DESHABILITADOS
// (enabled = false) y el GameManager los habilita solo en el estado Tienda.
// Ojo: Unity sigue enviando OnTriggerEnter2D a un script deshabilitado, por eso
// también se deshabilita el Collider2D; el objeto queda activo para que el
// tablero se siga viendo (desactivarlo lo haría desaparecer de la arena).
// R-26: E cerca del tablero (y en estado Tienda) abre el panel de compra.
// Desde el 26/11 avisa por eventos (AyudaTablero, TiendaVisible): la Tienda ya
// no depende de la Interfaz; solo la Interfaz depende de la Tienda.
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public class RewardBoard : MonoBehaviour
    {
        [SerializeField] private Collider2D zona;
        [SerializeField] private SpriteRenderer luz;        // indicador "abierto"

        private bool jugadorCerca;

        private void Awake()
        {
            if (zona == null) zona = GetComponent<Collider2D>();
        }

        public void Habilitar(bool si)
        {
            enabled = si;
            zona.enabled = si;
            if (luz != null) luz.enabled = si;
            if (!si) { jugadorCerca = false; GameEvents.RaiseAyudaTablero(false); }
        }

        private void OnTriggerEnter2D(Collider2D otro)
        {
            if (!enabled || !otro.CompareTag("Player")) return;
            jugadorCerca = true;
            GameEvents.RaiseAyudaTablero(true);
        }

        private void OnTriggerExit2D(Collider2D otro)
        {
            if (!otro.CompareTag("Player")) return;
            jugadorCerca = false;
            GameEvents.RaiseAyudaTablero(false);
            GameEvents.RaiseTiendaVisible(false);
        }

        private void Update()
        {
            if (jugadorCerca && InputLector.Interactuar()) GameEvents.RaiseTiendaVisible(true);
        }
    }
}
