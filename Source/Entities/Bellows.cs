namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/Bellows")]
[UsedImplicitly]
internal class Bellows : Spring
{
    internal enum CustomOrientations
    {
        Floor,
        WallLeft,
        WallRight,
        Ceiling
    }

    // Stole this trick from Frost Helper :heart:
    private static readonly Dictionary<CustomOrientations, Orientations> CustomToRegularOrientation = new()
    {
        [CustomOrientations.WallLeft] = Orientations.WallLeft,
        [CustomOrientations.WallRight] = Orientations.WallRight,
        [CustomOrientations.Floor] = Orientations.Floor,
        [CustomOrientations.Ceiling] = Orientations.Floor
    };

    internal readonly CustomOrientations customOrientation;

    private readonly float windStrength;
    private readonly float windDuration;

    internal float inactiveTimer;

    public Bellows(Vector2 position, CustomOrientations customOrientation, float wind_strength, float wind_duration, bool playerCanUse)
        : base(position, CustomToRegularOrientation[customOrientation], playerCanUse)
    {
        this.customOrientation = customOrientation;
        Orientation = CustomToRegularOrientation[customOrientation];
        windStrength = wind_strength;
        windDuration = wind_duration;
        this.playerCanUse = playerCanUse;

        Remove(Get<PlayerCollider>());
        Add(new PlayerCollider(NewOnCollide));
        Remove(Get<HoldableCollider>());
        Add(new HoldableCollider(NewOnHoldable));
        Remove(Get<PufferCollider>());
        PufferCollider pufferCollider = new PufferCollider(NewOnPuffer);
        Add(pufferCollider);

        sprite.RemoveSelf();
        sprite = GFX.SpriteBank.Create("Sherplung_WindHelper_bellows");
        Add(sprite);
        sprite.Play("idle");
        sprite.Origin.X = sprite.Width / 2f;
        sprite.Origin.Y = sprite.Height;
        Depth = -8501;
        staticMover = new StaticMover
        {
            OnAttach = delegate(Platform p)
            {
                Depth = p.Depth + 1;
            }
        };
        switch (customOrientation)
        {
            case CustomOrientations.Floor:
                Collider = new Hitbox(16f, 6f, -8f, -6f);
                pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                break;
            case CustomOrientations.WallLeft:
                Collider = new Hitbox(6f, 16f, 0f, -8f);
                pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                sprite.Rotation = (float)Math.PI / 2f;
                break;
            case CustomOrientations.WallRight:
                Collider = new Hitbox(6f, 16f, -6f, -8f);
                pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                sprite.Rotation = -(float)Math.PI / 2f;
                break;
            case CustomOrientations.Ceiling:
                Collider = new Hitbox(16f, 6f, -8f);
                pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -4f);
                sprite.Rotation = MathHelper.Pi;
                staticMover.SolidChecker = s => CollideCheck(s, Position - Vector2.UnitY);
                staticMover.JumpThruChecker = jt => CollideCheck(jt, Position - Vector2.UnitY);
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
        staticMover.OnEnable = OnEnable;
        staticMover.OnDisable = OnDisable;
    }

    public Bellows(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum<CustomOrientations>("orientation"), data.Float("wind_strength", 400f), data.Float("wind_duration", 1f), data.Bool("playerCanUse", true))
    {
    }

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Puffer.HitSpring += PufferOnHitSpring;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Puffer.HitSpring -= PufferOnHitSpring;
    }

    private static bool PufferOnHitSpring(On.Celeste.Puffer.orig_HitSpring orig, Puffer self, Spring spring)
    {
        if (spring is Bellows { customOrientation: CustomOrientations.Ceiling })
        {
            if (self.hitSpeed.Y <= 0f)
            {
                self.GotoHitSpeed(224f * Vector2.UnitY);
                self.MoveTowardsX(spring.CenterX, 4f);
                self.bounceWiggler.Start();
                self.Alert(true, false);
                return true;
            }
            return false;
        }
        return orig(self, spring);
    }

    public override void Update()
    {
        base.Update();
        inactiveTimer -= Engine.DeltaTime;
    }

    private void NewOnCollide(Player player)
    {
        if (player.StateMachine.State == Player.StDreamDash || !playerCanUse) return;
        if (CommunalHelperImports.IsImported && CommunalHelperImports.GetDreamTunnelDashState() is { } dreamTunnelState && player.StateMachine.State == dreamTunnelState) return;
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);

        switch (customOrientation)
        {
            case CustomOrientations.Floor:
            case CustomOrientations.Ceiling:
            {
                float realY = GravityHelperImports.InvertIfPlayerInverted(player.Speed.Y);
                if ((customOrientation == CustomOrientations.Floor && realY < 0) || (customOrientation == CustomOrientations.Ceiling && (realY > 0 || (realY == 0 && inactiveTimer > 0))))
                {
                    return;
                }

                bool playerInverted = GravityHelperImports.IsImported && GravityHelperImports.IsPlayerInverted();

                BounceAnimate();

                switch (playerInverted)
                {
                    case true when customOrientation == CustomOrientations.Floor:
                        GravityHelperImports.InvertedSuperBounce(player, Top);
                        break;
                    case false when customOrientation == CustomOrientations.Ceiling:
                        player.SuperBounce(Bottom + player.Height);
                        player.Speed.Y *= -1f;
                        break;
                    default:
                        player.SuperBounce(customOrientation == CustomOrientations.Floor ? Top : Bottom);
                        break;
                }

                player.varJumpSpeed = player.Speed.Y;

                if (playerInverted && customOrientation is CustomOrientations.Ceiling or CustomOrientations.Floor)
                {
                    inactiveTimer = 6f * Engine.DeltaTime;
                }

                switch (customOrientation)
                {
                    case CustomOrientations.Ceiling:
                        windController.AddWind(Vector2.UnitY * windStrength, windDuration);
                        break;
                    case CustomOrientations.Floor:
                        windController.AddWind(-Vector2.UnitY * windStrength, windDuration);
                        break;
                    case CustomOrientations.WallLeft:
                    case CustomOrientations.WallRight:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return;
            }
            case CustomOrientations.WallLeft:
            {
                if (!player.SideBounce(1, Right, CenterY)) return;

                BounceAnimate();
                windController.AddWind(Vector2.UnitX * windStrength, windDuration);
                return;
            }
            case CustomOrientations.WallRight:
            {
                if (!player.SideBounce(-1, Left, CenterY)) return;

                BounceAnimate();
                windController.AddWind(-Vector2.UnitX * windStrength, windDuration);
                return;
            }
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }

    private void NewOnHoldable(Holdable h)
    {
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
        if (!h.HitSpring(this)) return;

        BounceAnimate();
        switch (customOrientation)
        {
            case CustomOrientations.Floor:
                windController.AddWind(-Vector2.UnitY * windStrength, windDuration);
                break;
            case CustomOrientations.WallLeft:
                windController.AddWind(Vector2.UnitX * windStrength, windDuration);
                break;
            case CustomOrientations.WallRight:
                windController.AddWind(-Vector2.UnitX * windStrength, windDuration);
                break;
            case CustomOrientations.Ceiling:
                windController.AddWind(Vector2.UnitY * windStrength, windDuration);
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }

    private void NewOnPuffer(Puffer p)
    {
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
        if (!p.HitSpring(this)) return;

        BounceAnimate();
        switch (customOrientation)
        {
            case CustomOrientations.Floor:
                windController.AddWind(-Vector2.UnitY * windStrength, windDuration);
                break;
            case CustomOrientations.WallLeft:
                windController.AddWind(Vector2.UnitX * windStrength, windDuration);
                break;
            case CustomOrientations.WallRight:
                windController.AddWind(-Vector2.UnitX * windStrength, windDuration);
                break;
            case CustomOrientations.Ceiling:
                windController.AddWind(Vector2.UnitY * windStrength, windDuration);
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }
}