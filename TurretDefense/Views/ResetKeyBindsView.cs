using Microsoft.Xna.Framework;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views.Interfaces;

namespace TurretDefense.Views;

public class ResetKeyBindsView : IView
{
    public GameState NextState { get; } = GameState.PreviousState;

    public GameState PreviousState { get; set; } = GameState.Keybinds;

    public bool IsFinished { get; private set; } = false;

    public bool ShouldTransition { get; set; } = false;

    public void Update(GameTime gameTime, GameServiceContainer services)
    {
        services.GetService<IInputManager>().ResetToDefaults();
        ShouldTransition = true;
        IsFinished = true;
    }

    public void Render(IRenderManager renderManager) { }
}
