using System;
using System.Linq;
using Celeste.Mod.WindHelper.Entities;
using Monocle;
using static Celeste.WindController;

namespace Celeste.Mod.WindHelper;

public class WindHelperModule : EverestModule {
    public static WindHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(WindHelperModuleSettings);
    public static WindHelperModuleSettings Settings => (WindHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(WindHelperModuleSession);
    public static WindHelperModuleSession Session => (WindHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(WindHelperModuleSaveData);
    public static WindHelperModuleSaveData SaveData => (WindHelperModuleSaveData) Instance._SaveData;

    public WindHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(WindHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(WindHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        Everest.Events.Level.OnLoadLevel += LoadCustomWindController;

    }

    public override void Unload() {
        Everest.Events.Level.OnLoadLevel -= LoadCustomWindController;

    }
    private void LoadCustomWindController(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        level.Entities.FindFirst<WindController>()?.RemoveSelf();
        level.Add(level.windController = new ExtendedWindController(level.Session.LevelData.WindPattern));
        if (playerIntro != 0)
        {
            level.windController.SetStartPattern();
        }
    }
}