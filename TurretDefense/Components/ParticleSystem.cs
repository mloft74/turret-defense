using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TurretDefense.Interfaces;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Components;

public class ParticleSystem : IUpdatable, IRenderable
{
    private readonly Func<Vector2, int, RenderString> _generateText;

    private const float SMOKE_FIRE_SPLIT = 0.9f;

    private readonly List<Particle> _particles = new();
    private readonly List<CreepPoint> _creepPoints = new();
    private readonly Random _rng = new();

    public ParticleSystem(Func<Vector2, int, RenderString> generateText)
    {
        _generateText = generateText;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var particlesToRemove = new List<Particle>();
        foreach (var particle in _particles)
        {
            particle.Update(gameTime, services);
            if (particle.TimeAlive > particle.Timeout)
            {
                particlesToRemove.Add(particle);
            }
        }

        foreach (var particle in particlesToRemove)
        {
            _particles.Remove(particle);
        }

        var creepPointsToRemove = new List<CreepPoint>();
        foreach (var creepPoint in _creepPoints)
        {
            creepPoint.Update(gameTime, services);
            if (creepPoint.TimeAlive > creepPoint.Timeout)
            {
                creepPointsToRemove.Add(creepPoint);
            }
        }

        foreach (var creepPoint in creepPointsToRemove)
        {
            _creepPoints.Remove(creepPoint);
        }
    }

    public void Render(IRenderManager renderManager)
    {
        foreach (var particle in _particles)
        {
            particle.Render(renderManager);
        }

        foreach (var creepPoint in _creepPoints)
        {
            creepPoint.Render(renderManager);
        }
    }

    public void MissileThrust(GameTime gameTime, Vector2 position, float angle)
    {
        const float angleMagnitude = MathHelper.PiOver4 / 8;
        const int particlesPerMilli = 1;
        const float speedMagnitude = WORLD_SIZE * 0.2f / 1000;
        const int timeoutMillis = 100;
        var numberOfParticles = particlesPerMilli * (int)gameTime.ElapsedGameTime.TotalMilliseconds;
        for (var i = 0; i < numberOfParticles; ++i)
        {
            var dAngle = RandomNormal.Next(0.0f, angleMagnitude) + MathHelper.Pi + angle;
            var speed = MathF.Abs(RandomNormal.Next(0.0f, speedMagnitude)) + speedMagnitude;
            var velocityX = MathF.Cos(dAngle) * speed;
            var velocityY = MathF.Sin(dAngle) * speed;
            var velocity = new Vector2(velocityX, velocityY);

            var timeout = MathF.Abs(RandomNormal.Next(0.0f, timeoutMillis)) + timeoutMillis;

            var num = (float) _rng.NextDouble();
            var name = num < SMOKE_FIRE_SPLIT ? SMOKE : FIRE;
            var renderTexture = new RenderTexture(
                name,
                position,
                new Vector2(WORLD_SIZE * 0.01f),
                new Vector2(0.5f),
                Color.White,
                PARTICLE_DEPTH);
            var particle = new Particle(
                renderTexture,
                velocity,
                TimeSpan.FromMilliseconds(timeout));
            _particles.Add(particle);
        }
    }

    public void CreepExplosion(Vector2 position)
    {
        const float speedMagnitude = WORLD_SIZE * 0.1f / 1000;
        const int timeoutMillis = 100;
        for (var i = 0; i < 1000; ++i)
        {
            var angle = (float) _rng.NextDouble() * MathHelper.TwoPi;
            var speed = MathF.Abs(RandomNormal.Next(0.0f, speedMagnitude)) + speedMagnitude;
            var velocityX = MathF.Cos(angle) * speed;
            var velocityY = MathF.Sin(angle) * speed;
            var velocity = new Vector2(velocityX, velocityY);

            var timeout = MathF.Abs(RandomNormal.Next(0.0f, timeoutMillis)) + timeoutMillis;

            var num = (float) _rng.NextDouble();
            var name = num < SMOKE_FIRE_SPLIT ? SMOKE : FIRE;
            var renderTexture = new RenderTexture(
                name,
                position,
                new Vector2(WORLD_SIZE * 0.01f),
                new Vector2(0.5f),
                new Color(75, 0, 0),
                PARTICLE_DEPTH);
            var particle = new Particle(
                renderTexture,
                velocity,
                TimeSpan.FromMilliseconds(timeout));
            _particles.Add(particle);
        }
    }

    public void SellTurret(Vector2 position)
    {
        const float speedMagnitude = WORLD_SIZE * 0.1f / 1000;
        const int timeoutMillis = 100;
        for (var i = 0; i < 1000; ++i)
        {
            var angle = (float) _rng.NextDouble() * MathHelper.TwoPi;
            var speed = MathF.Abs(RandomNormal.Next(0.0f, speedMagnitude)) + speedMagnitude;
            var velocityX = MathF.Cos(angle) * speed;
            var velocityY = MathF.Sin(angle) * speed;
            var velocity = new Vector2(velocityX, velocityY);

            var timeout = MathF.Abs(RandomNormal.Next(0.0f, timeoutMillis)) + timeoutMillis;

            var num = (float) _rng.NextDouble();
            var name = num < SMOKE_FIRE_SPLIT ? SMOKE : FIRE;
            var renderTexture = new RenderTexture(
                name,
                position,
                new Vector2(WORLD_SIZE * 0.01f),
                new Vector2(0.5f),
                Color.White,
                PARTICLE_DEPTH);
            var particle = new Particle(
                renderTexture,
                velocity,
                TimeSpan.FromMilliseconds(timeout));
            _particles.Add(particle);
        }
    }

    public void BombTrail(GameTime gameTime, Vector2 position)
    {
        const int particlesPerMilli = 1;
        const float speedMagnitude = WORLD_SIZE * 0.025f / 1000;
        const int timeoutMillis = 200;
        var particleNum = particlesPerMilli * (int)gameTime.ElapsedGameTime.TotalMilliseconds;
        for (var i = 0; i < particleNum; ++i)
        {
            var angle = (float)_rng.NextDouble() * MathHelper.TwoPi;
            var speed = MathF.Abs(RandomNormal.Next(0.0f, speedMagnitude)) + speedMagnitude;
            var velocityX = MathF.Cos(angle) * speed;
            var velocityY = MathF.Sin(angle) * speed;
            var velocity = new Vector2(velocityX, velocityY);

            var timeout = MathF.Abs(RandomNormal.Next(0.0f, timeoutMillis)) + timeoutMillis;

            var renderTexture = new RenderTexture(
                SMOKE,
                position,
                new Vector2(WORLD_SIZE * 0.01f),
                new Vector2(0.5f),
                Color.White,
                PARTICLE_DEPTH);
            var particle = new Particle(
                renderTexture,
                velocity,
                TimeSpan.FromMilliseconds(timeout));
            _particles.Add(particle);
        }
    }

    public void BombExplosion(Vector2 position)
    {
        const float speedMagnitude = WORLD_SIZE * 0.1f / 1000;
        const int timeoutMillis = 100;
        for (var i = 0; i < 1000; ++i)
        {
            var angle = (float) _rng.NextDouble() * MathHelper.TwoPi;
            var speed = MathF.Abs(RandomNormal.Next(0.0f, speedMagnitude)) + speedMagnitude;
            var velocityX = MathF.Cos(angle) * speed;
            var velocityY = MathF.Sin(angle) * speed;
            var velocity = new Vector2(velocityX, velocityY);

            var timeout = MathF.Abs(RandomNormal.Next(0.0f, timeoutMillis)) + timeoutMillis;

            var num = (float) _rng.NextDouble();
            var name = num < SMOKE_FIRE_SPLIT ? SMOKE : FIRE;
            var renderTexture = new RenderTexture(
                name,
                position,
                new Vector2(WORLD_SIZE * 0.01f),
                new Vector2(0.5f),
                Color.White,
                PARTICLE_DEPTH);
            var particle = new Particle(
                renderTexture,
                velocity,
                TimeSpan.FromMilliseconds(timeout));
            _particles.Add(particle);
        }
    }

    public void CreepPoints(Vector2 position, int points)
    {
        var text = _generateText(position, points);
        _creepPoints.Add(new CreepPoint(text));
    }
}
