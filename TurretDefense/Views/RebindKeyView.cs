using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Views;

public class RebindKeyView : IView
{
    public GameState NextState { get; } = GameState.PreviousState;

    public GameState PreviousState { get; set; } = GameState.Keybinds;

    public bool IsFinished { get; private set; } = false;

    public bool ShouldTransition { get; set; } = false;

    private readonly string _inputToRebind;
    private readonly RenderString _renderString;
    private bool _readyToRebind = false;

    public RebindKeyView(string inputToRebind, RenderString renderString)
    {
        _inputToRebind = inputToRebind;
        _renderString = renderString;
    }

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        var inputManager = services.GetService<IInputManager>();
        var menuBlip = services.GetService<IResourceManager>().GetSound(MENU_BLIP);
        var (wasPressed, isPressed) = inputManager.GetInput(ESCAPE);
        if (!wasPressed && isPressed)
        {
            menuBlip.Play();
            ShouldTransition = true;
            IsFinished = true;
            return;
        }

        var kbState = Keyboard.GetState();
        switch (_readyToRebind)
        {
            case true when kbState.GetPressedKeyCount() > 0:
            {
                menuBlip.Play();
                var key = kbState.GetPressedKeys().First();
                inputManager.RebindKey(_inputToRebind, key);
                ShouldTransition = true;
                IsFinished = true;
                break;
            }
            case false when kbState.GetPressedKeyCount() == 0:
                _readyToRebind = true;
                break;
        }
    }

    public void Render(IRenderManager renderManager)
    {
        renderManager.QueueRenderString(_renderString);
    }
}
