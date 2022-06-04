using System.Collections.Generic;
using TurretDefense.Services.Interfaces;

namespace TurretDefense.Services;

public class ScoreManager : IScoreManager
{
    private const int KEEP = 10;
    private readonly List<int> _scores;

    public ScoreManager(List<int> scores)
    {
        _scores = scores;
    }

    public List<int> GetScores()
    {
        return new(_scores);
    }

    public void PostScore(int score)
    {
        const int sentinel = -1;
        var index = sentinel;
        for (var i = 0; i < _scores.Count; ++i)
        {
            if (score <= _scores[i]) continue;
            index = i;
            break;
        }

        index = index == sentinel ? _scores.Count : index;
        _scores.Insert(index, score);
        if (_scores.Count <= KEEP) return;
        _scores.RemoveRange(KEEP, _scores.Count - KEEP);
    }
}
