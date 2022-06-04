using System;
using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Components;

public class Particle : IUpdatable, IRenderable
{
    public TimeSpan Timeout { get; }

    public TimeSpan TimeAlive { get; private set; }

    private readonly RenderTexture _texture;
    private readonly Vector2 _velocity;

    public Particle(RenderTexture texture, Vector2 velocity, TimeSpan timeout, TimeSpan? timeAlive = null)
    {
        _texture = texture;
        _velocity = velocity;
        Timeout = timeout;
        TimeAlive = timeAlive ?? TimeSpan.Zero;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var millis = (float) gameTime.ElapsedGameTime.TotalMilliseconds;
        _texture.Position += _velocity * millis;
        TimeAlive += TimeSpan.FromMilliseconds(millis);
        var alpha = 1.0f - (float)(TimeAlive / Timeout);
        _texture.RenderColor = new Color(_texture.RenderColor, alpha);
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderTexture(_texture);
    }
}
