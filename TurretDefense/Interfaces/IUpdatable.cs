using Microsoft.Xna.Framework;

namespace TurretDefense.Interfaces;

public interface IUpdatable
{
    void Update(GameTime gameTime, GameServiceContainer services);
}
