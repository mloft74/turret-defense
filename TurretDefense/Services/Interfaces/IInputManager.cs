using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TurretDefense.Models;

namespace TurretDefense.Services.Interfaces;

public interface IInputManager
{
    Vector2 MouseWorldPosition { get; }

    void RebindKey(string name, Keys key);

    InputState GetInput(string name);

    bool IsMouseIntersecting(RenderTexture renderTexture);

    bool IsMouseIntersecting(RenderString renderString);

    void ProcessInput(GameServiceContainer services);

    void ResetToDefaults();

    Dictionary<string, KeyInfo> GetKeyMap();
}
