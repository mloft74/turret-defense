using Microsoft.Xna.Framework;
using TurretDefense.Services.Interfaces;
using TurretDefense.Views.Interfaces;

namespace TurretDefense.Views;

public class ExitView : IView
{
    public GameState NextState { get; } = GameState.Exit;

    public GameState PreviousState { get; set; }

    public bool IsFinished { get; } = true;

    public bool ShouldTransition { get; set; } = true;

    public void Update(GameTime gameTime, GameServiceContainer services) { }

    public void Render(IRenderManager renderManager) { }
}
