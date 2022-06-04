using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Components;

public class TurretTextures : IRenderable
{
    public string TurretType { get; }

    public RenderTexture TurretHead { get; }

    public bool ShouldRenderSelected { private get; set; }

    public float Radius { get; }

    public Vector2 Position => _turretBase.Position;

    public Color RangeColor { set => _turretRange.RenderColor = value; }

    public float Rotation
    {
        get => TurretHead.Rotation;
        set => TurretHead.Rotation = value;
    }

    private readonly RenderTexture _turretBase;
    private readonly RenderTexture _turretRange;

    public TurretTextures(
        string turretType,
        RenderTexture turretHead,
        RenderTexture turretBase,
        RenderTexture turretRange,
        bool shouldRenderSelected = false)
    {
        TurretType = turretType;
        TurretHead = turretHead;
        _turretBase = turretBase;
        _turretRange = turretRange;
        ShouldRenderSelected = shouldRenderSelected;
        Radius = _turretBase.Size.X * 0.5f;
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderTextures(new[] { TurretHead, _turretBase });
        if (!ShouldRenderSelected) return;
        renderManager.QueueRenderTexture(_turretRange);
    }

    public void SetPosition(Vector2 position)
    {
        TurretHead.Position = position;
        _turretBase.Position = position;
        _turretRange.Position = position;
    }

    public TurretTextures Copy(
        float? turretHeadDepth = null,
        float? turretBaseDepth = null,
        float? turretRangeDepth = null)
    {
        return new(
            TurretType,
            TurretHead.Copy(turretHeadDepth),
            _turretBase.Copy(turretBaseDepth),
            _turretRange.Copy(turretRangeDepth),
            ShouldRenderSelected);
    }
}
