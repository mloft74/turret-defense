using Microsoft.Xna.Framework;

namespace TurretDefense.Services.Interfaces;

public interface IWorldRenderConverter
{
    Vector2 ConvertPositionToRender(Vector2 worldVector);

    Vector2 ConvertSizeToRender(Vector2 worldVector);

    Vector2 ConvertPositionToWorld(Vector2 renderVector);

    Vector2 ConvertSizeToWorld(Vector2 renderVector);
}
