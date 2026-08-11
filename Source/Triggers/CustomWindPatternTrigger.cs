namespace Celeste.Mod.WindHelper.Triggers;

[CustomEntity("WindHelper/CustomWindPatternTrigger")]
[UsedImplicitly]
internal class CustomWindPatternTrigger : Trigger
{
    private readonly string instructions;
    private readonly bool onlyOnce;

    public CustomWindPatternTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        instructions = data.Attr("instructions", "0.0,0.0,0.0");
        onlyOnce = data.Bool("onlyOnce", true);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);

        windController.AddCustomWindPattern(instructions);
        if (onlyOnce) RemoveSelf();
    }
}