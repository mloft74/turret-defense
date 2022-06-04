using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using TurretDefense.Components;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Views;

public class GameplayView : IView
{
    public GameState NextState { get; private set; } = GameState.Pause;

    public GameState PreviousState { get; set; }

    public bool IsFinished { get; set; } = false;

    public bool ShouldTransition { get; set; } = false;

    private readonly Grid _grid;
    private readonly Func<Grid, Creep> _generateCreep;
    private readonly Func<int> _generateWaves;
    private readonly Func<TurretTextures, Turret> _generateTurret;
    private readonly Func<int, RenderString> _generateMoney;
    private readonly Func<int, RenderString> _generateScore;
    private readonly Func<int, RenderString> _generateHealth;
    private readonly Func<int, RenderString> _generateLevel;
    private readonly List<TurretTextures> _shopButtons;
    private readonly List<RenderString> _costs;
    private readonly RenderString _startLevel;
    private readonly RenderString _musicToggle;
    private readonly ParticleSystem _particleSystem;
    private readonly List<RenderTexture> _generalRenderTextures;

    private readonly List<Creep> _creeps = new();
    private readonly List<Turret> _turrets = new();
    private bool _spawning = false;

    private TimeSpan _spawnRate = TimeSpan.FromMilliseconds(1000);
    private TimeSpan _timeFromLastSpawn = TimeSpan.Zero;
    private TurretTextures? _turretPreview;
    private Turret? _selectedTurret;
    private Song? _song = null;
    private bool _isPlaying = false;
    private bool _shouldPlay = true;
    private int _wavesToSpawn = 0;
    private float _spawnNum = 1;
    private int _money = 70;
    private int _score = 0;
    private int _health = 50;
    private int _level = 0;

    public GameplayView(
        Grid grid,
        Func<Grid, Creep> generateCreep,
        Func<int> generateWaves,
        Func<TurretTextures, Turret> generateTurret,
        Func<int, RenderString> generateMoney,
        Func<int, RenderString> generateScore,
        Func<int, RenderString> generateHealth,
        Func<int, RenderString> generateLevel,
        List<TurretTextures> shopButtons,
        List<RenderString> costs,
        RenderString startLevel,
        RenderString musicToggle,
        ParticleSystem particleSystem,
        List<RenderTexture> generalRenderTextures)
    {
        _grid = grid;
        _generateCreep = generateCreep;
        _generateWaves = generateWaves;
        _generateTurret = generateTurret;
        _generateMoney = generateMoney;
        _generateScore = generateScore;
        _generateHealth = generateHealth;
        _generateLevel = generateLevel;
        _shopButtons = shopButtons;
        _costs = costs;
        _startLevel = startLevel;
        _musicToggle = musicToggle;
        _particleSystem = particleSystem;
        _generalRenderTextures = generalRenderTextures;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var resourceManager = services.GetService<IResourceManager>();
        var inputManager = services.GetService<IInputManager>();

        if (_health <= 0)
        {
            IsFinished = true;
            ShouldTransition = true;
            NextState = GameState.HighScores;
            services.GetService<IScoreManager>().PostScore(_score);
            MediaPlayer.Stop();
        }

        if (_song == null)
        {
            _song = resourceManager.GetSong(SONG);
            MediaPlayer.Play(_song);
            MediaPlayer.IsRepeating = true;
            _isPlaying = true;
        }

        if (!_isPlaying && _shouldPlay)
        {
            _isPlaying = true;
            MediaPlayer.Resume();
        }
        else if (!_shouldPlay)
        {
            _isPlaying = false;
            MediaPlayer.Pause();
        }

        _particleSystem.Update(gameTime, services);

        if (_spawning)
        {
            _timeFromLastSpawn += gameTime.ElapsedGameTime;
            if (_timeFromLastSpawn > _spawnRate)
            {
                const int mean = 1000;
                const int stdDev = 200;
                var time = MathF.Abs(RandomNormal.Next(mean, stdDev));
                _timeFromLastSpawn -= _spawnRate;
                _spawnRate = TimeSpan.FromMilliseconds(time);
                SpawnWave();
            }
            if (_wavesToSpawn == 0)
            {
                _spawning = false;
            }
        }

        var mousePosition = inputManager.MouseWorldPosition;

        var toRemove = new List<Creep>();
        foreach (var creep in _creeps)
        {
            if (creep.IsFinished)
            {
                toRemove.Add(creep);
                --_health;
            }
            else if (creep.IsDead)
            {
                toRemove.Add(creep);
                resourceManager.GetSound(CREEP_DEAD).Play();
                _particleSystem.CreepExplosion(creep.Position);
                _particleSystem.CreepPoints(creep.Position, creep.Points);
                _money += creep.Points;
                _score += creep.Points;
            }
            else
            {
                creep.Update(gameTime, services);
            }
        }
        foreach (var creep in toRemove)
        {
            _creeps.Remove(creep);
        }

        foreach (var turret in _turrets)
        {
            Creep? target = null;
            foreach (var creep in _creeps)
            {
                var type = creep.Type & turret.Type;
                if (type == CreepType.None ||
                    target != null ||
                    !DoCirclesIntersect(creep.Position, creep.Radius, turret.Position, turret.Radius)) continue;
                target = creep;
            }
            if (target != null)
            {
                turret.SetTarget(target);
            }
            else
            {
                turret.RemoveTarget();
            }
            turret.Update(gameTime, services);
        }

        var explosions = new List<(Vector2, int, CreepType)>();
        foreach (var turret in _turrets)
        {
            foreach (var creep in _creeps)
            {
                var type = creep.Type & turret.Type;
                if (type == CreepType.None) continue;
                foreach (var projectile in turret.Projectiles)
                {
                    if (!DoCirclesIntersect(creep.Position, creep.Radius, projectile.Position, projectile.Radius)) continue;
                    creep.Damage(projectile.DamageValue);
                    resourceManager.GetSound(CREEP_HIT).Play();
                    projectile.IsFinished = true;
                    if (!projectile.Explodes) continue;
                    _particleSystem.BombExplosion(projectile.Position);
                    if (!projectile.IsBomb) continue;
                    explosions.Add((projectile.Position, projectile.DamageValue, turret.Type));
                    resourceManager.GetSound(EXPLOSION).Play();
                }
            }
        }
        const float bombRadius = 0.1f * WORLD_SIZE;
        foreach (var (position, damage, turretType) in explosions)
        {
            foreach (var creep in _creeps)
            {
                var type = creep.Type & turretType;
                if (type == CreepType.None) continue;
                if (!DoCirclesIntersect(
                    creep.Position,
                    creep.Radius,
                    position,
                    bombRadius)) continue;
                creep.Damage(damage);
            }
        }

        if (_turretPreview != null)
        {
            if (IsPositionInWorld(mousePosition))
            {
                var unitOffset = new Vector2(0.5f);
                var cellPosition = Grid.WorldToGrid(mousePosition).ToVector2();
                var position = Grid.GridToWorld(cellPosition + unitOffset);
                _turretPreview.SetPosition(position);
                var intersectsWithCreep = _creeps.Any(c => DoCirclesIntersect(c.Position, c.Radius, _turretPreview.Position, _turretPreview.Radius));
                _turretPreview.RangeColor = _grid.IsTurretPreviewPositionValid(position) && !intersectsWithCreep?
                    Color.DarkBlue :
                    Color.Red;
            }
            else
            {
                _turretPreview.SetPosition(mousePosition);
            }
        }

        HandleInput(inputManager.GetInput(ESCAPE), () =>
        {
            ShouldTransition = true;
            resourceManager.GetSound(MENU_BLIP).Play();
            _isPlaying = false;
            MediaPlayer.Pause();
        });

        HandleInput(inputManager.GetInput(MOUSE_LEFT), () =>
        {
            if (mousePosition.X < WORLD_SIZE)
            {
                DeselectTurret();
            }
            else if (_selectedTurret != null)
            {
                if (inputManager.IsMouseIntersecting(_selectedTurret.UpgradeRenderString))
                {
                    UpgradeTurret();
                }
                else if (inputManager.IsMouseIntersecting(_selectedTurret.SellRenderString))
                {
                    SellTurret(resourceManager);
                }
            }

            var button = _shopButtons.FirstOrDefault(b => inputManager.IsMouseIntersecting(b.TurretHead));
            if (button != null)
            {
                SetPreviewFromButton(button);
            }
            else if (inputManager.IsMouseIntersecting(_startLevel))
            {
                StartLevel();
            }
            else if (inputManager.IsMouseIntersecting(_musicToggle))
            {
                _musicToggle.RenderColor = _musicToggle.RenderColor == Color.Crimson ?
                    Color.Green :
                    Color.Crimson;
                _shouldPlay = !_shouldPlay;
            }
            else if (_turretPreview != null)
            {
                var intersectsWithCreep = _creeps.Any(c => DoCirclesIntersect(c.Position, c.Radius, _turretPreview.Position, _turretPreview.Radius));
                if (IsRenderTextureInWorld(_turretPreview.TurretHead) &&
                    _grid.IsTurretPreviewPositionValid(_turretPreview.Position) &&
                    !intersectsWithCreep)
                {
                    var newTurret = _generateTurret(_turretPreview);
                    if (_money >= newTurret.BuyCost)
                    {
                        _money -= newTurret.BuyCost;
                        resourceManager.GetSound(TURRET_PLACE).Play();
                        _turrets.Add(newTurret);
                        _grid.AddTurret(_turretPreview.Position);
                        CalculatePaths();
                    }
                }
                RemovePreview();
            }

            var turret = _turrets.FirstOrDefault(t => inputManager.IsMouseIntersecting(t.TurretHead));
            if (turret != null)
            {
                SelectTurret(turret);
            }
        });

        HandleInput(inputManager.GetInput(MOUSE_RIGHT), RemovePreview);

        HandleInput(inputManager.GetInput(START_LEVEL), StartLevel);

        if (_selectedTurret == null) return;
        HandleInput(inputManager.GetInput(UPGRADE), UpgradeTurret);
        HandleInput(inputManager.GetInput(SELL), () => SellTurret(resourceManager));
    }

    public void Render(IRenderManager renderManager)
    {
        _particleSystem.Render(renderManager);
        foreach (var button in _shopButtons)
        {
            button.Render(renderManager);
        }
        renderManager.QueueRenderStrings(_costs);
        renderManager.QueueRenderString(_generateMoney(_money));
        renderManager.QueueRenderString(_generateScore(_score));
        renderManager.QueueRenderString(_generateHealth(_health));
        renderManager.QueueRenderString(_generateLevel(_level));

        renderManager.QueueRenderTextures(_generalRenderTextures);
        renderManager.QueueRenderString(_startLevel);
        renderManager.QueueRenderString(_musicToggle);

        foreach (var creep in _creeps)
        {
            creep.Render(renderManager);
        }
        foreach (var turret in _turrets)
        {
            turret.Render(renderManager);
        }

        _turretPreview?.Render(renderManager);
    }

    private void UpgradeTurret()
    {
        if (_selectedTurret == null) throw new ArgumentNullException(nameof(_selectedTurret), "turret to sell was null");
        if (_money < _selectedTurret.UpgradeCost) return;
        _money -= _selectedTurret.UpgradeCost;
        _selectedTurret.Upgrade();
    }

    private void SellTurret(IResourceManager resourceManager)
    {
        if (_selectedTurret == null) throw new ArgumentNullException(nameof(_selectedTurret), "turret to sell was null");
        _turrets.Remove(_selectedTurret);
        _grid.RemoveTurret(_selectedTurret.Position);
        _particleSystem.BombExplosion(_selectedTurret.Position);
        _money += _selectedTurret.SellCost;
        resourceManager.GetSound(TURRET_PLACE).Play();
        DeselectTurret();
        CalculatePaths();
    }

    private void CalculatePaths()
    {
        foreach (var creep in _creeps)
        {
            creep.CalculatePath();
        }
    }

    private void SelectTurret(Turret turret)
    {
        _selectedTurret = turret;
        _selectedTurret.SetIsSelected(true);
    }

    private void DeselectTurret()
    {
        _selectedTurret?.SetIsSelected(false);
        _selectedTurret = null;
    }

    private void SetPreviewFromButton(TurretTextures button)
    {
        _turretPreview = button.Copy(PREVIEW_TURRET_HEAD_DEPTH, PREVIEW_TURRET_BASE_DEPTH, PREVIEW_TURRET_RANGE_DEPTH);
        _turretPreview.ShouldRenderSelected = true;
    }

    private void RemovePreview()
    {
        if (_turretPreview != null) _turretPreview.ShouldRenderSelected = false;
        _turretPreview = null;
    }

    private void StartLevel()
    {
        ++_level;
        _wavesToSpawn += _generateWaves();
        if (!_spawning)
        {
            SpawnWave();
        }
        _spawning = true;
    }

    private void SpawnWave()
    {
        if (_wavesToSpawn == 0) return;
        --_wavesToSpawn;
        var spawn = (int)_spawnNum;
        _spawnNum += 0.025f;
        for (var i = 0; i < spawn; ++i)
        {
            _creeps.Add(_generateCreep(_grid));
        }
    }

    private static void HandleInput(InputState state, Action action)
    {
        var (wasPressed, isPressed) = state;
        if (wasPressed || !isPressed) return;
        action();
    }

    private static bool DoCirclesIntersect(Vector2 center1, float radius1, Vector2 center2, float radius2)
    {
        var difference = center1 - center2;
        return difference.Length() <= radius1 + radius2;
    }

    private static bool IsRenderTextureInWorld(RenderTexture texture)
    {
        var topLeft = texture.Position - texture.UnitOrigin * texture.Size;
        var bottomRight = topLeft + texture.Size;
        return IsPositionInWorld(topLeft) && IsPositionInWorld(bottomRight);
    }

    private static bool IsPositionInWorld(Vector2 position)
    {
        var (x, y) = position;
        return x >= 0 && y >= 0 && x < WORLD_SIZE && y < WORLD_SIZE;
    }
}
