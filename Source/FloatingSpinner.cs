using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.WindHelper.Entities;

[Tracked(false)]
[CustomEntity("WindHelper/FloatingSpinner")]
internal class FloatingSpinner : Entity
{
    //public static ParticleType P_Move;

    public const float ParticleInterval = 0.02f;

    public DustGraphic Sprite;

    private float offset = Calc.Random.NextFloat();

    private float Mass;

    public FloatingSpinner(Vector2 position, float mass = 1, bool ignore_solids = true)
        : base(position)
    {
        base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Add(new PlayerCollider(OnPlayer));
        Add(new HoldableCollider(OnHoldable));
        Add(new LedgeBlocker());
        Add(new WindMover(Move));
        Add(Sprite = new DustGraphic(ignore_solids, autoControlEyes: true, autoExpandDust: true));
        base.Depth = -50;
        Mass = mass;
    }

    public FloatingSpinner(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("mass"))
    {
    }

    public void ForceInstantiate()
    {
        Sprite.AddDustNodesIfInCamera();
    }

    public override void Update()
    {
        base.Update();
        /*
        WindDustEdges windDustEdges = base.Scene.Entities.FindFirst<WindDustEdges>();
        if (windDustEdges == null)
        {
            windDustEdges= new WindDustEdges();
            base.Scene.Add(windDustEdges);
        }
        */
        if (base.Scene.OnInterval(0.05f, offset) && Sprite.Estableshed)
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                Collidable = Math.Abs(entity.X - base.X) < 128f && Math.Abs(entity.Y - base.Y) < 128f;
            }
        }
    }

    private void OnPlayer(Player player)
    {
        player.Die((player.Position - Position).SafeNormalize());
        Sprite.OnHitPlayer();
    }

    private void OnHoldable(Holdable h)
    {
        h.HitSpinner(this);
    }

    private void Move(Vector2 strength)
    {
        base.Position += strength / Mass;
    }
}
