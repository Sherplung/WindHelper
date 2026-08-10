namespace Celeste.Mod.WindHelper.Extras;

public static class LoadExtendedWindController
{
    [OnLoad]
    internal static void Load()
    {
        Everest.Events.Level.OnLoadLevel += LoadCustomWindController;
    }

    [OnUnload]
    internal static void Unload()
    {
        Everest.Events.Level.OnLoadLevel -= LoadCustomWindController;
    }

    private static void LoadCustomWindController(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        level.Entities.FindFirst<WindController>()?.RemoveSelf();
        level.Add(level.windController = new ExtendedWindController(level.Session.LevelData.WindPattern));
        if (playerIntro == Player.IntroTypes.Transition) return;

        level.windController.SetStartPattern();
    }
}