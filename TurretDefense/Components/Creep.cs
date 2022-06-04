using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Services.Interfaces;
using static TurretDefense.RotationTurn;

namespace TurretDefense.Components;

[Flags]
public enum CreepType
{
    None = 0,
    Air = 1,
    Ground = 1 << 1,
}

public class Creep : IUpdatable, IRenderable
{
    public CreepType Type { get; }

    public int Points { get; }

    public float Radius { get; }

    public bool IsFinished { get; private set; } = false;

    public bool IsDead { get; private set; } = false;

    public Vector2 Position => _textures.Position;

    private const float ROTATION_RATE = MathHelper.Pi / 1000;

    private readonly Grid _grid;
    private List<Point>? _path;
    private readonly Opening _exit;
    private readonly CreepTextures _textures;
    private readonly float _speed;
    private readonly int _maxHealthPoints;
    private int _healthPoints;

    public Creep(
        CreepType isGround,
        int points,
        Grid grid,
        List<Point>? path,
        Opening exit,
        CreepTextures textures,
        float speed,
        int healthPoints)
    {
        Type = isGround;
        Points = points;
        _grid = grid;
        _path = path;
        _exit = exit;
        _textures = textures;
        _speed = speed;
        _maxHealthPoints = healthPoints;
        _healthPoints = healthPoints;
        Radius = _textures.Size.X * 0.5f;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        if (_path == null || _path.Count == 0) return;
        var unitOffset = new Vector2(0.5f);
        var gridPosition = Grid.WorldToGrid(_textures.Position).ToVector2() + unitOffset;
        var frontPosition = _path[0].ToVector2() + unitOffset;
        if (gridPosition == frontPosition)
        {
            _path.RemoveAt(0);
            if (_path.Count == 0)
            {
                IsFinished = true;
                return;
            }
        }
        var nextPosition = _path[0].ToVector2() + unitOffset;
        var targetPosition = Grid.GridToWorld(nextPosition);
        var diff = targetPosition - _textures.Position;
        diff.Normalize();
        _textures.Position += diff * _speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        var rotationDiff = ComputeRotationDiff(
            _textures.Rotation,
            targetPosition,
            _textures.Position,
            ROTATION_RATE,
            gameTime);
        _textures.Rotation = MathHelper.WrapAngle(_textures.Rotation + rotationDiff);
    }

    public void Render(IRenderManager renderManager)
    {
        _textures.Render(renderManager);
    }

    public void CalculatePath()
    {
        _path = _grid.PathForCreep(_textures.Position, _exit, Type)?.ToList();
    }

    public void Damage(int projectileDamageValue)
    {
        _healthPoints -= projectileDamageValue;
        if (_healthPoints <= 0)
        {
            IsDead = true;
        }
        _textures.HealthPercent = (float)_healthPoints / _maxHealthPoints;
    }
}
