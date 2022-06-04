using System;
using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Components;

public class CreepPoint : IUpdatable, IRenderable
{
    public TimeSpan Timeout { get; } = TimeSpan.FromMilliseconds(1000);

    public TimeSpan TimeAlive { get; private set; }

    private RenderString _text;

    private readonly Vector2 _velocity = -Vector2.UnitY * WORLD_SIZE * 0.1f / 1000;

    public CreepPoint(RenderString text, TimeSpan? timeAlive = null)
    {
        _text = text;
        TimeAlive = timeAlive ?? TimeSpan.Zero;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var millis = (float) gameTime.ElapsedGameTime.TotalMilliseconds;
        _text.Position += _velocity * millis;
        TimeAlive += TimeSpan.FromMilliseconds(millis);
        var alpha = 1.0f - (float)(TimeAlive / Timeout);
        _text.RenderColor = new(_text.RenderColor, alpha);
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderString(_text);
    }
}
