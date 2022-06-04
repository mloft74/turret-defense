using Microsoft.Xna.Framework;

namespace TurretDefense.Models;

public class RenderString
{
    public string FontName { get; }

    public string Text { get; }

    public Vector2 Position { get; set; }

    public Color RenderColor { get; set; }

    public Vector2 Scale { get; }

    public float LayerDepth { get; }

    public float Rotation { get; }

    public Vector2 UnitOrigin { get; }

    public RenderString(
        string fontName,
        string text,
        Vector2 position,
        Color renderColor,
        Vector2 scale,
        float layerDepth,
        float rotation = 0,
        Vector2? unitOrigin = null)
    {
        FontName = fontName;
        Text = text;
        Position = position;
        RenderColor = renderColor;
        Scale = scale;
        LayerDepth = layerDepth;
        Rotation = rotation;
        UnitOrigin = unitOrigin ?? Vector2.Zero;
    }


    public RenderString Copy(Color? color = null)
    {
        return new(
            FontName,
            Text,
            Position,
            color ?? RenderColor,
            Scale,
            LayerDepth,
            Rotation,
            UnitOrigin);
    }
}
