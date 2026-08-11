namespace Celeste.Mod.WindHelper;

public class WindHelperModule : EverestModule
{
    private static WindHelperModule Instance {get; set;}

    public override Type SettingsType => typeof(WindHelperModuleSettings);
    public static WindHelperModuleSettings Settings => (WindHelperModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(WindHelperModuleSession);
    public static WindHelperModuleSession Session => (WindHelperModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(WindHelperModuleSaveData);
    public static WindHelperModuleSaveData SaveData => (WindHelperModuleSaveData)Instance._SaveData;

    public WindHelperModule()
    {
        Instance = this;
        #if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(WindHelperModule), LogLevel.Debug);
        #else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(WindHelperModule), LogLevel.Info);
        #endif
    }

    // OPTIONAL DEPENDENCIES GO HERE
    internal static bool CrystallineHelperLoaded;

    public override void Load()
    {
        FrostHelperImports.Load();
        CommunalHelperImports.Load();
        GravityHelperImports.Load();
        LifecycleMethods.OnLoad();

        #region Optional Dependency Loading
            EverestModuleMetadata crystallineHelper = new()
            {
                Name = "CrystallineHelper",
                Version = new Version(1, 17, 2)
            };
            CrystallineHelperLoaded = Everest.Loader.DependencyLoaded(crystallineHelper);
        #endregion
    }

    public override void Unload()
    {
        LifecycleMethods.OnUnload();
    }
}