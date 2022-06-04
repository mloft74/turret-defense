using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;
using static TurretDefense.Constants;

namespace TurretDefense.Services;

public class InputManager : IInputManager
{
    public Vector2 MouseWorldPosition { get; private set; } = Vector2.Zero;

    private readonly Dictionary<string, KeyInfo> _defaults = new()
    {
        [MENU_UP] = new KeyInfo(false, Keys.Up),
        [MENU_DOWN] = new KeyInfo(false, Keys.Down),
        [ENTER] = new KeyInfo(false, Keys.Enter),
        [ESCAPE] = new KeyInfo(false, Keys.Escape),
        [UPGRADE] = new KeyInfo(true, Keys.U),
        [SELL] = new KeyInfo(true, Keys.S),
        [START_LEVEL] = new KeyInfo(true, Keys.G)
    };

    private readonly Dictionary<string, KeyInfo> _keyMap = new();
    private readonly Dictionary<string, InputState> _inputStates = new()
    {
        [MOUSE_LEFT] = new(false, false),
        [MOUSE_RIGHT] = new(false, false)
    };

    public InputManager(Dictionary<string, KeyInfo> keyMap)
    {
        if (keyMap.Count == 0)
        {
            ResetToDefaults();
        }
        else
        {
            _keyMap = keyMap;
        }

        foreach (var name in _keyMap.Keys)
        {
            _inputStates.Add(name, new(false, false));
        }
    }

    public void RebindKey(string name, Keys key)
    {
        _keyMap[name] = new(true, key);

        var temp = new Dictionary<string, KeyInfo>(_keyMap);
        foreach (var (dictName, keyInfo) in temp)
        {
            if (dictName != name && keyInfo.Rebindable && keyInfo.Key == key)
            {
                _keyMap[dictName] = new(true, null);
            }
        }
    }

    public InputState GetInput(string name)
    {
        return _inputStates[name];
    }

    public bool IsMouseIntersecting(RenderTexture renderTexture)
    {
        return IsMouseIntersecting(renderTexture.Position, renderTexture.Size, renderTexture.UnitOrigin);
    }

    public bool IsMouseIntersecting(RenderString renderString)
    {
        return IsMouseIntersecting(renderString.Position, renderString.Scale, renderString.UnitOrigin);
    }

    private bool IsMouseIntersecting(Vector2 position, Vector2 size, Vector2 unitOrigin)
    {
        var (mX, mY) = MouseWorldPosition;
        var topLeft = position - size * unitOrigin;
        var (aX, aY) = topLeft;
        var (bX, bY) = topLeft + size;

        return mX > aX &&
            mX < bX &&
            mY > aY &&
            mY < bY;
    }

    public void ProcessInput(GameServiceContainer services)
    {
        var kbState = Keyboard.GetState();
        foreach (var (name, keyInfo) in _keyMap)
        {
            var wasPressed = _inputStates[name].IsPressed;
            var isPressed = keyInfo.Key != null && kbState.IsKeyDown(keyInfo.Key.Value);
            _inputStates[name] = new(wasPressed, isPressed);
        }

        var mouseState = Mouse.GetState();

        var leftWasPressed = _inputStates[MOUSE_LEFT].IsPressed;
        var leftIsPressed = mouseState.LeftButton.HasFlag(ButtonState.Pressed);
        _inputStates[MOUSE_LEFT] = new(leftWasPressed, leftIsPressed);

        var rightWasPressed = _inputStates[MOUSE_RIGHT].IsPressed;
        var rightIsPressed = mouseState.RightButton.HasFlag(ButtonState.Pressed);
        _inputStates[MOUSE_RIGHT] = new(rightWasPressed, rightIsPressed);

        var converter = services.GetService<IWorldRenderConverter>();
        var mousePoint = mouseState.Position;
        MouseWorldPosition = converter.ConvertPositionToWorld(mousePoint.ToVector2());
    }

    public void ResetToDefaults()
    {
        _keyMap.Clear();
        foreach (var (name, keyInfo) in _defaults)
        {
            _keyMap.Add(name, keyInfo);
        }
    }

    public Dictionary<string, KeyInfo> GetKeyMap()
    {
        return new(_keyMap);
    }
}
