// -----------------------------------------------------------------------------
// ArmaData.cs - ScriptableObject con las variables de balance de un arma.
// Vivir en un asset permite ajustar un valor por iteración sin recompilar
// (regla de "una variable por iteración" del módulo 1).
// IDs del GDD: ARM-01 pistola, ARM-02 escopeta, ARM-03 ametralladora.
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    [CreateAssetMenu(fileName = "arm_nueva", menuName = "Zombie Slug/Arma")]
    public class ArmaData : ScriptableObject
    {
        public string id = "ARM-01";
        public string nombre = "Pistola";
        [Tooltip("Daño por proyectil o perdigón")] public float dano = 1f;
        [Tooltip("Disparos por segundo")] public float cadencia = 3f;
        [Tooltip("Proyectiles por disparo (escopeta = 5)")] public int perdigones = 1;
        [Tooltip("Abertura total del cono en grados")] public float dispersion = 0f;
        [Tooltip("Metros por segundo")] public float velocidadProyectil = 12f;
        [Tooltip("Metros que recorre antes de desactivarse")] public float alcance = 20f;
        [Tooltip("-1 = infinita; se recarga al iniciar cada oleada")] public int municionPorOleada = -1;
        [Tooltip("Metros de empuje aplicados al enemigo")] public float empuje = 0f;
        [Tooltip("Costo en monedas en el tablero")] public int costo = 0;
    }
}
