using Celeste;
using Celeste.Editor;
using Celeste.Mod.Entities;
using Celeste.Mod.WindHelper.Entities;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WindHelper;

[CustomEntity("WindHelper/AddWindComponentsTrigger")]

internal class AddWindComponentsTrigger : Trigger
{
    public WindController.Patterns Pattern;

    public enum BehaviorTypes
    {
        WhileInside = 0,
        AddPerma = 1,
        AddDuration = 2,
    }

    public BehaviorTypes behavior;

    private Vector2 strength;

    private float duration;

    private bool onlyOnce;

    private bool used;

    public AddWindComponentsTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        behavior = data.Enum<BehaviorTypes>("behaviorType");
        strength.X = data.Float("windX");
        strength.Y = data.Float("windY");
        duration = data.Float("duration");
        onlyOnce = data.Bool("onlyOnce");
        used = false;
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
                    windController.AddPermaWind(strength);
                    break;
                case BehaviorTypes.AddPerma:
                    windController.AddPermaWind(strength);
                    if (onlyOnce) { used = true; }
                    break;
                case BehaviorTypes.AddDuration:
                    windController.AddWind(strength, duration);
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
                    windController.AddPermaWind(-strength);
                    if (onlyOnce) { used = true; }
                    break;
                case BehaviorTypes.AddPerma:
                    break;
                case BehaviorTypes.AddDuration:
                    break;
            }
        }
    }
}
