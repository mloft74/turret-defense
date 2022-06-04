using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Components;

public class MenuSelector : IUpdatable, IRenderable
{
    public int Selection { get; set; } = 0;

    private readonly RenderTexture _texture;
    private readonly Vector2 _initialPosition;

    public MenuSelector(RenderTexture texture, Vector2 initialPosition)
    {
        _texture = texture;
        _initialPosition = initialPosition;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        _texture.Position = _initialPosition + Selection * _texture.Size * Vector2.UnitY;
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderTexture(_texture);
    }
}
