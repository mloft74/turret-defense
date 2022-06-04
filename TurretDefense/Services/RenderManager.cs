using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Services;

public class RenderManager : IRenderManager
{
    private readonly List<RenderTexture> _renderables = new();
    private readonly List<RenderString> _renderStrings = new();

    public void QueueRenderTexture(RenderTexture renderTexture)
    {
        _renderables.Add(renderTexture);
    }

    public void QueueRenderTextures(IEnumerable<RenderTexture> renderTextures)
    {
        _renderables.AddRange(renderTextures);
    }

    public void QueueRenderString(RenderString renderString)
    {
        _renderStrings.Add(renderString);
    }

    public void QueueRenderStrings(IEnumerable<RenderString> renderStrings)
    {
        _renderStrings.AddRange(renderStrings);
    }

    public void Draw(GraphicsDevice graphicsDevice, GameServiceContainer services, SpriteBatch spriteBatch, GameTime gameTime)
    {
        graphicsDevice.Clear(Color.Black);

        var converter = services.GetService<IWorldRenderConverter>();
        var resourceManager = services.GetService<IResourceManager>();

        spriteBatch.Begin(SpriteSortMode.FrontToBack);

        foreach (var renderable in _renderables)
        {
            var position = converter.ConvertPositionToRender(renderable.Position);

            var textureInfo = resourceManager.GetTextureInfo(renderable.TextureName);

            var size = converter.ConvertSizeToRender(renderable.Size);
            var scale = size * textureInfo.InverseScale;

            var origin = renderable.UnitOrigin * textureInfo.Size;

            var timeStamps = textureInfo.FrameTimeStamps;
            var time = renderable.TimeStamp;
            var index = 0;
            for (var timeStampsIndex = 0; timeStampsIndex < timeStamps.Count; ++timeStampsIndex)
            {
                if (time > timeStamps[timeStampsIndex]) continue;
                index = timeStampsIndex;
                break;
            }

            renderable.TimeStamp += gameTime.ElapsedGameTime;
            if (renderable.TimeStamp > textureInfo.TotalTime)
            {
                renderable.TimeStamp -= textureInfo.TotalTime;
            }

            var rectangle = new Rectangle(
                new Point(textureInfo.Size.ToPoint().X * index, 0),
                textureInfo.Size.ToPoint());

            spriteBatch.Draw(
                textureInfo.Texture,
                position,
                rectangle,
                renderable.RenderColor,
                renderable.Rotation,
                origin,
                scale,
                SpriteEffects.None,
                renderable.LayerDepth);
        }

        _renderables.Clear();

        foreach (var renderString in _renderStrings)
        {
            var position = converter.ConvertPositionToRender(renderString.Position);
            var convertedScale = converter.ConvertSizeToRender(renderString.Scale);
            var font = resourceManager.GetFont(renderString.FontName);
            var measurement = font.MeasureString(renderString.Text);
            var inverseScale = Vector2.One / measurement;
            var renderScale = convertedScale * inverseScale;
            spriteBatch.DrawString(
                font,
                renderString.Text,
                position,
                renderString.RenderColor,
                renderString.Rotation,
                renderString.UnitOrigin,
                renderScale,
                SpriteEffects.None,
                renderString.LayerDepth);
        }

        _renderStrings.Clear();

        spriteBatch.End();
    }
}
