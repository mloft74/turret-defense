using System;
using Microsoft.Xna.Framework;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Services;

public class WorldRenderConverter : IWorldRenderConverter
{
    private readonly int _renderSize;
    private readonly Vector2 _renderTranslateVector;

    public WorldRenderConverter(int width, int height)
    {
        _renderSize = Math.Min(width, height);

        var isWidthLarger = width > height;
        var diff = Math.Abs(width - height);
        var offset = diff / 2;
        _renderTranslateVector = isWidthLarger ?
            new Vector2(offset, 0) :
            new Vector2(0, offset);
    }

    public Vector2 ConvertPositionToRender(Vector2 worldVector)
    {
        var scaled = ConvertSizeToRender(worldVector);
        return scaled + _renderTranslateVector;
    }

    public Vector2 ConvertSizeToRender(Vector2 worldVector)
    {
        var unitVector = worldVector / Constants.WORLD_SIZE;
        return unitVector * _renderSize;
    }

    public Vector2 ConvertPositionToWorld(Vector2 renderVector)
    {
        var translated = renderVector - _renderTranslateVector;
        return ConvertSizeToWorld(translated);
    }

    public Vector2 ConvertSizeToWorld(Vector2 renderVector)
    {
        var unitVector = renderVector / _renderSize;
        return unitVector * Constants.WORLD_SIZE;
    }
}
