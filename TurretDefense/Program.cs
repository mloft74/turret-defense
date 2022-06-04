using System;

namespace TurretDefense;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new GameLoop();
        game.Run();
    }
}
