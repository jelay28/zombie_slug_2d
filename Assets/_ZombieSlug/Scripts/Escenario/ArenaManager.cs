// -----------------------------------------------------------------------------
// ArenaManager.cs - Cambio de arena visual (calle, metro elevado, cisternas).
// Patrón "padre desactivado": cada arena es un objeto vacío con su fondo y sus
// plataformas como hijos. Una sola llamada a SetActive enciende o apaga todo
// el conjunto; las arenas apagadas no se dibujan ni colisionan.
// R-25: la arena cambia al empezar las oleadas 4 y 7 (dato en OleadaData.arena).
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public class ArenaManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] arenas;
        private int actual = -1;

        public void MostrarArena(int indice)
        {
            if (indice == actual || arenas == null || arenas.Length == 0) return;
            indice = Mathf.Clamp(indice, 0, arenas.Length - 1);
            for (int i = 0; i < arenas.Length; i++) arenas[i].SetActive(i == indice);
            actual = indice;
        }
    }
}
