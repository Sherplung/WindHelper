using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Editor;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using System.Collections;

namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/ControllableWindTrigger")]

internal class ControllableWindTrigger : Trigger
{
    private float Timer;

    public WindController.Patterns Pattern;

    private Coroutine windCoroutine;

    public enum BehaviorTypes 
    {
        WhileInside,
        Add,
        Remove,
        Duration
    }

    public BehaviorTypes behavior;

    private float strength;

    private float duration;

    private bool ongoingDuration;

    private bool onlyOnce;

    private bool used;

    public ControllableWindTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        behavior = data.Enum<BehaviorTypes>("behaviorType");
        strength = data.Float("windStrength");
        duration = data.Float("duration");
        onlyOnce = data.Bool("onlyOnce");
        used = false;
    }

    private IEnumerator TimedControllableWind()
    {
        ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
        ongoingDuration = true;
        windController.ChangeControllableWind(strength, true);
        yield return duration;
        windController.ChangeControllableWind(strength, false);
        ongoingDuration = false;
    }
    public override void OnEnter(Player player)
    {
        if (!used)
        {
            base.OnEnter(player);
            ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
            if (windController == null)
            {
                windController = new ExtendedWindController(Pattern);
                base.Scene.Add(windController);
            }
            switch (behavior)
            {
                case BehaviorTypes.WhileInside:
                    windController.ChangeControllableWind(strength, true);
                    break;
                case BehaviorTypes.Add:
                    windController.ChangeControllableWind(strength, true);
                    if (onlyOnce) { used = true; }
                    break;
                case BehaviorTypes.Remove:
                    windController.ChangeControllableWind(strength, false);
                    if (onlyOnce) { used = true; }
                    break;
                case BehaviorTypes.Duration:
                    if (!ongoingDuration)
                    {
                        Add(windCoroutine = new Coroutine(TimedControllableWind()));
                    }
                    if (onlyOnce) { used = true; }
                    break;
            }
        }
    }

    public override void OnLeave(Player player)
    {
        if (!used)
        {
            base.OnLeave(player);
            ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
            if (windController == null)
            {
                windController = new ExtendedWindController(Pattern);
                base.Scene.Add(windController);
            }
            switch (behavior)
            {
                case BehaviorTypes.WhileInside:
                    windController.ChangeControllableWind(strength, false);
                    if (onlyOnce) { used = true; }
                    break;
                case BehaviorTypes.Add:
                    break;
                case BehaviorTypes.Remove:
                    break;
                case BehaviorTypes.Duration:
                    break;
            }
        }
    }
}
