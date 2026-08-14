namespace Celeste.Mod.WindHelper.Triggers;

[CustomEntity("WindHelper/ControllableWindTrigger")]
[UsedImplicitly]
internal class ControllableWindTrigger : Trigger
{
    public enum BehaviorTypes
    {
        WhileInside,
        Add,
        Remove,
        Duration
    }

    public BehaviorTypes behavior;
    private readonly float strength;
    private readonly float duration;
    private readonly bool onlyOnce;

    private bool currentlyActive;
    private bool used;

    public ControllableWindTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        behavior = data.Enum<BehaviorTypes>("behaviorType");
        strength = data.Float("windStrength");
        duration = data.Float("duration");
        onlyOnce = data.Bool("onlyOnce");
    }

    private IEnumerator TimedControllableWind()
    {
        ExtendedWindController windController = Scene.Tracker.GetEntity<ExtendedWindController>();

        currentlyActive = true;
        windController.ChangeControllableWind(strength);

        yield return duration;
        windController.ChangeControllableWind(strength, false);
        currentlyActive = false;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
        if (used) return;

        switch (behavior)
        {
            case BehaviorTypes.WhileInside:
                windController.ChangeControllableWind(strength);
                break;
            case BehaviorTypes.Add:
                windController.ChangeControllableWind(strength);
                if (onlyOnce) used = true;
                break;
            case BehaviorTypes.Remove:
                windController.ChangeControllableWind(strength, false);
                if (onlyOnce) used = true;
                break;
            case BehaviorTypes.Duration:
                if (currentlyActive) return;
                Add(new Coroutine(TimedControllableWind()));
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
                windController.ChangeControllableWind(strength, false);
                if (onlyOnce) used = true;
                break;
            case BehaviorTypes.Add:
            case BehaviorTypes.Remove:
            case BehaviorTypes.Duration:
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }
}