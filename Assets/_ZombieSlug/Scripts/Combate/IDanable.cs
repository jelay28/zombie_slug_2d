// -----------------------------------------------------------------------------
// IDanable.cs - Contrato de "algo que recibe daño" (Ken y los zombis).
// Existe para que las dependencias vayan en una sola dirección: Jugador y
// Enemigos dependen de Combate, pero Combate no conoce a ninguno de los dos.
// Antes, MeleeAuto (Jugador) llamaba a EnemyBase y EnemyBase llamaba a
// PlayerHealth: un ciclo Jugador <-> Enemigos (corregido el 26/11).
// -----------------------------------------------------------------------------
using UnityEngine;

namespace ZombieSlug
{
    public interface IDanable
    {
        bool Vivo { get; }
        void RecibirDano(float cantidad, bool conHacha, Vector2 empuje);
    }
}
