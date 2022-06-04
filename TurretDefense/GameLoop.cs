using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TurretDefense.Services;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views;
using TurretDefense.Views.Interfaces;
using static TurretDefense.Constants;
using static TurretDefense.Factory;
using static TurretDefense.Persistence;

namespace TurretDefense;

public class GameLoop : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = default!;

    private GameState _gameState;
    private Dictionary<GameState, IView> _views = default!;

    public GameLoop()
    {
        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        _gameState = GameState.Initialization;

        var scores = LoadScores();
        var keyMap = LoadKeyMap();

        Services.AddService<IResourceManager>(new ResourceManager());
        Services.AddService<IRenderManager>(new RenderManager());
        Services.AddService<IWorldRenderConverter>( new WorldRenderConverter(
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight));
        Services.AddService<IScoreManager>(new ScoreManager(scores));
        Services.AddService<IInputManager>(new InputManager(keyMap));

        _views = new() { [_gameState] = GenerateView(_gameState, Services) };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new(GraphicsDevice);

        var blankSquare = Content.Load<Texture2D>("Images/blankSquare");

        var mediumDiamond = Content.Load<Texture2D>("Images/mediumDiamond");

        var grassWalls = Content.Load<Texture2D>("Images/grassWalls");

        var fire = Content.Load<Texture2D>("Images/fire");
        var smoke = Content.Load<Texture2D>("Images/smoke");

        var creep1 = Content.Load<Texture2D>("Images/creepFirst");
        var creep2 = Content.Load<Texture2D>("Images/creepSecond");
        var creep3 = Content.Load<Texture2D>("Images/creepThird");

        var blurCircle = Content.Load<Texture2D>("Images/blurCircle");
        var turretBase = Content.Load<Texture2D>("Images/turretBase");
        var turret11 = Content.Load<Texture2D>("Images/turretFirstLevel1");
        var turret12 = Content.Load<Texture2D>("Images/turretFirstLevel2");
        var turret13 = Content.Load<Texture2D>("Images/turretFirstLevel3");
        var turret21 = Content.Load<Texture2D>("Images/turretSecondLevel1");
        var turret22 = Content.Load<Texture2D>("Images/turretSecondLevel2");
        var turret23 = Content.Load<Texture2D>("Images/turretSecondLevel3");
        var turret31 = Content.Load<Texture2D>("Images/turretThirdLevel1");
        var turret32 = Content.Load<Texture2D>("Images/turretThirdLevel2");
        var turret33 = Content.Load<Texture2D>("Images/turretThirdLevel3");
        var turret41 = Content.Load<Texture2D>("Images/turretFourthLevel1");
        var turret42 = Content.Load<Texture2D>("Images/turretFourthLevel2");
        var turret43 = Content.Load<Texture2D>("Images/turretFourthLevel3");

        var bullet = Content.Load<Texture2D>("Images/bullet");

        var creepDead = Content.Load<SoundEffect>("Audio/creepDead");
        var creepHit = Content.Load<SoundEffect>("Audio/creepHit");
        var shoot = Content.Load<SoundEffect>("Audio/shoot");
        var explosion = Content.Load<SoundEffect>("Audio/explosion");
        var menuBlip = Content.Load<SoundEffect>("Audio/menuBlip");
        var turretPlace = Content.Load<SoundEffect>("Audio/turretPlace");

        var song = Content.Load<Song>("Audio/DST-TurretDefenseTheme");

        var roboto = Content.Load<SpriteFont>("Fonts/roboto");

        var resourceManager = Services.GetService<IResourceManager>();

        resourceManager.RegisterTexture(BLANK, blankSquare, new[] { int.MaxValue });

        resourceManager.RegisterTexture(SELECTOR, mediumDiamond, new[] { int.MaxValue });

        resourceManager.RegisterTexture(BACKGROUND, grassWalls, new[] { int.MaxValue });

        resourceManager.RegisterTexture(FIRE, fire, new[] { int.MaxValue });
        resourceManager.RegisterTexture(SMOKE, smoke, new[] { int.MaxValue });

        resourceManager.RegisterTexture(CREEP_1, creep1, new[] { int.MaxValue });
        resourceManager.RegisterTexture(CREEP_2, creep2, new[] { int.MaxValue });
        resourceManager.RegisterTexture(CREEP_3, creep3, new[] { int.MaxValue });

        resourceManager.RegisterTexture(TURRET_RANGE, blurCircle, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_BASE, turretBase, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_1_1, turret11, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_1_2, turret12, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_1_3, turret13, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_2_1, turret21, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_2_2, turret22, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_2_3, turret23, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_3_1, turret31, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_3_2, turret32, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_3_3, turret33, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_4_1, turret41, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_4_2, turret42, new[] { int.MaxValue });
        resourceManager.RegisterTexture(TURRET_4_3, turret43, new[] { int.MaxValue });

        resourceManager.RegisterTexture(BULLET, bullet, new[] { int.MaxValue });

        resourceManager.RegisterSound(CREEP_DEAD, creepDead);
        resourceManager.RegisterSound(CREEP_HIT, creepHit);
        resourceManager.RegisterSound(SHOOT, shoot);
        resourceManager.RegisterSound(EXPLOSION, explosion);
        resourceManager.RegisterSound(MENU_BLIP, menuBlip);
        resourceManager.RegisterSound(TURRET_PLACE, turretPlace);

        resourceManager.RegisterSong(SONG, song);

        resourceManager.RegisterFont(FONT_NAME, roboto);
    }

    protected override void Update(GameTime gameTime)
    {
        var inputManager = Services.GetService<IInputManager>();
        inputManager.ProcessInput(Services);

        if (_gameState == GameState.Exit)
        {
            SaveScores(Services.GetService<IScoreManager>().GetScores());
            SaveKeyMap(inputManager.GetKeyMap());
            Exit();
        }

        if (_views[_gameState].ShouldTransition)
        {
            TransitionState();
        }
        _views[_gameState].Update(gameTime, Services);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var renderManager = Services.GetService<IRenderManager>();

        _views[_gameState].Render(renderManager);

        renderManager.Draw(GraphicsDevice, Services, _spriteBatch, gameTime);

        base.Draw(gameTime);
    }

    private void TransitionState()
    {
        var view = _views[_gameState];
        view.ShouldTransition = false;
        var nextState = view.NextState;
        var previousState = nextState == GameState.MainMenu ?
            GameState.MainMenu :
            _gameState; // saves previous state, except for main menu, which has no previous state

        if (_gameState == GameState.Pause && view.IsFinished && nextState == GameState.MainMenu)
        {
            var gameplayView = (GameplayView)_views[GameState.Gameplay];
            gameplayView.IsFinished = true;
        }

        _gameState = nextState == GameState.PreviousState ? view.PreviousState : nextState;
        if (!_views.ContainsKey(_gameState))
        {
            _views.Add(_gameState, GenerateView(_gameState, Services));
        }
        else if (_views[_gameState].IsFinished)
        {
            _views[_gameState] = GenerateView(_gameState, Services);
        }

        if (nextState == GameState.PreviousState) return;
        _views[_gameState].PreviousState = view is GameplayView { IsFinished: true } ? GameState.MainMenu : previousState;
    }
}
