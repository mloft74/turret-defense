using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TurretDefense.Components;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views;
using TurretDefense.Views.Interfaces;
using static TurretDefense.Constants;
using static TurretDefense.RotationTurn;

// This file became a giant mess because I just wanted to finish the project.
// Why did Turret Defense turn out to be so complex?

namespace TurretDefense;

public static class Factory
{
    public static IView GenerateView(GameState gameState, GameServiceContainer services)
    {
        return gameState switch
        {
            GameState.Initialization => new InitializationView(),
            GameState.MainMenu => GenerateMainMenuView(services),
            GameState.Gameplay => GenerateGameplayView(services),
            GameState.Pause => GeneratePauseMenuView(services),
            GameState.HighScores => GenerateHighScoresMenuView(services),
            GameState.Keybinds => GenerateKeybindsMenuView(services),
            GameState.RebindUpgrade => GenerateRebindUpgradeView(services),
            GameState.RebindSell => GenerateRebindSellView(services),
            GameState.RebindStartLevel => GenerateRebindStartLevelView(services),
            GameState.ResetKeyBinds => new ResetKeyBindsView(),
            GameState.Credits => GenerateCreditsView(services),
            GameState.PreviousState => throw new ArgumentException(
                $"{Enum.GetName(typeof(GameState), GameState.PreviousState)} does not have an associated View",
                nameof(gameState)),
            GameState.Exit => GenerateExitView(),
            _ => throw new ArgumentOutOfRangeException(nameof(gameState), gameState, "Invalid Game State")
        };
    }

    private static MenuView GenerateMainMenuView(GameServiceContainer services)
    {
        const string title = "Main Menu";

        const string newGame = "New Game";
        const string highScores = "High Scores";
        const string rebindKeys = "Rebind Keys";
        const string credits = "Credits";
        const string exit = "Exit to Desktop";

        return GenerateMenuView(
            services,
            title,
            new()
            {
                newGame,
                highScores,
                rebindKeys,
                credits,
                exit
            },
            option => option switch
            {
                newGame => GameState.Gameplay,
                highScores => GameState.HighScores,
                rebindKeys => GameState.Keybinds,
                credits => GameState.Credits,
                exit => GameState.Exit,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(option),
                    option,
                    GenerateNextStateExceptionMessage(title))
            },
            option => option switch
            {
                newGame => true,
                highScores => false,
                rebindKeys => false,
                credits => false,
                exit => true,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(option),
                    option,
                    GenerateIsFinishedExceptionMessage(title))
            });
    }

    private static GameplayView GenerateGameplayView(GameServiceContainer services)
    {
        var grid = new Grid();
        var particleSystem = GenerateParticleSystem(services);

        const float inverseTurretScalar = (float)TURRET_GRID_SIZE / GRID_NUM;
        var turretSize = new Vector2(WORLD_SIZE * inverseTurretScalar);
        var turretRadius = turretSize.Length() * 1.5f;
        var turretUnitOrigin = new Vector2(0.5f);
        var generateTurret = GenerateGenerateTurret(services, turretRadius, particleSystem);
        var generateTurretButton = GenerateGenerateTurretButton(turretSize, turretRadius, turretUnitOrigin);
        var generateTurretCost = GenerateGenerateTurretCost(services);

        var basePosition = new Vector2(-0.05f * WORLD_SIZE, WORLD_SIZE * 0.5f);

        var baseButtonPosition = basePosition + (-Vector2.UnitX + turretUnitOrigin) * turretSize;
        var turretButton1 = generateTurretButton(TURRET_1, baseButtonPosition - turretSize * Vector2.UnitY * 2);
        var turretButton2 = generateTurretButton(TURRET_2, baseButtonPosition - turretSize * Vector2.UnitY);
        var turretButton3 = generateTurretButton(TURRET_3, baseButtonPosition);
        var turretButton4 = generateTurretButton(TURRET_4, baseButtonPosition + turretSize * Vector2.UnitY);
        var turretButtons = new List<TurretTextures> { turretButton1, turretButton2, turretButton3, turretButton4 };

        var baseCostPosition = baseButtonPosition - turretSize * turretUnitOrigin;
        var turretCost1 = generateTurretCost(TURRET_1_BUY, baseCostPosition - turretSize * Vector2.UnitY * 2);
        var turretCost2 = generateTurretCost(TURRET_2_BUY, baseCostPosition - turretSize * Vector2.UnitY);
        var turretCost3 = generateTurretCost(TURRET_3_BUY, baseCostPosition);
        var turretCost4 = generateTurretCost(TURRET_4_BUY, baseCostPosition + turretSize * Vector2.UnitY);
        var turretCosts = new List<RenderString> { turretCost1, turretCost2, turretCost3, turretCost4 };

        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        const string startText = "Start level";
        var (slX, slY) = font.MeasureString(startText);
        var slMeasurementScaledY = new Vector2(slX / slY, 1.0f); // 1.0f because uY / uY = 1
        var slScale = slMeasurementScaledY * SMALL_FONT;
        var startLevel = new RenderString(
            FONT_NAME,
            startText,
            new Vector2(-0.05f * WORLD_SIZE, WORLD_SIZE * 0.75f) - slScale * Vector2.UnitX,
            Color.White,
            slScale,
            UI_STRING_DEPTH);

        const string toggleMuteText = "Music";
        var (tmX, tmY) = font.MeasureString(toggleMuteText);
        var tmMeasurementScaledY = new Vector2(tmX / tmY, 1.0f); // 1.0f because uY / uY = 1
        var tmScale = tmMeasurementScaledY * SMALL_FONT;
        var toggleMute = new RenderString(
            FONT_NAME,
            toggleMuteText,
            new Vector2(-0.05f * WORLD_SIZE, WORLD_SIZE * 0.25f) - tmScale * Vector2.UnitX,
            Color.Green,
            tmScale,
            UI_STRING_DEPTH);

        var world = new RenderTexture(
            BACKGROUND,
            Vector2.Zero,
            new(WORLD_SIZE),
            Vector2.Zero,
            Color.White,
            WORLD_DEPTH);

        const float ratio = 7.0f / 18.0f; // this assumes a 16:9 ratio; 16 - 9 = 7, 7 / 2 / 9 = 7 / 18
        const float uiX = WORLD_SIZE * ratio;

        var shopBackground = new RenderTexture(
            BLANK,
            new(-uiX, 0),
            new(uiX, WORLD_SIZE),
            Vector2.Zero,
            Color.Black,
            UI_BACKGROUND_DEPTH);

        var manageBackground = new RenderTexture(
            BLANK,
            new(WORLD_SIZE, 0),
            new(uiX, WORLD_SIZE),
            Vector2.Zero,
            Color.Black,
            UI_BACKGROUND_DEPTH);

        var generalRenderables = new List<RenderTexture> { world, shopBackground, manageBackground };

        return new(
            grid,
            GenerateGenerateCreep(),
            GenerateGenerateWaves(),
            generateTurret,
            GenerateGenerateMoney(services),
            GenerateGenerateScore(services),
            GenerateGenerateHealth(services),
            GenerateGenerateLevel(services),
            turretButtons,
            turretCosts,
            startLevel,
            toggleMute,
            particleSystem,
            generalRenderables);
    }

    private static ParticleSystem GenerateParticleSystem(GameServiceContainer services)
    {
        return new(GenerateGenerateCreepPoints(services));
    }

    private static Func<Vector2, int, RenderString> GenerateGenerateCreepPoints(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return (position, points) =>
        {
            var text = $"{points}";
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because uY / uY = 1
            var scale = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position,
                Color.White,
                scale,
                SCORE_DEPTH,
                unitOrigin: new(0.5f));
        };
    }

    private static Func<TurretTextures, Turret> GenerateGenerateTurret(
        GameServiceContainer services,
        float turretRadius,
        ParticleSystem particleSystem)
    {
        const string upgrade = "Upgrade";
        const string sell = "Sell";

        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        var size = new Vector2(SMALL_FONT);
        var basePosition = new Vector2(WORLD_SIZE * 1.05f, WORLD_SIZE * 0.5f);

        var (uX, uY) = font.MeasureString(upgrade);
        var upgradeMeasurementScaledY = new Vector2(uX / uY, 1.0f); // 1.0f because uY / uY = 1
        var upgradeScale = upgradeMeasurementScaledY * size;
        var upgradeRenderString = new RenderString(
            FONT_NAME,
            upgrade,
            basePosition - upgradeScale * Vector2.UnitY,
            Color.White,
            upgradeScale,
            UI_STRING_DEPTH);

        var (sX, sY) = font.MeasureString(sell);
        var sellMeasurementScaledY = new Vector2(sX / sY, 1.0f); // 1.0f because sY / sY = 1
        var sellScale = sellMeasurementScaledY * size;
        var sellRenderString = new RenderString(
            FONT_NAME,
            sell,
            basePosition,
            Color.White,
            sellScale,
            UI_STRING_DEPTH);

        const int spawnLevel = 1;
        return textures =>
        {
            var (targetType, fireRateMillis) = textures.TurretType switch
            {
                TURRET_1 => (CreepType.Ground, 1000),
                TURRET_2 => (CreepType.Ground, 1500),
                TURRET_3 => (CreepType.Air, 1500),
                TURRET_4 => (CreepType.Ground | CreepType.Air, 500),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(textures.TurretType),
                    $"Only types of 1, 2, 3, 4 allowed. Got {textures.TurretType}")
            };

            var initialCost = textures.TurretType switch
            {
                TURRET_1 => TURRET_1_BUY,
                TURRET_2 => TURRET_2_BUY,
                TURRET_3 => TURRET_3_BUY,
                TURRET_4 => TURRET_4_BUY,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(textures.TurretType),
                    $"Only types of 1, 2, 3, 4 allowed. Got {textures.TurretType}")
            };

            var costs = new List<int> { initialCost, initialCost - 2, initialCost, 0 };

            var sells = new List<int>
            {
                0, costs[0] - 1, costs[0] + costs[1] - 2, costs[2] + costs[1] + costs[0] - 3
            };

            return new(
                turretRadius,
                upgradeRenderString,
                sellRenderString,
                targetType,
                costs,
                sells,
                spawnLevel,
                textures.Copy(TURRET_HEAD_DEPTH, TURRET_BASE_DEPTH, TURRET_RANGE_DEPTH),
                TimeSpan.FromMilliseconds(fireRateMillis),
                GenerateGenerateTurretProjectile(textures.Position, textures.TurretType, particleSystem),
                GenerateGenerateUpgrade(services),
                GenerateGenerateSell(services));
        };
    }

    private static Func<int, RenderString> GenerateGenerateUpgrade(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return cost =>
        {
            var text = $"Upgrade: {cost}";
            var position = new Vector2(WORLD_SIZE * 1.05f, WORLD_SIZE * 0.25f);
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because sY / sY = 1
            var scaledY = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position,
                Color.White,
                scaledY,
                UI_STRING_DEPTH);
        };
    }

    private static Func<int, RenderString> GenerateGenerateSell(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return cost =>
        {
            var text = $"Sell: {cost}";
            var position = new Vector2(WORLD_SIZE * 1.05f, WORLD_SIZE * 0.25f);
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because sY / sY = 1
            var scaledY = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position + scaledY * Vector2.UnitY,
                Color.White,
                scaledY,
                UI_STRING_DEPTH);
        };
    }

    private static Func<float, int, Creep, Projectile> GenerateGenerateTurretProjectile(
        Vector2 position,
        string turretType,
        ParticleSystem particleSystem)
    {
        const float radiusProjectile1 = WORLD_SIZE * 0.01f;
        const float radiusProjectile2 = WORLD_SIZE * 0.02f;
        const float radiusProjectile3 = WORLD_SIZE * 0.02f;
        const float radiusProjectile4 = WORLD_SIZE * 0.01f;
        const int baseDamage1 = 2;
        const int baseDamage2 = 4;
        const int baseDamage3 = 6;
        const int baseDamage4 = 0;
        const float speedProjectile1 = WORLD_SIZE * 1.0f / 1000;
        const float speedProjectile2 = WORLD_SIZE * 0.5f / 1000;
        const float speedProjectile3 = WORLD_SIZE * 0.5f / 1000;
        const float speedProjectile4 = WORLD_SIZE * 1.0f / 1000;
        var generateUpdateProjectile1 = GenerateGenerateBulletProjectileUpdate(speedProjectile1);
        var generateUpdateProjectile2 = GenerateGenerateBombProjectileUpdate(particleSystem, speedProjectile2);
        var generateUpdateProjectile3 = GenerateGenerateMissileProjectileUpdate(particleSystem, speedProjectile3);
        var generateUpdateProjectile4 = GenerateGenerateBulletProjectileUpdate(speedProjectile4);
        var (textureName, radius, damage, isBomb, explodes) = turretType switch
        {
            TURRET_1 => (BULLET, radiusProjectile1, baseDamage1, false, false),
            TURRET_2 => (BULLET, radiusProjectile2, baseDamage2, true, true),
            TURRET_3 => (BULLET, radiusProjectile3, baseDamage3, false, true),
            TURRET_4 => (BULLET, radiusProjectile4, baseDamage4, false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(turretType), turretType, "Only 1 ,2 ,3 ,4 allowed")
        };

        return (angle, level, creep) =>
        {
            var texture = new RenderTexture(
                textureName,
                position,
                new(2 * radius),
                new(0.5f),
                Color.Orange,
                PROJECTILE_DEPTH,
                angle);
            var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var update = turretType switch
            {
                TURRET_1 => generateUpdateProjectile1(texture, direction),
                TURRET_2 => generateUpdateProjectile2(texture, direction),
                TURRET_3 => generateUpdateProjectile3(texture, creep),
                TURRET_4 => generateUpdateProjectile4(texture, direction),
                _ => throw new ArgumentOutOfRangeException(nameof(turretType), turretType, "Only 1, 2, 3, 4 allowed")
            };
            return new(
                radius,
                damage + level * 2,
                isBomb,
                explodes,
                texture,
                update);
        };
    }

    // WTF even is this return type? I'm insane. Turret Defense was a mistake.
    private static Func
    <
        RenderTexture,
        Vector2,
        Action
        <
            GameTime,
            GameServiceContainer
        >
    >
    GenerateGenerateBulletProjectileUpdate(float speedPerMilli)
    {
        return (texture, direction) => (gameTime, services) =>
        {
            texture.Position += direction * speedPerMilli * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        };
    }

    private static Func
    <
        RenderTexture,
        Vector2,
        Action
        <
            GameTime,
            GameServiceContainer
        >
    >
    GenerateGenerateBombProjectileUpdate(ParticleSystem particleSystem, float speedPerMilli)
    {
        return (texture, direction) => (gameTime, services) =>
        {
            texture.Position += direction * speedPerMilli * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            particleSystem.BombTrail(gameTime, texture.Position);
        };
    }

    private static Func
    <
        RenderTexture,
        Creep,
        Action
        <
            GameTime,
            GameServiceContainer
        >
    >
    GenerateGenerateMissileProjectileUpdate(ParticleSystem particleSystem, float speedPerMilli)
    {
        const float rotationRate = MathHelper.TwoPi / 1000;
        // refactor turret and creep turn code to be static and accessible from anywhere
        return (texture, target) => (gameTime, services) =>
        {
            if (!target.IsDead && !target.IsFinished)
            {
                var rotationDiff = ComputeRotationDiff(
                    texture.Rotation,
                    target.Position,
                    texture.Position,
                    rotationRate,
                    gameTime);
                texture.Rotation = MathHelper.WrapAngle(texture.Rotation + rotationDiff);
            }

            particleSystem.MissileThrust(gameTime, texture.Position, texture.Rotation);
            var direction = new Vector2(
                MathF.Cos(texture.Rotation),
                MathF.Sin(texture.Rotation));
            texture.Position += direction * speedPerMilli * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        };
    }

    private static Func<string, Vector2, TurretTextures> GenerateGenerateTurretButton(Vector2 size, float turretRadius, Vector2 unitOrigin)
    {
        return (turretType, position) =>
        {
            var turretHead = new RenderTexture(
                $"{turretType}1",
                position,
                size,
                unitOrigin,
                Color.White,
                UI_TURRET_HEAD_DEPTH);

            var turretBase = new RenderTexture(
                TURRET_BASE,
                turretHead.Position,
                turretHead.Size,
                turretHead.UnitOrigin,
                turretHead.RenderColor,
                UI_TURRET_BASE_DEPTH);

            var turretRange = new RenderTexture(
                TURRET_RANGE,
                turretBase.Position,
                new(turretRadius * 2),
                turretBase.UnitOrigin,
                Color.Red,
                UI_TURRET_RANGE_DEPTH);

            return new(turretType, turretHead, turretBase, turretRange);
        };
    }

    private static Func<int, Vector2, RenderString> GenerateGenerateTurretCost(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return (cost, position) =>
        {
            var text = $"Cost: {cost}";
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position - scale * Vector2.UnitX,
                Color.White,
                scale,
                UI_STRING_DEPTH);
        };
    }

    private static Func<int> GenerateGenerateWaves()
    {
        var waves = 3;
        const int mean = 1;
        const int standardDeviation = 1;
        return () =>
        {
            var toReturn = waves;
            waves += (int)MathF.Abs(RandomNormal.Next(mean, standardDeviation));
            return toReturn;
        };
    }

    private static Func<int, RenderString> GenerateGenerateMoney(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return money =>
        {
            var text = $"Money: {money}";
            var position = new Vector2(-0.05f * WORLD_SIZE, 0.25f * WORLD_SIZE);
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position - scale * Vector2.UnitX - scale * Vector2.UnitY,
                Color.White,
                scale,
                UI_STRING_DEPTH);
        };
    }

    private static Func<int, RenderString> GenerateGenerateScore(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return money =>
        {
            var text = $"Score: {money}";
            var position = new Vector2(-0.05f * WORLD_SIZE, 0.25f * WORLD_SIZE);
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position - scale * Vector2.UnitX - scale * Vector2.UnitY * 2,
                Color.White,
                scale,
                UI_STRING_DEPTH);
        };
    }

    private static Func<int, RenderString> GenerateGenerateHealth(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return health =>
        {
            var text = $"Health: {health}";
            var position = new Vector2(-0.05f * WORLD_SIZE, 0.25f * WORLD_SIZE);
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position - scale * Vector2.UnitX - scale * Vector2.UnitY * 3,
                Color.White,
                scale,
                UI_STRING_DEPTH);
        };
    }

    private static Func<int, RenderString> GenerateGenerateLevel(GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        return level =>
        {
            var text = $"Level: {level}";
            var position = new Vector2(-0.05f * WORLD_SIZE, 0.25f * WORLD_SIZE);
            var (x, y) = font.MeasureString(text);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * SMALL_FONT;
            return new(
                FONT_NAME,
                text,
                position - scale * Vector2.UnitX - scale * Vector2.UnitY * 4,
                Color.White,
                scale,
                UI_STRING_DEPTH);
        };
    }

    private static Func<Grid, Creep> GenerateGenerateCreep()
    {
        var speedCreep1 = 0.07f * WORLD_SIZE / 1000;
        var speedCreep2 = 0.02f * WORLD_SIZE / 1000;
        var speedCreep3 = 0.04f * WORLD_SIZE / 1000;
        var increaseSpeed1 = speedCreep1 / 100;
        var increaseSpeed2 = speedCreep2 / 100;
        var increaseSpeed3 = speedCreep3 / 100;

        var healthCreep1 = 5.0f;
        var healthCreep2 = 2.0f;
        var healthCreep3 = 10.0f;
        var increaseHealth1 = healthCreep1 / 100;
        var increaseHealth2 = healthCreep2 / 100;
        var increaseHealth3 = healthCreep3 / 100;

        const int pointsCreep1 = 2;
        const int pointsCreep2 = 5;
        const int pointsCreep3 = 5;

        var rng = new Random();

        return grid =>
        {
            var creepGen = rng.Next(3);

            var entryGen = rng.Next(4);
            var exitGen = rng.Next(4);
            while (exitGen == entryGen)
            {
                exitGen = rng.Next(4);
            }

            var entry = entryGen switch
            {
                0 => Opening.Left,
                1 => Opening.Top,
                2 => Opening.Right,
                3 => Opening.Bottom,
                _ => throw new ArgumentOutOfRangeException(nameof(creepGen), "Only 0, 1, 2, 3")
            };

            var exit = exitGen switch
            {
                0 => Opening.Left,
                1 => Opening.Top,
                2 => Opening.Right,
                3 => Opening.Bottom,
                _ => throw new ArgumentOutOfRangeException(nameof(creepGen), "Only 0, 1, 2, 3")
            };

            var (creepTextureName, creepSpeed, creepType, health, points) = creepGen switch
            {
                0 => (CREEP_1, speedCreep1, CreepType.Ground, healthCreep1, pointsCreep1),
                1 => (CREEP_2, speedCreep2, CreepType.Air, healthCreep2, pointsCreep2),
                2 => (CREEP_3, speedCreep3, CreepType.Ground, healthCreep3, pointsCreep3),
                _ => throw new ArgumentOutOfRangeException(nameof(creepGen), "Only 0, 1, 2")
            };

            var (redBarDepth, greenBarDepth, creepDepth) = creepType switch
            {
                CreepType.Ground => (GROUND_CREEP_RED_DEPTH, GROUND_CREEP_GREEN_DEPTH, GROUND_CREEP_DEPTH),
                CreepType.Air => (AIR_CREEP_RED_DEPTH, AIR_CREEP_GREEN_DEPTH, AIR_CREEP_DEPTH),
                _ => throw new ArgumentOutOfRangeException(nameof(creepGen), "Only Air or Ground")
            };

            switch (creepGen)
            {
                case 0:
                    speedCreep1 += increaseSpeed1;
                    healthCreep1 += increaseHealth1;
                    break;
                case 1:
                    speedCreep2 += increaseSpeed2;
                    healthCreep2 += increaseHealth2;
                    break;
                case 2:
                    speedCreep3 += increaseSpeed3;
                    healthCreep3 += increaseHealth3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(creepGen), "Only 0, 1, 2");
            }

            const float inverseCreepScalar = (float)CREEP_GRID_SIZE / GRID_NUM;
            const float creepSize = WORLD_SIZE * inverseCreepScalar;

            var redBar = new RenderTexture(
                BLANK,
                Vector2.Zero,
                new(creepSize, creepSize * 0.1f),
                new(0.5f),
                Color.Red,
                redBarDepth);
            var greenBar = new RenderTexture(
                BLANK,
                Vector2.Zero,
                new(creepSize, creepSize * 0.1f),
                new(0.5f),
                Color.Green,
                greenBarDepth);


            var position = Grid.WorldPositionForOpening(entry, new(0.5f));

            var texture = new RenderTexture(
                creepTextureName,
                position,
                new(creepSize),
                new(0.5f),
                Color.White,
                creepDepth,
                entry switch
                {
                    Opening.Left => 0.0f,
                    Opening.Top => MathHelper.PiOver2,
                    Opening.Right => MathHelper.Pi,
                    Opening.Bottom => -MathHelper.PiOver2,
                    _ => throw new ArgumentOutOfRangeException()
                });

            var creepTextures = new CreepTextures(redBar, greenBar, texture);

            var path = grid.PathForCreep(position, exit, creepType)?.ToList();

            return new(
                creepType,
                points,
                grid,
                path,
                exit,
                creepTextures,
                creepSpeed,
                (int)health);
        };
    }

    private static MenuView GeneratePauseMenuView(GameServiceContainer services)
    {
        const string title = "Pause";

        const string resume = "Resume Game";
        const string exit = "Exit to Main Menu";

        return GenerateMenuView(
            services,
            title,
            new()
            {
                resume,
                exit
            },
            option => option switch
            {
                resume => GameState.PreviousState,
                exit => GameState.MainMenu,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(option),
                    option,
                    GenerateNextStateExceptionMessage(title))
            },
            _ => true);
    }

    private static TextView GenerateHighScoresMenuView(GameServiceContainer services)
    {
        var scores = services.GetService<IScoreManager>().GetScores();
        var messages = scores.Select(s => s.ToString()).ToList();
        return GenerateTextView(services, "High Scores", messages);
    }

    private static MenuView GenerateKeybindsMenuView(GameServiceContainer services)
    {
        var keyMap = services.GetService<IInputManager>().GetKeyMap();
        const string title = "Rebind Keys";

        var upgrade = $"{UPGRADE}: {keyMap[UPGRADE].Key}";
        var sell = $"{SELL}: {keyMap[SELL].Key}";
        var startLevel = $"{START_LEVEL}: {keyMap[START_LEVEL].Key}";

        const string reset = "Reset keybinds";
        const string back = "Back to Main Menu";

        return GenerateMenuView(
            services,
            title,
            new()
            {
                upgrade,
                sell,
                startLevel,
                reset,
                back
            },
            option => option switch
            {
                _ when option == upgrade => GameState.RebindUpgrade,
                _ when option == sell => GameState.RebindSell,
                _ when option == startLevel => GameState.RebindStartLevel,
                reset => GameState.ResetKeyBinds,
                back => GameState.MainMenu,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(option),
                    option,
                    GenerateNextStateExceptionMessage(title))
            },
            option => true);
    }

    private static RebindKeyView GenerateRebindUpgradeView(GameServiceContainer services)
    {
        return GenerateRebindKeyView(UPGRADE, services);
    }

    private static RebindKeyView GenerateRebindSellView(GameServiceContainer services)
    {
        return GenerateRebindKeyView(SELL, services);
    }

    private static RebindKeyView GenerateRebindStartLevelView(GameServiceContainer services)
    {
        return GenerateRebindKeyView(START_LEVEL, services);
    }

    private static TextView GenerateCreditsView(GameServiceContainer services)
    {
        return GenerateTextView(
            services,
            "Credits",
            new()
            {
                "Author: Makayden Lofthouse",
                "Music: DST of opengameart.org",
                "Grass texture:",
                "    LuminousDragonGames of opengameart.org"
            });
    }

    private static ExitView GenerateExitView()
    {
        return new();
    }

    private static string GenerateNextStateExceptionMessage(string title)
    {
        return $"Invalid nextState option for {title}";
    }

    private static string GenerateIsFinishedExceptionMessage(string title)
    {
        return $"Invalid isFinished option for {title}";
    }

    private static MenuView GenerateMenuView(
        GameServiceContainer services,
        string title,
        List<string> options,
        Func<string, GameState> nextState,
        Func<string, bool> isFinished)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        var offsetVector = new Vector2(WORLD_SIZE * 0.06f);

        var (tX, tY) = font.MeasureString(title);
        var titleMeasurementScaledY = new Vector2(tX / tY, 1.0f); // 1.0f because ty / ty = 1
        var titleScale = titleMeasurementScaledY * LARGE_FONT;
        var menuTitle = new RenderString(
            FONT_NAME,
            title,
            offsetVector,
            Color.White,
            titleScale,
            MENU_DEPTH);

        var selectorStartingPosition = offsetVector + titleScale * Vector2.UnitY;
        var size = new Vector2(MEDIUM_FONT);
        var selectorTexture = new RenderTexture(
            SELECTOR,
            selectorStartingPosition,
            size,
            Vector2.Zero,
            Color.White,
            MENU_DEPTH);
        var selector = new MenuSelector(selectorTexture, selectorStartingPosition);

        var stringStartingPosition = selectorStartingPosition + size * Vector2.UnitX;
        var renderStrings = options.Select((option, index) =>
        {
            var position = stringStartingPosition + index * size * Vector2.UnitY;
            var (x, y) = font.MeasureString(option);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * size;
            return new RenderString(
                FONT_NAME,
                option,
                position,
                Color.White,
                scale,
                MENU_DEPTH);
        });

        return new(menuTitle, renderStrings.ToList(), nextState, isFinished, selector);
    }

    private static TextView GenerateTextView(GameServiceContainer services, string title, List<string> messages)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        var offsetVector = new Vector2(WORLD_SIZE * 0.06f);

        var (tX, tY) = font.MeasureString(title);
        var titleMeasurementScaledY = new Vector2(tX / tY, 1.0f); // 1.0f because ty / ty = 1
        var titleScale = titleMeasurementScaledY * LARGE_FONT;
        var viewTitle = new RenderString(
            FONT_NAME,
            title,
            offsetVector,
            Color.White,
            titleScale,
            MENU_DEPTH);

        var stringStartingPosition = offsetVector + titleScale * Vector2.UnitY;
        var size = new Vector2(SMALL_FONT);
        var renderStrings = messages.Select((option, index) =>
        {
            var position = stringStartingPosition + index * size * Vector2.UnitY;
            var (x, y) = font.MeasureString(option);
            var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
            var scale = measurementScaledY * size;
            return new RenderString(
                FONT_NAME,
                option,
                position,
                Color.White,
                scale,
                MENU_DEPTH);
        });

        return new(viewTitle, renderStrings.ToList());
    }

    private static RebindKeyView GenerateRebindKeyView(string inputToRebind, GameServiceContainer services)
    {
        var font = services.GetService<IResourceManager>().GetFont(FONT_NAME);
        var text = $"Press a key to bind to \"{inputToRebind}\"";
        var (x, y) = font.MeasureString(text);
        var measurementScaledY = new Vector2(x / y, 1.0f); // 1.0f because y / y = 1
        var scale = measurementScaledY * SMALL_FONT;
        var renderString = new RenderString(
            FONT_NAME,
            text,
            new Vector2(WORLD_SIZE * 0.1f),
            Color.White,
            scale,
            MENU_DEPTH);
        return new(inputToRebind, renderString);
    }
}
