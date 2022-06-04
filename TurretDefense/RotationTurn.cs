using System;
using Microsoft.Xna.Framework;

namespace TurretDefense;

public static class RotationTurn
{
    public static float ComputeRotationDiff(
        float currentAngle,
        Vector2 target,
        Vector2 position,
        float rotationRate,
        GameTime gameTime)
    {
        var rotationDirection = ComputeRotationDirection(currentAngle, target, position);
        var rotation = rotationRate * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (float.IsNaN(rotationDirection)) return 0.0f;

        if (rotationDirection == 0)
        {
            rotationDirection = 1;
        }

        return MathF.Sign(rotationDirection) * rotation;
    }

    private static float ComputeRotationDirection(float angle, Vector2 target, Vector2 position)
    {
        var direction = new Vector2(
            MathF.Cos(angle),
            MathF.Sin(angle));
        var diff = target - position;
        diff.Normalize();

        return Determinant(direction, diff);
    }

    private static float Determinant(Vector2 row1, Vector2 row2)
    {
        var (x1, y1) = row1;
        var (x2, y2) = row2;
        return x1 * y2 - x2 * y1;
    }

}
