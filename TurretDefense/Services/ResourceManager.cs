using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TurretDefense.Models;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Services;

public class ResourceManager : IResourceManager
{
    private readonly Dictionary<string, Texture2dInfo> _textureInfos = new();
    private readonly Dictionary<string, SpriteFont> _fonts = new();
    private readonly Dictionary<string, SoundEffect> _sounds = new();
    private readonly Dictionary<string, Song> _songs = new();

    public void RegisterTexture(string name, Texture2D texture, IEnumerable<int> timings)
    {
        var textureInfo = new Texture2dInfo(texture, timings);
        _textureInfos.Add(name, textureInfo);
    }

    public void RegisterFont(string name, SpriteFont font)
    {
        _fonts.Add(name, font);
    }

    public void RegisterSound(string name, SoundEffect sound)
    {
        _sounds.Add(name, sound);
    }

    public void RegisterSong(string name, Song song)
    {
        _songs.Add(name, song);
    }

    public Texture2dInfo GetTextureInfo(string name)
    {
        return _textureInfos[name];
    }

    public SpriteFont GetFont(string name)
    {
        return _fonts[name];
    }

    public SoundEffect GetSound(string name)
    {
        return _sounds[name];
    }

    public Song GetSong(string name)
    {
        return _songs[name];
    }
}
