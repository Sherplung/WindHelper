namespace Celeste.Mod.WindHelper;

public class WindHelperModuleSession : EverestModuleSession
{
    public enum EasingTypes
    {
        EaseSlowToZero = 0,
        NoEasing = 1,
        EaseFastAlways = 2,
        EaseFastUpEaseSlowDown = 3,
        EaseFastStartEaseSlowEnd = 4
    }

    public float MaxWindSpeed = 2000f;

    public EasingTypes AdditiveWindEasing {get; set;} = EasingTypes.EaseSlowToZero;

    public bool AdditiveWindAmbience {get; set;} = false;
}