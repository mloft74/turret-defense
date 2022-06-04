using System;
using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Components;

public class Projectile : IUpdatable, IRenderable
{
    public float Radius { get; }

    public int DamageValue { get; }

    public bool IsBomb { get; }

    public bool Explodes { get; }

    public bool IsFinished { get; set; }

    public Vector2 Position => _texture.Position;

    private readonly RenderTexture _texture;
    private readonly Action<GameTime, GameServiceContainer> _innerUpdate;

    public Projectile(
        float radius,
        int damageValue,
        bool isBomb,
        bool explodes,
        RenderTexture texture,
        Action<GameTime, GameServiceContainer> innerUpdate)
    {
        Radius = radius;
        DamageValue = damageValue;
        IsBomb = isBomb;
        Explodes = explodes;
        _texture = texture;
        _innerUpdate = innerUpdate;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        _innerUpdate(gameTime, services);
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderTexture(_texture);
    }
}
