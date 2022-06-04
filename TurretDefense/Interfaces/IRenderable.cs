using TurretDefense.Services.Interfaces;

namespace TurretDefense.Interfaces;

public interface IRenderable
{
    void Render(IRenderManager renderManager);
}
