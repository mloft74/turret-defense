using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using static TurretDefense.Constants;
using static TurretDefense.RotationTurn;

namespace TurretDefense.Components;

public class Turret : IUpdatable, IRenderable
{
    public float Radius { get; }

    public RenderString UpgradeRenderString { get; private set;}

    public RenderString SellRenderString { get; }

    public CreepType Type { get; }

    public List<Projectile> Projectiles { get; } = new List<Projectile>();

    public Vector2 Position => _textures.Position;

    public RenderTexture TurretHead => _textures.TurretHead.Copy();

    public int BuyCost => _upgrades[0];

    public int UpgradeCost => _upgrades[_level];

    public int SellCost => _sells[_level];

    private const int MAX_LEVEL = 3;
    private const float ROTATION_RATE = MathHelper.Pi / 1000;

    private readonly List<int> _upgrades;
    private readonly List<int> _sells;
    private int _level;
    private readonly TurretTextures _textures;
    private readonly TimeSpan _fireRate;
    private readonly Func<float, int, Creep, Projectile> _generateProjectile;
    private readonly Func<int, RenderString> _generateUpgrade;
    private readonly Func<int, RenderString> _generateSell;

    private Creep? _target = null;
    private TimeSpan _timeSinceLastFire;
    private bool _isSelected = false;

    public Turret(
        float radius,
        RenderString upgradeRenderString,
        RenderString sellRenderString,
        CreepType type,
        List<int> upgrades,
        List<int> sells,
        int level,
        TurretTextures textures,
        TimeSpan fireRate,
        Func<float, int, Creep, Projectile> generateProjectile,
        Func<int, RenderString> generateUpgrade,
        Func<int, RenderString> generateSell)
    {
        Radius = radius;
        UpgradeRenderString = upgradeRenderString;
        SellRenderString = sellRenderString;
        Type = type;
        _upgrades = upgrades;
        _sells = sells;
        _level = level;
        _textures = textures;
        _fireRate = fireRate;
        _generateProjectile = generateProjectile;
        _generateUpgrade = generateUpgrade;
        _generateSell = generateSell;
        _timeSinceLastFire = _fireRate;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        _timeSinceLastFire += gameTime.ElapsedGameTime;
        if (_target != null)
        {
            var rotationDiff = ComputeRotationDiff(
                _textures.Rotation,
                _target.Position,
                _textures.Position,
                ROTATION_RATE,
                gameTime);
            _textures.Rotation = MathHelper.WrapAngle(_textures.Rotation + rotationDiff);

            if (_timeSinceLastFire > _fireRate)
            {
                Projectiles.Add(_generateProjectile(_textures.Rotation, _level, _target!));
                services.GetService<IResourceManager>().GetSound(SHOOT).Play();
                _timeSinceLastFire -= _fireRate;
            }
        }

        var toRemove = new List<Projectile>();
        foreach (var projectile in Projectiles)
        {
            projectile.Update(gameTime, services);
            if (!IsInBounds(projectile) || projectile.IsFinished)
            {
                toRemove.Add(projectile);
            }
        }
        foreach (var projectile in toRemove)
        {
            Projectiles.Remove(projectile);
        }
    }

    public void Render(IRenderManager renderManager)
    {
        _textures.Render(renderManager);
        foreach (var projectile in Projectiles)
        {
            projectile.Render(renderManager);
        }
        if (!_isSelected) return;
        renderManager.QueueRenderStrings(new[] { UpgradeRenderString, SellRenderString });
        renderManager.QueueRenderString(_generateUpgrade(UpgradeCost));
        renderManager.QueueRenderString(_generateSell(SellCost));
    }

    public void Upgrade()
    {
        if (_level == MAX_LEVEL) return;
        ++_level;
        _textures.TurretHead.TextureName = $"{_textures.TurretType}{_level}";
        if (_level == MAX_LEVEL)
        {
            UpgradeRenderString = UpgradeRenderString.Copy(Color.Gray);
        }
    }

    public void SetIsSelected(bool isSelected)
    {
        _isSelected = isSelected;
        _textures.ShouldRenderSelected = _isSelected;
    }

    public void SetTarget(Creep position)
    {
        if (_timeSinceLastFire > _fireRate)
        {
            _timeSinceLastFire = _fireRate;
        }
        _target = position;
    }

    public void RemoveTarget()
    {
        _target = null;
    }

    private static bool IsInBounds(Projectile projectile)
    {
        var (x, y) = projectile.Position;
        return x >= 0 && y >= 0 && x < WORLD_SIZE && y < WORLD_SIZE;
    }
}
