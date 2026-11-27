// -----------------------------------------------------------------------------
// ShopController.cs - Lógica de compra del tablero (R-09).
// Mejoras: MEJ-01 salud +25 % (30, máx. 2), MEJ-02 vida extra (40, máx. 2),
// MEJ-03 escopeta (50), MEJ-04 ametralladora (80).
// La interfaz solo llama a estos métodos; no decide precios ni reglas.
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public class ShopController : MonoBehaviour
    {
        [SerializeField] private ScoreManager puntaje;
        [SerializeField] private PlayerHealth salud;
        [SerializeField] private PlayerShooting disparo;
        [SerializeField] private ArmaData escopeta;
        [SerializeField] private ArmaData ametralladora;

        [Header("Precios y límites")]
        [SerializeField] private int costoSalud = 30;
        [SerializeField] private int costoVida = 40;
        [SerializeField] private int maximoPorMejora = 2;

        private int comprasSalud;
        private int comprasVida;

        public string ComprarSalud()
        {
            if (comprasSalud >= maximoPorMejora) return "Límite alcanzado";
            if (!puntaje.Gastar(costoSalud)) return "Monedas insuficientes";
            comprasSalud++;
            salud.AumentarSaludMaxima(0.25f);
            return "Salud +25 %";
        }

        public string ComprarVida()
        {
            if (comprasVida >= maximoPorMejora) return "Límite alcanzado";
            if (!puntaje.Gastar(costoVida)) return "Monedas insuficientes";
            comprasVida++;
            salud.AgregarVida();
            return "Vida extra";
        }

        public string ComprarEscopeta() { return ComprarArma(escopeta); }
        public string ComprarAmetralladora() { return ComprarArma(ametralladora); }

        private string ComprarArma(ArmaData a)
        {
            if (disparo.TieneArma(a)) return "Ya la tienes";
            if (!puntaje.Gastar(a.costo)) return "Monedas insuficientes";
            disparo.AgregarArma(a);
            return a.nombre + " comprada";
        }
    }
}
