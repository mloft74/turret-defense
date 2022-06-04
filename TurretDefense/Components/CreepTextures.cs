using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Components;

public class CreepTextures : IRenderable
{
    public Vector2 Position
    {
        get => _creepTexture.Position;
        set
        {
            _redBar.Position = value + _offset;
            _greenBar.Position = value + _offset;
            _creepTexture.Position = value;
        }
    }

    public Vector2 Size => _creepTexture.Size;

    public float Rotation
    {
        get => _creepTexture.Rotation;
        set => _creepTexture.Rotation = value;
    }

    public float HealthPercent
    {
        set
        {
            var (x, y) = _redBar.Size;
            _greenBar.Size = new(x * value, y);
        }
    }

    private readonly RenderTexture _redBar;
    private readonly RenderTexture _greenBar;
    private readonly RenderTexture _creepTexture;
    private readonly Vector2 _offset;

    public CreepTextures(RenderTexture redBar, RenderTexture greenBar, RenderTexture creepTexture)
    {
        _redBar = redBar;
        _greenBar = greenBar;
        _creepTexture = creepTexture;
        _offset = -Vector2.UnitY * (_creepTexture.Size * _creepTexture.UnitOrigin + _redBar.Size);
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderTextures(new[] { _redBar, _greenBar, _creepTexture });
    }
}
