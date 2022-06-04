using System.Collections.Generic;

namespace TurretDefense.Services.Interfaces;

public interface IScoreManager
{
    List<int> GetScores();

    void PostScore(int score);
}
