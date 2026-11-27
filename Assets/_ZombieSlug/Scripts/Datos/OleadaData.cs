// -----------------------------------------------------------------------------
// OleadaData.cs - ScriptableObject con la composición de una oleada.
// Los valores salen de la tabla de calibración (módulo 1, tabla 8; GDD 1.4).
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    [CreateAssetMenu(fileName = "ola_00", menuName = "Zombie Slug/Oleada")]
    public class OleadaData : ScriptableObject
    {
        public int numero = 1;
        public int caminantes = 6;
        public int corredores = 0;
        public int tanques = 0;
        public bool jefe = false;
        [Tooltip("Segundos entre apariciones: 2,4 - 0,13 (n - 1), mínimo 1,2")] public float intervalo = 2.4f;
        [Tooltip("Multiplicador de vida: 1 + 0,05 (n - 1)")] public float multiplicadorVida = 1f;
        [Tooltip("Arena visual: 0 = calle, 1 = metro elevado, 2 = patio de cisternas")] public int arena = 0;

        public int Total { get { return caminantes + corredores + tanques + (jefe ? 1 : 0); } }
    }
}
