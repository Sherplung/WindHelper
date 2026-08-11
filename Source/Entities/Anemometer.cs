namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/Anemometer")]
[UsedImplicitly]
internal class Anemometer : Actor
{
    private enum WindDirections
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,
        DashDirection = 8
    }

    private readonly WindDirections windDirection;
    private Vector2 windDirectionVector;
    private readonly float windStrength;
    private readonly float windDuration;
    private float windCooldown;

    private readonly int uses;
    private int usesRemaining;
    private readonly bool canRefresh;

    private readonly Sprite sprite;
    public Holdable Hold;
    private readonly Collision onCollideH;
    private readonly Collision onCollideV;
    public HoldableCollider hitSeeker = null;

    private Level Level;
    public Vector2 Speed;
    private Vector2 previousPosition;

    private float noGravityTimer;
    private Vector2 prevLiftSpeed;
    private float hardVerticalHitSoundCooldown;

    private readonly string animDir;
    private string animCount;

    public Anemometer(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        windDirection = data.Enum<WindDirections>("wind_direction");
        windStrength = data.Float("wind_strength");
        windDuration = data.Float("wind_duration");
        uses = data.Int("uses");
        usesRemaining = uses;
        canRefresh = data.Bool("canRefresh", true);
        previousPosition = Position;
        Depth = 100;
        Collider = new Hitbox(8f, 10f, -4f, -10f);
        Add(sprite = GFX.SpriteBank.Create("Sherplung_WindHelper_anemometer"));
        Add(Hold = new Holdable(0.1f));
        Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
        Hold.SlowFall = false;
        Hold.SlowRun = false;
        Hold.OnPickup = OnPickup;
        Hold.OnRelease = OnRelease;
        Hold.DangerousCheck = Dangerous;
        Hold.OnHitSeeker = HitSeeker;
        Hold.OnHitSpring = HitSpring;
        Hold.OnHitSpinner = HitSpinner;
        Hold.SpeedGetter = () => Speed;
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        LiftSpeedGraceTime = 0.1f;
        Hold.SpeedSetter = delegate(Vector2 speed)
        {
            Speed = speed;
        };
        switch (windDirection)
        {
            case WindDirections.Up:
                windDirectionVector.Y = -1;
                windDirectionVector.X = 0;
                windDirectionVector.Normalize();
                animDir = "U";
                break;
            case WindDirections.UpRight:
                windDirectionVector.Y = -1;
                windDirectionVector.X = 1;
                windDirectionVector.Normalize();
                animDir = "UR";
                break;
            case WindDirections.Right:
                windDirectionVector.Y = 0;
                windDirectionVector.X = 1;
                windDirectionVector.Normalize();
                animDir = "R";
                break;
            case WindDirections.DownRight:
                windDirectionVector.Y = 1;
                windDirectionVector.X = 1;
                windDirectionVector.Normalize();
                animDir = "DR";
                break;
            case WindDirections.Down:
                windDirectionVector.Y = 1;
                windDirectionVector.X = 0;
                windDirectionVector.Normalize();
                animDir = "D";
                break;
            case WindDirections.DownLeft:
                windDirectionVector.Y = 1;
                windDirectionVector.X = -1;
                windDirectionVector.Normalize();
                animDir = "DL";
                break;
            case WindDirections.Left:
                windDirectionVector.Y = 0;
                windDirectionVector.X = -1;
                windDirectionVector.Normalize();
                animDir = "L";
                break;
            case WindDirections.UpLeft:
                windDirectionVector.Y = -1;
                windDirectionVector.X = -1;
                windDirectionVector.Normalize();
                animDir = "UL";
                break;
            case WindDirections.DashDirection:
                windDirectionVector.Y = 0;
                windDirectionVector.X = 0;
                windDirectionVector.Normalize();
                animDir = "DD";
                break;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
        UpdateSprite();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level = SceneAs<Level>();
    }

    private void Refresh()
    {
        if (!canRefresh) return;
        usesRemaining = uses;
    }

    private void UpdateSprite()
    {
        if (uses < 0)
        {
            animCount = "I";
        }
        else
            animCount = usesRemaining switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                _ => "3"
            };
        sprite.Play(animCount + animDir);
    }

    public override void Update()
    {
        base.Update();
        hardVerticalHitSoundCooldown -= Engine.DeltaTime;
        windCooldown -= Engine.DeltaTime;
        Depth = 100;
        Player player = Scene.Tracker.GetEntity<Player>();
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);

        if (Hold.IsHeld)
        {
            prevLiftSpeed = Vector2.Zero;
            if (Input.Dash.Pressed && windCooldown <= 0 && usesRemaining != 0)
            {
                if (windDirection == WindDirections.DashDirection)
                {
                    if (Input.MoveX != 0 || Input.MoveY != 0)
                    {
                        windDirectionVector = Utils.CorrectDashPrecision(Input.GetAimVector().SafeNormalize(Vector2.Zero));
                    }
                    else
                    {
                        windDirectionVector = player.Facing == Facings.Right ? Vector2.UnitX : -Vector2.UnitX;
                    }
                }
                windController.AddWind(windStrength * windDirectionVector, windDuration);
                windCooldown = windDuration;
                Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
                usesRemaining--;
                UpdateSprite();
            }
            else if (player.OnGround())
            {
                Refresh();
                UpdateSprite();
            }
        }
        else
        {
            if (OnGround())
            {
                Refresh();
                UpdateSprite();
                float target = !OnGround(Position + Vector2.UnitX * 3f) ? 20f : OnGround(Position - Vector2.UnitX * 3f) ? 0f : -20f;
                Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                Vector2 liftSpeed = LiftSpeed;
                if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                {
                    Speed = prevLiftSpeed;
                    prevLiftSpeed = Vector2.Zero;
                    Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                    if (Speed.X != 0f && Speed.Y == 0f)
                    {
                        Speed.Y = -60f;
                    }
                    if (Speed.Y < 0f)
                    {
                        noGravityTimer = 0.15f;
                    }
                }
                else
                {
                    prevLiftSpeed = liftSpeed;
                    if (liftSpeed.Y < 0f && Speed.Y < 0f)
                    {
                        Speed.Y = 0f;
                    }
                }
            }
            else if (Hold.ShouldHaveGravity)
            {
                float num = 800f;
                if (Math.Abs(Speed.Y) <= 30f)
                {
                    num *= 0.5f;
                }
                float num2 = 350f;
                if (Speed.Y < 0f)
                {
                    num2 *= 0.5f;
                }
                Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                if (noGravityTimer > 0f)
                {
                    noGravityTimer -= Engine.DeltaTime;
                }
                else
                {
                    Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                }
            }
            previousPosition = ExactPosition;
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            bool actorInverted = GravityHelperImports.IsImported && GravityHelperImports.IsActorInverted(this);
            if (Center.X > Level.Bounds.Right)
            {
                MoveH(32f * Engine.DeltaTime);
                if (Left - 8f > Level.Bounds.Right)
                {
                    RemoveSelf();
                }
            }
            else if (Left < Level.Bounds.Left)
            {
                Left = Level.Bounds.Left;
                Speed.X *= -0.4f;
            }
            else if (Top < Level.Bounds.Top - 4)
            {
                if (actorInverted)
                {
                    RemoveSelf();
                    return;
                }
                
                Top = Level.Bounds.Top + 4;
                Speed.Y = 0f;
            }
            else if (Top > Level.Bounds.Bottom + 16)
            {
                if (!actorInverted)
                {
                    RemoveSelf();
                    return;
                }
                
                Top = Level.Bounds.Top + 4;
                Speed.Y = 0f;
            }
            if (X < Level.Bounds.Left + 10)
            {
                MoveH(32f * Engine.DeltaTime);
            }
            TempleGate templeGate = CollideFirst<TempleGate>();
            if (templeGate != null && player != null)
            {
                templeGate.Collidable = false;
                MoveH(Math.Sign(player.X - X) * 32 * Engine.DeltaTime);
                templeGate.Collidable = true;
            }
            Hold.CheckAgainstColliders();
        }
    }

    public void ExplodeLaunch(Vector2 from)
    {
        if (Hold.IsHeld) return;
        Speed = (Center - from).SafeNormalize(120f);
        SlashFx.Burst(Center, Speed.Angle());
    }

    public bool Dangerous(HoldableCollider holdableCollider) => !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;

    public void HitSeeker(Seeker seeker)
    {
        Audio.Play("event:/sherplung/wind_helper/anemometer_impact", Position);
        if (Hold.IsHeld) return;

        Speed = (Center - seeker.Center).SafeNormalize(120f);
    }

    public void HitSpinner(Entity spinner)
    {
        if (Hold.IsHeld || !(Speed.Length() < 0.01f) || !(LiftSpeed.Length() < 0.01f) || !((previousPosition - ExactPosition).Length() < 0.01f) || !OnGround()) return;
        int num = Math.Sign(X - spinner.X);
        if (num == 0)
        {
            num = 1;
        }
        Speed.X = num * 120f;
        Speed.Y = -30f;
    }

    public bool HitSpring(Spring spring)
    {
        if (Hold.IsHeld) return false;

        Vector2 getSpringSpeedMultiplier = FrostHelperImports.IsImported ? FrostHelperImports.GetSpringSpeedMultiplier(spring) : Vector2.One;
        float realY = GravityHelperImports.InvertIfActorInverted(this, Speed.Y);
        bool actorInverted = GravityHelperImports.IsImported && GravityHelperImports.IsActorInverted(this);

        if (spring is Bellows bellows)
        {
            switch (bellows.customOrientation)
            {
                case Bellows.CustomOrientations.Floor:
                case Bellows.CustomOrientations.Ceiling:
                    if ((bellows.customOrientation == Bellows.CustomOrientations.Floor && realY < 0) || (bellows.customOrientation == Bellows.CustomOrientations.Ceiling && (realY > 0 || (realY == 0 && bellows.inactiveTimer > 0))))
                    {
                        return false;
                    }
                    Speed.X *= 0.5f;
                    Speed.Y = actorInverted switch
                    {
                        true when bellows.customOrientation == Bellows.CustomOrientations.Floor => 160f,
                        true when bellows.customOrientation == Bellows.CustomOrientations.Ceiling => -160f,
                        false when bellows.customOrientation == Bellows.CustomOrientations.Floor => -160f,
                        false when bellows.customOrientation == Bellows.CustomOrientations.Ceiling => 160f,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    noGravityTimer = 0.15f;
                    if (actorInverted && bellows.customOrientation is Bellows.CustomOrientations.Ceiling or Bellows.CustomOrientations.Floor)
                    {
                        bellows.inactiveTimer = 6f * Engine.DeltaTime;
                    }
                    return true;
                case Bellows.CustomOrientations.WallLeft:
                    if (!(Speed.X <= 0f)) return false;
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                case Bellows.CustomOrientations.WallRight:
                    if (!(Speed.X >= 0f)) return false;
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                default:
                    throw new Exception("Impossible Enum Value! How did you do that?");
            }
        }

        if (FrostHelperImports.SafeIsCeilingSpring(spring))
        {
            // I shouldn't have to do this????
            if ((!actorInverted && realY < 0) || (actorInverted && realY > 0)) return false;
            Speed.X *= 0.5f;
            Speed.Y = actorInverted ? 160f : -160f;
            noGravityTimer = 0.15f;
            Speed *= getSpringSpeedMultiplier;
            return true;
        }

        switch (spring.Orientation)
        {
            case Spring.Orientations.Floor:
                if (realY < 0) return false;
                Speed.X *= 0.5f;
                Speed.Y = actorInverted ? 160f : -160f;
                noGravityTimer = 0.15f;
                Speed *= getSpringSpeedMultiplier;
                return true;
            case Spring.Orientations.WallLeft:
                if (Speed.X > 0f) return false;
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                Speed *= getSpringSpeedMultiplier;
                return true;
            case Spring.Orientations.WallRight:
                if (Speed.X < 0f) return false;
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                Speed *= getSpringSpeedMultiplier;
                return true;
            default:
                throw new Exception("Impossible Enum Value! How did you do that?");
        }
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch dashSwitch)
        {
            dashSwitch.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        }
        Audio.Play("event:/sherplung/wind_helper/anemometer_impact", Position);
        Speed.X *= -0.4f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch dashSwitch)
        {
            dashSwitch.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }
        if (Speed.Y > 0f)
        {
            if (hardVerticalHitSoundCooldown <= 0f)
            {
                Audio.Play("event:/sherplung/wind_helper/anemometer_impact", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                hardVerticalHitSoundCooldown = 1f;
            }
            //Audio.Play(SurfaceIndex.GetPathFromIndex(9) + "/landing", Position, "crystal_velocity", 0f);
        }
        if (Speed.Y > 140f && data.Hit is not SwapBlock && data.Hit is not DashSwitch)
        {
            Speed.Y *= -0.6f;
        }
        else
        {
            Speed.Y = 0f;
        }
    }

    public override bool IsRiding(Solid solid) => Speed.Y == 0f && base.IsRiding(solid);

    private void OnPickup()
    {
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
    }

    private void OnRelease(Vector2 force)
    {
        RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }
        Speed = force * 200f;
        if (Speed != Vector2.Zero)
        {
            noGravityTimer = 0.1f;
        }
    }
}