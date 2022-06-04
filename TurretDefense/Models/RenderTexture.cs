using System;
using Microsoft.Xna.Framework;

namespace TurretDefense.Models;

public class RenderTexture
{
    public string TextureName { get; set; }

    public Vector2 Position { get; set; }

    public Vector2 Size { get; set; }

    public Vector2 UnitOrigin { get; }

    public Color RenderColor { get; set; }

    public float LayerDepth { get; }

    public float Rotation { get; set; }

    public TimeSpan TimeStamp { get; set; }

    public RenderTexture(
        string textureName,
        Vector2 position,
        Vector2 size,
        Vector2 unitOrigin,
        Color renderColor,
        float layerDepth,
        float rotation = 0.0f,
        TimeSpan? timeStamp = null)
    {
        TextureName = textureName;
        Position = position;
        Size = size;
        UnitOrigin = unitOrigin;
        RenderColor = renderColor;
        LayerDepth = layerDepth;
        Rotation = rotation;
        TimeStamp = timeStamp ?? TimeSpan.Zero;
    }

    public RenderTexture Copy(float? layerDepth = null)
    {
        return new(
            TextureName,
            Position,
            Size,
            UnitOrigin,
            RenderColor,
            layerDepth ?? LayerDepth,
            Rotation,
            TimeStamp);
    }
}
