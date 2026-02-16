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
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using static Celeste.WindController;
using MonoMod;

namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/Bellows")]
internal class Bellows : Spring
{
    /*
    public enum Orientations
    {
        Floor,
        WallLeft,
        WallRight
    }
    

    private Sprite sprite;

    private Wiggler wiggler;

    private StaticMover staticMover;

    public Orientations Orientation;

    private bool playerCanUse;

    public Color DisabledColor = Color.White;

    public bool VisibleWhenDisabled;
    */

    private float windStrength;

    private float windDuration;

    private Level level;

    public WindController.Patterns Pattern;

    public Bellows(Vector2 position, Orientations orientation, float wind_strength, float wind_duration, bool playerCanUse)
        : base(position, orientation, playerCanUse)
    {
        Orientation = orientation;
        windStrength = wind_strength;
        windDuration = wind_duration;  
        this.playerCanUse = playerCanUse;
        PlayerCollider playerCollider = Get<PlayerCollider>();
        Action<Player> origP = playerCollider.OnCollide;
        playerCollider.OnCollide = player =>
        {
            OnCollide(player);
        };
        HoldableCollider hCollider = Get<HoldableCollider>();
        Action<Holdable> origH = hCollider.OnCollide;
        hCollider.OnCollide = h =>
        {
            OnHoldable(h);
        };
        PufferCollider pufferCollider = Get<PufferCollider>();
        Action<Puffer> origPFF = pufferCollider.OnCollide;
        pufferCollider.OnCollide = p =>
        {
            OnPuffer(p);
        };
        sprite.RemoveSelf();
        sprite = GFX.SpriteBank.Create("Sherplung_WindHelper_bellows");
        Add(sprite);
        sprite.Play("idle");
        sprite.Origin.X = sprite.Width / 2f;
        sprite.Origin.Y = sprite.Height;
        base.Depth = -8501;
        staticMover = new StaticMover();
        staticMover.OnAttach = delegate (Platform p)
        {
            base.Depth = p.Depth + 1;
        };
        switch (orientation)
        {
            case Orientations.Floor:
                base.Collider = new Hitbox(16f, 6f, -8f, -6f);
                pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                break;
            case Orientations.WallLeft:
                base.Collider = new Hitbox(6f, 16f, 0f, -8f);
                pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                sprite.Rotation = (float)Math.PI / 2f;
                break;
            case Orientations.WallRight:
                base.Collider = new Hitbox(6f, 16f, -6f, -8f);
                pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                sprite.Rotation = -(float)Math.PI / 2f;
                break;
            default:
                throw new Exception("Orientation not supported!");
        }
        staticMover.OnEnable = OnEnable;
        staticMover.OnDisable = OnDisable;
    }

    public Bellows(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum<Orientations>("orientation"), data.Float("wind_strength", defaultValue: 400f), data.Float("wind_duration", defaultValue: 1f), data.Bool("playerCanUse", defaultValue: true))
    {
    }

    [MonoModLinkTo("Celeste.Entity", "System.Void Added()")]
    public void base_Added(Scene scene)
    {
        base.Added(scene);
    }
    public override void Added(Scene scene)
    {
        base_Added(scene);
        level = SceneAs<Level>();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
    }

    private new void OnCollide(Player player)
    {
        ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
        if (windController == null)
        {
            windController = new ExtendedWindController(Pattern);
            base.Scene.Add(windController);
        }

        if (player.StateMachine.State == 9 || !playerCanUse)
        {
            return;
        }

        if (WindHelperModule.communalHelperLoaded)
        {
            if (CommunalHelperIntegration.GetDreamTunnelDashState?.Invoke() is { } dreamTunnelState && player.StateMachine.State == dreamTunnelState)
            {
                return;
            }
        }

        if (Orientation == Orientations.Floor)
        {
            if (player.Speed.Y >= 0f)
            {
                windController.AddWind((-Vector2.UnitY * windStrength), windDuration);
                BounceAnimate();
                player.SuperBounce(Top);
            }
            return;
        }
        if (Orientation == Orientations.WallLeft)
        {
            if (player.SideBounce(1, Right, CenterY))
            {
                BounceAnimate();
                windController.AddWind(Vector2.UnitX * windStrength, windDuration);
            }
            return;
        }
        if (Orientation == Orientations.WallRight)
        {
            if (player.SideBounce(-1, Left, CenterY))
            {
                BounceAnimate();
                windController.AddWind(-Vector2.UnitX * windStrength, windDuration);
            }
            return;
        }
        throw new Exception("Orientation not supported!");
    }

    private new void BounceAnimate()
    {
        Audio.Play("event:/game/general/spring", base.BottomCenter);
        staticMover.TriggerPlatform();
        sprite.Play("bounce", restart: true);
        wiggler.Start();
    }


    private new void OnHoldable(Holdable h)
    {
        ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
        if (windController == null)
        {
            windController = new ExtendedWindController(Pattern);
            base.Scene.Add(windController);
        }
        if (h.HitSpring(this))
        {
            BounceAnimate();
            switch (Orientation)
            {
                case Orientations.Floor:
                    windController.AddWind(-Vector2.UnitY * windStrength, windDuration);
                    break;
                case Orientations.WallLeft:
                    windController.AddWind(Vector2.UnitX * windStrength, windDuration);
                    break;
                case Orientations.WallRight:
                    windController.AddWind(-Vector2.UnitX * windStrength, windDuration);
                    break;
            }
        }
    }

    private new void OnPuffer(Puffer p)
    {
        ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
        if (windController == null)
        {
            windController = new ExtendedWindController(Pattern);
            base.Scene.Add(windController);
        }
        if (p.HitSpring(this))
        {
            BounceAnimate();
            switch (Orientation)
            {
                case Orientations.Floor:
                    windController.AddWind(-Vector2.UnitY * windStrength, windDuration);
                    break;
                case Orientations.WallLeft:
                    windController.AddWind(Vector2.UnitX * windStrength, windDuration);
                    break;
                case Orientations.WallRight:
                    windController.AddWind(-Vector2.UnitX * windStrength, windDuration);
                    break;
            }
        }
    }
    private new void OnSeeker(Seeker seeker)
    {
        ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
        if (seeker.Speed.Y >= -120f)
        {
            BounceAnimate();
            seeker.HitSpring();
            windController.AddWind(-Vector2.UnitY * windStrength, windDuration);
        }
    }
    private new void OnEnable()
    {
        Visible = (Collidable = true);
        sprite.Color = Color.White;
        sprite.Play("idle");
    }

    private new void OnDisable()
    {
        Collidable = false;
        if (VisibleWhenDisabled)
        {
            //sprite.Play("disabled");
            sprite.Color = DisabledColor;
        }
        else
        {
            Visible = false;
        }
    }
}
