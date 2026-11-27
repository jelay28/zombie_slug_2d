// -----------------------------------------------------------------------------
// InputLector.cs
// Punto único de lectura del teclado y el ratón. Si el proyecto usa el paquete
// Input System (plantilla Universal 2D de Unity 6.3), lee Keyboard.current; si
// no, cae al Input Manager clásico. Así ningún otro script depende del sistema
// de entrada elegido.
// -----------------------------------------------------------------------------
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ZombieSlug
{
    public static class InputLector
    {
#if ENABLE_INPUT_SYSTEM
        static Keyboard K { get { return Keyboard.current; } }
        static Mouse M { get { return Mouse.current; } }

        public static float Horizontal()
        {
            if (K == null) return 0f;
            float h = 0f;
            if (K.aKey.isPressed || K.leftArrowKey.isPressed) h -= 1f;
            if (K.dKey.isPressed || K.rightArrowKey.isPressed) h += 1f;
            return h;
        }
        public static bool Salto() { return K != null && K.spaceKey.wasPressedThisFrame; }
        public static bool Arriba() { return K != null && (K.wKey.isPressed || K.upArrowKey.isPressed); }
        public static bool Abajo() { return K != null && (K.sKey.isPressed || K.downArrowKey.isPressed); }
        public static bool Disparar() { return (K != null && K.jKey.isPressed) || (M != null && M.leftButton.isPressed); }
        public static bool CambiarArma() { return K != null && K.qKey.wasPressedThisFrame; }
        public static bool Interactuar() { return K != null && K.eKey.wasPressedThisFrame; }
        public static bool Pausa() { return K != null && (K.escapeKey.wasPressedThisFrame || K.pKey.wasPressedThisFrame); }
        public static bool Continuar() { return K != null && (K.enterKey.wasPressedThisFrame || K.nKey.wasPressedThisFrame); }
#else
        public static float Horizontal()
        {
            float h = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            return h;
        }
        public static bool Salto() { return Input.GetKeyDown(KeyCode.Space); }
        public static bool Arriba() { return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow); }
        public static bool Abajo() { return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow); }
        public static bool Disparar() { return Input.GetKey(KeyCode.J) || Input.GetMouseButton(0); }
        public static bool CambiarArma() { return Input.GetKeyDown(KeyCode.Q); }
        public static bool Interactuar() { return Input.GetKeyDown(KeyCode.E); }
        public static bool Pausa() { return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P); }
        public static bool Continuar() { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.N); }
#endif
    }
}
