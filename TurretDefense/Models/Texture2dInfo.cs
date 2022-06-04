using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TurretDefense.Models;

public record Texture2dInfo
{
    public Texture2D Texture { get; init; }

    public Vector2 Size { get; init; }

    public Vector2 InverseScale { get; init; }

    public List<TimeSpan> FrameTimeStamps { get; init; }

    public TimeSpan TotalTime { get; init; }

    public Texture2dInfo(Texture2D texture, IEnumerable<int> timings)
    {
        Texture = texture;
        Size = new Vector2(texture.Height);
        InverseScale = Vector2.One / Size;
        var total = 0;
        FrameTimeStamps = timings.Select(timing =>
        {
            total += timing;
            return TimeSpan.FromMilliseconds(total);
        }).ToList();
        TotalTime = TimeSpan.FromMilliseconds(total);
    }
}
