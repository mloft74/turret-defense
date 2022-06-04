namespace TurretDefense;

public static class Constants
{
    // world const
    public const int WORLD_SIZE = 5000;


    // turret consts
    public const int TURRET_1_BUY = 14;
    public const int TURRET_2_BUY = 20;
    public const int TURRET_3_BUY = 24;
    public const int TURRET_4_BUY = 30;

    // grid consts
    private const int TURRET_NUM = 11;
    public const int TURRET_GRID_SIZE = 3;
    public const int CREEP_GRID_SIZE = 3;
    private const int TOTAL_EDGE_GRID_SIZE = 2;
    public const int GRID_NUM = TURRET_NUM * TURRET_GRID_SIZE + TOTAL_EDGE_GRID_SIZE;

    // font consts
    public const string FONT_NAME = "roboto";
    public const int LARGE_FONT = 400;
    public const int MEDIUM_FONT = 300;
    public const int SMALL_FONT = 200;

    // permanent input consts
    public const string MENU_UP = "Menu up";
    public const string MENU_DOWN = "Menu down";
    public const string ESCAPE = "Escape";
    public const string ENTER = "Enter";
    public const string MOUSE_LEFT = "Mouse left";
    public const string MOUSE_RIGHT = "Mouse right";

    // rebindable input consts
    public const string UPGRADE = "Upgrade";
    public const string SELL = "Sell";
    public const string START_LEVEL = "Start level";

    // blank texture const
    public const string BLANK = "blank";

    // menu texture const
    public const string SELECTOR = "selector";

    // background const
    public const string BACKGROUND = "background";

    // explosion texture consts
    public const string FIRE = "fire";
    public const string SMOKE = "smoke";

    // creep texture consts
    public const string CREEP_1 = "creep1";
    public const string CREEP_2 = "creep2";
    public const string CREEP_3 = "creep3";

    // turret texture consts
    public const string TURRET_RANGE = "turretRange";
    public const string TURRET_BASE = "turretBase";
    public const string TURRET_1 = "turret1";
    public const string TURRET_2 = "turret2";
    public const string TURRET_3 = "turret3";
    public const string TURRET_4 = "turret4";
    public const string TURRET_1_1 = TURRET_1 + "1";
    public const string TURRET_1_2 = TURRET_1 + "2";
    public const string TURRET_1_3 = TURRET_1 + "3";
    public const string TURRET_2_1 = TURRET_2 + "1";
    public const string TURRET_2_2 = TURRET_2 + "2";
    public const string TURRET_2_3 = TURRET_2 + "3";
    public const string TURRET_3_1 = TURRET_3 + "1";
    public const string TURRET_3_2 = TURRET_3 + "2";
    public const string TURRET_3_3 = TURRET_3 + "3";
    public const string TURRET_4_1 = TURRET_4 + "1";
    public const string TURRET_4_2 = TURRET_4 + "2";
    public const string TURRET_4_3 = TURRET_4 + "3";

    // projectile texture consts
    public const string BULLET = "bullet";

    // sound consts
    public const string CREEP_DEAD = "creepDead";
    public const string CREEP_HIT = "creepHit";
    public const string SHOOT = "shoot";
    public const string EXPLOSION = "explosion";
    public const string MENU_BLIP = "menuBlip";
    public const string TURRET_PLACE = "turretPlace";

    // song const
    public const string SONG = "song";

    // layer consts
    public const float MENU_DEPTH = 0.00f;
    public const float WORLD_DEPTH = 0.00f;
    public const float TURRET_RANGE_DEPTH = 0.01f;
    public const float TURRET_BASE_DEPTH = 0.02f;
    public const float TURRET_HEAD_DEPTH = 0.03f;
    public const float GROUND_CREEP_DEPTH = 0.03f;
    public const float GROUND_CREEP_RED_DEPTH = 0.04f;
    public const float GROUND_CREEP_GREEN_DEPTH = 0.05f;
    public const float AIR_CREEP_DEPTH = 0.06f;
    public const float AIR_CREEP_RED_DEPTH = 0.07f;
    public const float AIR_CREEP_GREEN_DEPTH = 0.08f;
    public const float PARTICLE_DEPTH = 0.09f;
    public const float PROJECTILE_DEPTH = 0.10f;
    public const float SCORE_DEPTH = 0.11f;
    public const float UI_BACKGROUND_DEPTH = 0.12f;
    public const float UI_STRING_DEPTH = 0.14f;
    public const float UI_TURRET_RANGE_DEPTH = 0.14f;
    public const float UI_TURRET_BASE_DEPTH = 0.15f;
    public const float UI_TURRET_HEAD_DEPTH = 0.16f;
    public const float PREVIEW_TURRET_RANGE_DEPTH = 0.17f;
    public const float PREVIEW_TURRET_BASE_DEPTH = 0.18f;
    public const float PREVIEW_TURRET_HEAD_DEPTH = 0.19f;
}
