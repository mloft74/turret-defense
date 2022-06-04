using Microsoft.Xna.Framework;

namespace TurretDefense;

public record Node(
    Point Position,
    int PathCost,
    int DistanceRemaining,
    Node? Parent)
{
    public int TotalCost => PathCost + DistanceRemaining;
}
