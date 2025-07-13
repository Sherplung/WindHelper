using Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WindHelper;

[SettingName("modoptions_windhelpermodule_title")]
public class WindHelperModuleSettings : EverestModuleSettings {

    public enum EasingTypes
    {
        EaseSlowToZero = 0,
        NoEasing = 1,
        EaseFastAlways = 2,
        EaseFastUpEaseSlowDown = 3,
        EaseFastStartEaseSlowEnd = 4
    }

    [SettingName("SHERPLUNG_WINDHELPER_MODOPTIONS_ADDITIVEWINDEASING")]
    [SettingSubText("SHERPLUNG_WINDHELPER_MODOPTIONS_ADDITIVEWINDEASING_HINT")]
    public EasingTypes AdditiveWindEasing { get; set; } = EasingTypes.EaseSlowToZero;

    [SettingName("SHERPLUNG_WINDHELPER_MODOPTIONS_ADDITIVEWINDAMBIENCE")]
    [SettingSubText("SHERPLUNG_WINDHELPER_MODOPTIONS_ADDITIVEWINDAMBIENCE_HINT")]
    public bool AdditiveWindAmbience { get; set; } = false;

}