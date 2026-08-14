namespace Celeste.Mod.WindHelper.Extras;

public static class LoadExtendedWindController
{
    internal static bool WindHelperIsNeeded;

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.LevelLoader.ctor += CheckForExtendedWindController;
        Everest.Events.Level.OnLoadLevel += LoadCustomWindController;
        On.Celeste.WindController.Update += WindControllerOnUpdate;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.LevelLoader.ctor -= CheckForExtendedWindController;
        Everest.Events.Level.OnLoadLevel -= LoadCustomWindController;
        On.Celeste.WindController.Update -= WindControllerOnUpdate;
    }

    // Got this from Gravity helper :heart:
    private static bool RequiresWindHelperForSession(Session session)
    {
        bool RequiresWindHelper(EntityData data)
        {
            return data.Name.StartsWith("WindHelper") || data.Has("_windHelper");
        }

        EntityData entityData = session.MapData.Levels.SelectMany(l => l.Entities).FirstOrDefault(RequiresWindHelper);
        return entityData != null || session.MapData.Levels.SelectMany(l => l.Triggers).Any(RequiresWindHelper);
    }

    private static void CheckForExtendedWindController(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
    {
        orig(self, session, startPosition);
        WindHelperIsNeeded = RequiresWindHelperForSession(session);
    }

    private static void LoadCustomWindController(Level level, Player.IntroTypes playerIntro, bool _)
    {
        if (!WindHelperIsNeeded) return;

        level.Add(level.windController = new ExtendedWindController(level.Session.LevelData.WindPattern));
        if (playerIntro == Player.IntroTypes.Transition) return;

        level.windController.SetStartPattern();
    }

    private static void WindControllerOnUpdate(On.Celeste.WindController.orig_Update orig, WindController self)
    {
        if (self.Scene.Tracker.GetEntities<ExtendedWindController>().Count != 0) return;
        orig(self);
    }
}