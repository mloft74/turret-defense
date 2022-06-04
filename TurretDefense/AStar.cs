using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TurretDefense;

 // Sebastian Lague's video helped me write this
 // https://youtu.be/-L-WgKMFuhE
public static class AStar
{
    private const int HORIZONTAL_COST = 10;
    private const int DIAGONAL_COST = 14;

    public static IEnumerable<Point>? Solve(List<List<char>> grid, char free, Point start, Point end)
    {
        var open = new List<Node>();
        var closed = new List<Point>();
        open.Add(GenerateNode(start, null, end, false));

        while (true)
        {
            if (open.Count == 0) return null;
            var currentNode = open[0];
            var position = currentNode.Position;
            open.RemoveAt(0);
            closed.Add(position);

            if (position == end) return ConvertToList(currentNode);

            var (col, row) = position;
            for (var rowOffset = -1; rowOffset <= 1; ++rowOffset)
            {
                for (var colOffset = -1; colOffset <= 1; ++colOffset)
                {
                    var rowNum = row + rowOffset;
                    var colNum = col + colOffset;
                    if (rowOffset == 0 &&
                        colOffset == 0 ||
                        rowNum < 0 ||
                        colNum < 0 ||
                        rowNum >= grid.Count ||
                        colNum >= grid.Count ||
                        grid[rowNum][colNum] != free) continue;

                    var nextPosition = position + new Point(colOffset, rowOffset);
                    if (closed.Contains(nextPosition)) continue;

                    var isDiagonal = rowOffset != 0 && colOffset != 0;
                    var node = GenerateNode(nextPosition, currentNode, end, isDiagonal);
                    PriorityInsert(open, node);
                }
            }
        }
    }

    private static IEnumerable<Point> ConvertToList(Node node)
    {
        var list = new List<Point>();
        var current = node;
        while (current != null)
        {
            list.Add(current.Position);
            current = current.Parent;
        }
        list.Reverse();
        return list;
    }

    private static Node GenerateNode(Point point, Node? parent, Point end, bool isDiagonal)
    {
        var cost = isDiagonal ? DIAGONAL_COST : HORIZONTAL_COST; // see video above
        var pathCost = parent?.PathCost + cost ?? 0;
        var distanceToEnd = DistanceToEnd(point, end);
        return new(point, pathCost, distanceToEnd, parent);
    }

    private static int DistanceToEnd(Point first, Point second)
    {
        var (dx, dy) = first - second;
        dx = Math.Abs(dx);
        dy = Math.Abs(dy);
        var min = Math.Min(dx, dy);
        var max = Math.Max(dx, dy);
        var diff = max - min;
        return HORIZONTAL_COST * diff + DIAGONAL_COST * min;
    }

    private static void PriorityInsert(List<Node> open, Node node) // combine these loops if time allows
    {
        var toRemove = new List<Node>();
        var shouldReturn = false;
        foreach (var n in open)
        {
            if (n.Position != node.Position) continue;
            if (n.TotalCost > node.TotalCost)
            {
                toRemove.Add(n);
            }
            else
            {
                shouldReturn = true;
            }
        }
        foreach (var n in toRemove)
        {
            open.Remove(n);
        }
        if (shouldReturn) return;

        var count = open.Count;
        var index = 0;
        var isInserted = false;
        while (!isInserted && index < count)
        {
            var current = open[index];
            if (node.TotalCost <= current.TotalCost)
            {
                open.Insert(index, node);
                isInserted = true;
            }
            ++index;
        }
        if (!isInserted) open.Add(node);
    }
}
