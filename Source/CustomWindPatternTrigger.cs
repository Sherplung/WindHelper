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

[CustomEntity("WindHelper/CustomWindPatternTrigger")]

internal class CustomWindPatternTrigger : Trigger
{
    public WindController.Patterns Pattern;

    private string instructions;

    private bool onlyOnce;

    private bool used;

    public CustomWindPatternTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        instructions = data.Attr("instructions", "0.0,0.0,0.0");
        onlyOnce = data.Bool("onlyOnce", true);
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
            windController.AddCustomWindPattern(instructions);
            if (onlyOnce) { used = true; }
        }
    }
}
