using System;
using Microsoft.Xna.Framework;

namespace TurretDefense;

public static class RandomNormal
{
    private static readonly Random _rng = new();

    // Box-Muller: https://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform
    public static float Next(float mean = 0.0f, float standardDeviation = 1.0f)
    {
        var U1 = (float) _rng.NextDouble();
        var U2 = (float) _rng.NextDouble();
        var R = MathF.Sqrt(-2 * MathF.Log(U1));
        var Theta = MathHelper.Tau * U2;
        var Z = R * MathF.Cos(Theta);
        var N = mean + Z * standardDeviation;
        return N;
    }
}
