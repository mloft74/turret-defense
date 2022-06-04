using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Views;

public class TextView : IView
{
    public GameState NextState { get; } = GameState.PreviousState;

    public GameState PreviousState { get; set; }

    public bool IsFinished { get; private set; }

    public bool ShouldTransition { get; set; }

    private readonly RenderString _viewTitle;
    private readonly List<RenderString> _renderStrings;

    public TextView(RenderString viewTitle, List<RenderString> renderStrings)
    {
        _viewTitle = viewTitle;
        _renderStrings = renderStrings;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var inputManager = services.GetService<IInputManager>();
        var menuBlip = services.GetService<IResourceManager>().GetSound(MENU_BLIP);
        HandleInput(inputManager.GetInput(ENTER), menuBlip, Terminate);
        HandleInput(inputManager.GetInput(ESCAPE), menuBlip, Terminate);
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderString(_viewTitle);
        renderManager.QueueRenderStrings(_renderStrings);
    }

    private static void HandleInput(InputState state, SoundEffect sound, Action action)
    {
        var (wasPressed, isPressed) = state;
        if (wasPressed || !isPressed) return;
        sound.Play();
        action();
    }

    private void Terminate()
    {
        ShouldTransition = true;
        IsFinished = true;
    }
}
