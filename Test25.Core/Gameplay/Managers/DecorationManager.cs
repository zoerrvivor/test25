using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.Core.Gameplay.World;
using Test25.Core.Utilities;

namespace Test25.Core.Gameplay.Managers
{
    public class DecorationManager
    {
        private List<Texture2D> _decorationTextures;

        public DecorationManager(List<Texture2D> decorationTextures)
        {
            _decorationTextures = decorationTextures;
        }

        // Generate decorations by stamping their textures directly onto the terrain pixel data.
        public void GenerateDecorations(Terrain terrain)
        {
            int numDecorations = Rng.Range(3, 6); // Spawn 3 to 5 ruins
            int attempts = 0;
            while (numDecorations > 0 && attempts < 50)
            {
                attempts++;
                // Random X position, keeping away from very edges
                int x = Rng.Range(50, terrain.Width - 50);

                // Choose a random decoration texture
                Texture2D decoTexture = _decorationTextures[Rng.Range(0, _decorationTextures.Count)];

                // Find the lowest ground point under the decoration
                int halfWidth = decoTexture.Width / 2;
                int startX = x - halfWidth;
                int endX = x + halfWidth;
                if (startX < 0) startX = 0;
                if (endX >= terrain.Width) endX = terrain.Width - 1;

                int maxGroundY = -1;
                for (int checkX = startX; checkX <= endX; checkX++)
                {
                    int h = terrain.GetHeight(checkX);
                    if (h > maxGroundY) maxGroundY = h;
                }

                // Ensure decoration is on land (not underwater)
                if (maxGroundY < terrain.WaterLevel)
                {
                    int embedAmount = 15;
                    int y = maxGroundY + embedAmount - decoTexture.Height;
                    // Stamp the decoration onto the terrain
                    terrain.Blit(decoTexture, x - halfWidth, y);
                    numDecorations--;
                }
            }
        }

        public void AddDecoration(string type, Vector2 position, Terrain terrain)
        {
            if (_decorationTextures == null || _decorationTextures.Count == 0) return;

            // Choose texture based on type (simple mapping)
            Texture2D decoTexture = _decorationTextures[0];
            // Building ruins are typically the first few

            int halfWidth = decoTexture.Width / 2;
            int y = (int)position.Y - decoTexture.Height + 10; // Slight embed

            terrain.Blit(decoTexture, (int)position.X - halfWidth, y);
        }
    }
}
