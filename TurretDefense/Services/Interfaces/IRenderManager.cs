using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TurretDefense.Models;

namespace TurretDefense.Services.Interfaces;

public interface IRenderManager
{
    void QueueRenderTexture(RenderTexture renderTexture);

    void QueueRenderTextures(IEnumerable<RenderTexture> renderTextures);

    void QueueRenderString(RenderString renderString);

    void QueueRenderStrings(IEnumerable<RenderString> renderStrings);

    void Draw(GraphicsDevice graphicsDevice, GameServiceContainer services, SpriteBatch spriteBatch, GameTime gameTime);
}
