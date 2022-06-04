using TurretDefense.Interfaces;

namespace TurretDefense.Views.Interfaces;

/*
 * NOTE TO FUTURE SELF: maybe make a way to pass information between views,
 * like maybe a arguments object or dynamic or something
 */
public interface IView : IUpdatable, IRenderable
{
    GameState NextState { get; }

    GameState PreviousState { get; set; }

    bool IsFinished { get; }

    bool ShouldTransition { get; set; }
}
