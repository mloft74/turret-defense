using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using TurretDefense.Components;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Views;

public class MenuView : IView
{
    public GameState NextState => _shouldGoBack ?
        PreviousState :
        _nextState(_menuOptions[_selector.Selection].Text);

    public GameState PreviousState { get; set; }

    public bool IsFinished => _shouldGoBack || _isFinished(_menuOptions[_selector.Selection].Text);

    public bool ShouldTransition { get; set; } = false;

    private readonly RenderString _menuTitle;
    private readonly List<RenderString> _menuOptions;
    private readonly Func<string, GameState> _nextState;
    private readonly Func<string, bool> _isFinished;
    private readonly MenuSelector _selector;
    private bool _shouldGoBack = false;

    public MenuView(
        RenderString menuTitle,
        List<RenderString> menuOptions,
        Func<string, GameState> nextState,
        Func<string, bool> isFinished,
        MenuSelector selector)
    {
        _menuTitle = menuTitle;
        _menuOptions = menuOptions;
        _nextState = nextState;
        _isFinished = isFinished;
        _selector = selector;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var inputManager = services.GetService<IInputManager>();
        var handleInput = GenerateHandleInput(services.GetService<IResourceManager>().GetSound(MENU_BLIP));

        handleInput(inputManager.GetInput(MOUSE_LEFT), () =>
        {
            const int sentinel = -1;
            var index = sentinel;
            for (var optionIndex = 0; optionIndex < _menuOptions.Count; ++optionIndex)
            {
                if (!inputManager.IsMouseIntersecting(_menuOptions[optionIndex])) continue;
                index = optionIndex;
                break;
            }
            if (index == sentinel) return false;

            ShouldTransition = true;
            _selector.Selection = index;
            return true;
        });
        handleInput(inputManager.GetInput(MENU_UP), () => ModifyMenuState(false));
        handleInput(inputManager.GetInput(MENU_DOWN), () => ModifyMenuState(true));
        handleInput(inputManager.GetInput(ENTER), () => ShouldTransition = true);
        handleInput( inputManager.GetInput(ESCAPE), () =>
        {
            ShouldTransition = true;
            _shouldGoBack = true;
            return true;
        });
        _selector.Update(gameTime, services);
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderString(_menuTitle);
        renderManager.QueueRenderStrings(_menuOptions);
        _selector.Render(renderManager);
    }

    private bool ModifyMenuState(bool isPositive)
    {
        var direction = isPositive ? 1 : -1;
        var selectionCount = _menuOptions.Count;
        var nextSelection = _selector.Selection + direction + selectionCount;
        _selector.Selection = nextSelection % selectionCount;
        return true;
    }

    private static Action<InputState, Func<bool>> GenerateHandleInput(SoundEffect sound)
    {
        return (state, action) =>
        {
            var (wasPressed, isPressed) = state;
            if (wasPressed || !isPressed) return;
            if(!action()) return;
            sound.Play();
        };
    }
}
