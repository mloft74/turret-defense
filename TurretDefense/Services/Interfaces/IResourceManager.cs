using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TurretDefense.Models;

namespace TurretDefense.Services.Interfaces;

public interface IResourceManager
{
    void RegisterTexture(string name, Texture2D texture, IEnumerable<int> timings);

    void RegisterFont(string name, SpriteFont font);

    void RegisterSound(string name, SoundEffect sound);

    void RegisterSong(string name, Song song);

    Texture2dInfo GetTextureInfo(string name);

    SpriteFont GetFont(string name);

    SoundEffect GetSound(string name);

    Song GetSong(string name);
}
