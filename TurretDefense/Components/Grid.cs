using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using static TurretDefense.Constants;

namespace TurretDefense.Components;

public enum Opening
{
    Left,
    Top,
    Right,
    Bottom
}

public class Grid
{
    private const char FREE = ' ';
    private const char BLOCKED = 'X';
    private const char ADJACENT = 'O';

    private const float CELL_SIZE = (float)WORLD_SIZE / GRID_NUM;

    private readonly List<List<char>> _groundGrid;
    private readonly List<List<char>> _airGrid;

    public Grid()
    {
        _groundGrid = GenerateGrid();
        _airGrid = GenerateGrid();
    }

    private static List<List<char>> GenerateGrid()
    {
        var grid = new List<List<char>>();
        const int openSpaceOffset = (GRID_NUM - TURRET_GRID_SIZE) / 2 - 1; // - 1 to make it zero based

        for (var rowNum = 0; rowNum < GRID_NUM; ++rowNum)
        {
            var row = new List<char>();
            for (var colNum = 0; colNum < GRID_NUM; ++colNum)
            {
                var col = FREE;
                if ((rowNum == 0 ||
                        colNum == 0 ||
                        rowNum == GRID_NUM - 1 ||
                        colNum == GRID_NUM - 1) &&
                    (rowNum <= openSpaceOffset ||
                        rowNum > openSpaceOffset + TURRET_GRID_SIZE) &&
                    (colNum <= openSpaceOffset ||
                        colNum > openSpaceOffset + TURRET_GRID_SIZE))
                {
                    col = BLOCKED;
                }

                row.Add(col);
            }
            grid.Add(row);
        }

        GenerateAdjacents(grid);
        // PrintGrid(grid);

        return grid;
    }

    private static void GenerateAdjacents(List<List<char>> grid)
    {
        const int offset = 1;
        for (var row = 0; row < GRID_NUM; ++row)
        {
            for (var col = 0; col < GRID_NUM; ++col)
            {
                if (grid[row][col] == BLOCKED) continue;
                grid[row][col] = FREE;
                for (var rowOffset = -offset; rowOffset <= offset; ++rowOffset)
                {
                    for (var colOffset = -offset; colOffset <= offset; ++colOffset)
                    {
                        var rowCheck = row + rowOffset;
                        var colCheck = col + colOffset;
                        if (rowCheck < 0 || colCheck < 0 || rowCheck > GRID_NUM - 1 || colCheck > GRID_NUM - 1) continue;
                        if (grid[rowCheck][colCheck] != BLOCKED) continue;
                        grid[row][col] = ADJACENT;
                    }
                }
            }
        }
    }

    public static Vector2 GridToWorld(Point position)
    {
        return GridToWorld(position.ToVector2());
    }

    public static Vector2 GridToWorld(Vector2 position)
    {
        return position * CELL_SIZE;
    }

    public static Point WorldToGrid(Vector2 position)
    {
        return (position / CELL_SIZE).ToPoint();
    }

    public static Vector2 WorldPositionForOpening(Opening opening, Vector2 gridUnitOffset)
    {
        var position = GridPositionForOpening(opening).ToVector2();
        return GridToWorld(position + gridUnitOffset);
    }

    public static Point GridPositionForOpening(Opening opening)
    {
        const int mid = GRID_NUM / 2;
        return opening switch
        {
            Opening.Left => new(0, mid),
            Opening.Top => new(mid, 0),
            Opening.Right => new(GRID_NUM - 1, mid),
            Opening.Bottom => new(mid, GRID_NUM - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(opening), opening, null)
        };
    }

    public IEnumerable<Point>? PathForCreep(Vector2 position, Opening exit, CreepType type)
    {
        var start = WorldToGrid(position);
        var end = GridPositionForOpening(exit);
        return type switch
        {
            CreepType.Air => AStar.Solve(_airGrid, FREE, start, end),
            CreepType.Ground => AStar.Solve(_groundGrid, FREE, start, end),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void AddTurret(Vector2 position)
    {
        var (col, row) = WorldToGrid(position);
        AddTurret(row, col, _groundGrid);
        // PrintGrid(_groundGrid);
    }

    private void AddTurret(int row, int col, List<List<char>> grid)
    {
        if (!IsTurretPositionValid(row, col))
        {
            throw new ArgumentException($"{nameof(row)}: {row}, {nameof(col)}: {col} is an invalid position in the grid");
        }

        const int offset = TURRET_GRID_SIZE / 2;
        for (var rowOffset = -offset; rowOffset <= offset; ++rowOffset)
        {
            for (var colOffset = -offset; colOffset <= offset; ++colOffset)
            {
                grid[row + rowOffset][col + colOffset] = BLOCKED;
            }
        }

        GenerateAdjacents(grid);
    }

    public void RemoveTurret(Vector2 position)
    {
        var (col, row) = WorldToGrid(position);
        const int offset = (TURRET_GRID_SIZE + 2) / 2;
        for (var rowOffset = -offset; rowOffset <= offset; ++rowOffset)
        {
            for (var colOffset = -offset; colOffset <= offset; ++colOffset)
            {
                var rowNum = row + rowOffset;
                var colNum = col + colOffset;
                if (rowOffset == -offset ||
                    colOffset == -offset ||
                    rowOffset == offset ||
                    colOffset == offset)
                {
                    if (_groundGrid[rowNum][colNum] == ADJACENT)
                    {
                        _groundGrid[rowNum][colNum] = FREE;
                    }
                }
                else
                {
                    _groundGrid[rowNum][colNum] = FREE;
                }
            }
        }

        GenerateAdjacents(_groundGrid);
        // PrintGrid(_groundGrid);
    }

    public bool IsTurretPreviewPositionValid(Vector2 position)
    {
        var (col, row) = WorldToGrid(position);
        return IsTurretPositionValid(row, col) && !DoesPreviewBlockAnyPath(row, col);
    }

    private bool IsTurretPositionValid(int row, int col)
    {
        return IsTurretInBounds(row, col) && IsPlaceFree(row, col);
    }

    private static bool IsTurretInBounds(int row, int col)
    {
        return row >= 0 && col >= 0 && row < GRID_NUM && col < GRID_NUM;
    }

    private bool IsPlaceFree(int row, int col)
    {
        const int offset = TURRET_GRID_SIZE / 2;
        for (var rowOffset = -offset; rowOffset <= offset; ++rowOffset)
        {
            for (var colOffset = -offset; colOffset <= offset; ++colOffset)
            {
                var rowNum = row + rowOffset;
                var colNum = col + colOffset;
                if (rowNum < 0 ||
                    colNum < 0 ||
                    rowNum >= _groundGrid.Count ||
                    colNum >= _groundGrid.Count ||
                    _groundGrid[rowNum][colNum] == BLOCKED) return false;
            }
        }
        return true;
    }

    private bool DoesPreviewBlockAnyPath(int row, int col)
    {
        var grid = CopyGrid(_groundGrid);
        AddTurret(row, col, grid);

        var paths = new List<(Opening, Opening)>
        {
            (Opening.Left, Opening.Right),
            (Opening.Top, Opening.Bottom)
        };

        foreach (var (startOpening, endOpening) in paths)
        {
            var start = GridPositionForOpening(startOpening);
            var end = GridPositionForOpening(endOpening);
            var result = AStar.Solve(grid, FREE, start, end);
            if (result == null) return true;
        }

        return false;
    }

    private static void PrintGrid(List<List<char>> grid)
    {
        foreach (var row in grid)
        {
            foreach (var col in row)
            {
                Console.Write($"{col} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private static List<List<char>> CopyGrid(List<List<char>> grid)
    {
        return grid.Select(row => new List<char>(row)).ToList();
    }
}
