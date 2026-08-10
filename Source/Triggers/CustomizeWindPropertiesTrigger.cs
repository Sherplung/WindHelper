namespace Celeste.Mod.WindHelper.Triggers;

[CustomEntity("WindHelper/CustomizeWindPropertiesTrigger")]
[UsedImplicitly]
internal class CustomizeWindPropertiesTrigger : Trigger
{
    private readonly float MaxWindSpeed;
    private readonly WindHelperModuleSession.EasingTypes AdditiveWindEasing;
    private readonly bool AdditiveWindAmbience;
    private readonly bool OneUse;

    public CustomizeWindPropertiesTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        MaxWindSpeed = data.Float("maxWindSpeed");
        AdditiveWindEasing = data.Enum<WindHelperModuleSession.EasingTypes>("easingType");
        AdditiveWindAmbience = data.Bool("additiveWindAmbience");
        OneUse = data.Bool("oneUse");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        WindHelperModule.Session.MaxWindSpeed = MaxWindSpeed;
        WindHelperModule.Session.AdditiveWindEasing = AdditiveWindEasing;
        WindHelperModule.Session.AdditiveWindAmbience = AdditiveWindAmbience;
        if (OneUse) RemoveSelf();
    }
}