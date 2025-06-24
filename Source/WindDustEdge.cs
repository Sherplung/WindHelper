using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;

namespace Celeste.Mod.WindHelper.Entities;

[Tracked(false)]
public class WindDustEdge : Component
{
    public Action RenderDust;

    public WindDustEdge(Action onRenderDust)
        : base(active: false, visible: true)
    {
        RenderDust = onRenderDust;
    }
}
