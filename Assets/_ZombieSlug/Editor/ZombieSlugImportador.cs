// -----------------------------------------------------------------------------
// ZombieSlugImportador.cs - Importación homogénea del arte (AssetPostprocessor).
// Todo PNG que entre en Assets/_ZombieSlug/Art queda como Sprite (2D and UI),
// modo Single, 32 píxeles por unidad (1 unidad = 1 m), filtro Point (pixel art),
// sin compresión ni mipmaps. Personajes y tablero con pivote abajo al centro,
// para que su posición en Y coincida con los pies.
// -----------------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace ZombieSlug.EditorTools
{
    public class ZombieSlugImportador : AssetPostprocessor
    {
        public const int PPU = 32;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/_ZombieSlug/Art/")) return;
            Aplicar((TextureImporter)assetImporter, assetPath);
        }

        public static void Aplicar(TextureImporter ti, string ruta)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = PPU;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            bool abajo = ruta.Contains("/Sprites/Jugador/") || ruta.Contains("/Sprites/Enemigos/") || ruta.Contains("tablero_01");
            s.spriteAlignment = abajo ? (int)SpriteAlignment.BottomCenter : (int)SpriteAlignment.Center;
            s.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(s);
        }
    }
}
