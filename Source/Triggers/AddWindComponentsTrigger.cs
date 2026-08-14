namespace Celeste.Mod.WindHelper.Triggers;

[CustomEntity("WindHelper/AddWindComponentsTrigger")]
[UsedImplicitly]
internal class AddWindComponentsTrigger : Trigger
{
    public enum BehaviorTypes
    {
        WhileInside = 0,
        AddPerma = 1,
        AddDuration = 2
    }

    public BehaviorTypes behavior;
    private readonly Vector2 strength;
    private readonly float duration;
    private readonly bool onlyOnce;

    private bool used;

    public AddWindComponentsTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        behavior = data.Enum<BehaviorTypes>("behaviorType");
        strength.X = data.Float("windX");
        strength.Y = data.Float("windY");
        duration = data.Float("duration");
        onlyOnce = data.Bool("onlyOnce");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
        if (used) return;

        switch (behavior)
        {
            case BehaviorTypes.WhileInside:
                windController.AddPermaWind(strength);
                break;
            case BehaviorTypes.AddPerma:
                windController.AddPermaWind(strength);
                if (onlyOnce) used = true;
                break;
            case BehaviorTypes.AddDuration:
                windController.AddWind(strength, duration);
                if (onlyOnce) used = true;
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
        if (used) return;

        switch (behavior)
        {
            case BehaviorTypes.WhileInside:
                windController.AddPermaWind(-strength);
                if (onlyOnce) used = true;
                break;
            case BehaviorTypes.AddPerma:
            case BehaviorTypes.AddDuration:
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }
}