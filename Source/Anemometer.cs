using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.WindController;

namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/Anemometer")]

internal class Anemometer : Actor
{
    public enum WindDirections
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

    private Vector2 windDirectionVector;

    private float windStrength;

    private float windDuration;

    private float windCooldown;

    private WindDirections windDirection;

    private int uses;

    private int usesRemaining;

    public Vector2 Speed;

    public Holdable Hold;

    private Sprite sprite;

    private Level Level;

    private Collision onCollideH;

    private Collision onCollideV;

    private float noGravityTimer;

    private Vector2 prevLiftSpeed;

    private Vector2 previousPosition;

    private HoldableCollider hitSeeker;

    private float hardVerticalHitSoundCooldown;

    public WindController.Patterns Pattern;

    private string animDir;

    private string animCount;

    public Anemometer(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        windDirection = data.Enum<WindDirections>("wind_direction");
        windStrength = data.Float("wind_strength");
        windDuration = data.Float("wind_duration");
        uses = data.Int("uses");
        usesRemaining = uses;
        if (uses < 0) { animCount = "I"; }
        else if (uses == 0) { animCount = "0"; }
        else if (uses == 1) { animCount = "1"; }
        else if (uses == 2) { animCount = "2"; }
        else if (uses == 3) { animCount = "3"; }
        else { animCount = "3"; }
        previousPosition = base.Position;
        base.Depth = 100;
        base.Collider = new Hitbox(8f, 10f, -4f, -10f);
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
        Hold.SpeedSetter = delegate (Vector2 speed)
        {
            Speed = speed;
        };
        switch (windDirection)
        {
            case WindDirections.Up:
                windDirectionVector.Y = -1;
                windDirectionVector.X = 0;
                windDirectionVector.SafeNormalize();
                animDir = "U";
                break;
            case WindDirections.UpRight:
                windDirectionVector.Y = -1;
                windDirectionVector.X = 1;
                windDirectionVector.SafeNormalize();
                animDir = "UR";
                break;
            case WindDirections.Right:
                windDirectionVector.Y = 0;
                windDirectionVector.X = 1;
                windDirectionVector.SafeNormalize();
                animDir = "R";
                break;
            case WindDirections.DownRight:
                windDirectionVector.Y = 1;
                windDirectionVector.X = 1;
                windDirectionVector.SafeNormalize();
                animDir = "DR";
                break;
            case WindDirections.Down:
                windDirectionVector.Y = 1;
                windDirectionVector.X = 0;
                windDirectionVector.SafeNormalize();
                animDir = "D";
                break;
            case WindDirections.DownLeft:
                windDirectionVector.Y = 1;
                windDirectionVector.X = -1;
                windDirectionVector.SafeNormalize();
                animDir = "DL";
                break;
            case WindDirections.Left:
                windDirectionVector.Y = 0;
                windDirectionVector.X = -1;
                windDirectionVector.SafeNormalize();
                animDir = "L";
                break;
            case WindDirections.UpLeft:
                windDirectionVector.Y = -1;
                windDirectionVector.X = -1;
                windDirectionVector.SafeNormalize();
                animDir = "UL";
                break;
            case WindDirections.DashDirection:
                windDirectionVector.Y = 0;
                windDirectionVector.X = 0;
                windDirectionVector.SafeNormalize();
                animDir = "DD";
                break;
        }
        sprite.Play(animCount + animDir);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level = SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        hardVerticalHitSoundCooldown -= Engine.DeltaTime;
        windCooldown -= Engine.DeltaTime;
        base.Depth = 100;
        Player player = base.Scene.Tracker.GetEntity<Player>();
        ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
        if (windController == null)
        {
            windController = new ExtendedWindController(Pattern);
            base.Scene.Add(windController);
        }
        if (Hold.IsHeld)
        {
            prevLiftSpeed = Vector2.Zero;
            if (Input.Dash.Pressed && windCooldown <= 0 && usesRemaining != 0)
            {
                if (windDirection != WindDirections.DashDirection)
                {
                    windController.AddWind(windStrength * windDirectionVector, windDuration);
                    windCooldown = windDuration;
                    Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
                    usesRemaining--;
                    if (uses < 0) { animCount = "I"; }
                    else if (usesRemaining == 0) { animCount = "0"; }
                    else if (usesRemaining == 1) { animCount = "1"; }
                    else if (usesRemaining == 2) { animCount = "2"; }
                    else if (usesRemaining == 3) { animCount = "3"; }
                    else { animCount = "3"; }
                    sprite.Play(animCount + animDir);
                }
                else if(Input.MoveX != 0 || Input.MoveY != 0)
                {
                    windDirectionVector.X = Input.MoveX;
                    windDirectionVector.Y = Input.MoveY;
                    windDirectionVector.SafeNormalize();
                    windController.AddWind(windStrength * windDirectionVector, windDuration);
                    windCooldown = windDuration;
                    Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
                    usesRemaining--;
                    if (uses < 0) { animCount = "I"; }
                    else if (usesRemaining == 0) { animCount = "0"; }
                    else if (usesRemaining == 1) { animCount = "1"; }
                    else if (usesRemaining == 2) { animCount = "2"; }
                    else if (usesRemaining == 3) { animCount = "3"; }
                    else { animCount = "3"; }
                    sprite.Play(animCount + animDir);
                }
            }
            else if (player.OnGround())
            {
                usesRemaining = uses;
                if (uses < 0) { animCount = "I"; }
                else if (usesRemaining == 0) { animCount = "0"; }
                else if (usesRemaining == 1) { animCount = "1"; }
                else if (usesRemaining == 2) { animCount = "2"; }
                else if (usesRemaining == 3) { animCount = "3"; }
                else { animCount = "3"; }
                sprite.Play(animCount + animDir);
            }
        }
        else
        {
            if (OnGround())
            {
                usesRemaining = uses;
                if (uses < 0) { animCount = "I"; }
                else if (usesRemaining == 0) { animCount = "0"; }
                else if (usesRemaining == 1) { animCount = "1"; }
                else if (usesRemaining == 2) { animCount = "2"; }
                else if (usesRemaining == 3) { animCount = "3"; }
                else { animCount = "3"; }
                sprite.Play(animCount + animDir);
                float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                Vector2 liftSpeed = base.LiftSpeed;
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
            previousPosition = base.ExactPosition;
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (base.Center.X > (float)Level.Bounds.Right)
            {
                MoveH(32f * Engine.DeltaTime);
                if (base.Left - 8f > (float)Level.Bounds.Right)
                {
                    RemoveSelf();
                }
            }
            else if (base.Left < (float)Level.Bounds.Left)
            {
                base.Left = Level.Bounds.Left;
                Speed.X *= -0.4f;
            }
            else if (base.Top < (float)(Level.Bounds.Top - 4))
            {
                base.Top = Level.Bounds.Top + 4;
                Speed.Y = 0f;
            }
            else if (base.Top > (float)(Level.Bounds.Bottom + 16))
            {
                RemoveSelf();
                return;
            }
            if (base.X < (float)(Level.Bounds.Left + 10))
            {
                MoveH(32f * Engine.DeltaTime);
            }
            TempleGate templeGate = CollideFirst<TempleGate>();
            if (templeGate != null && player != null)
            {
                templeGate.Collidable = false;
                MoveH((float)(Math.Sign(player.X - base.X) * 32) * Engine.DeltaTime);
                templeGate.Collidable = true;
            }
            Hold.CheckAgainstColliders();
        }
    }

    public void ExplodeLaunch(Vector2 from)
    {
        if (!Hold.IsHeld)
        {
            Speed = (base.Center - from).SafeNormalize(120f);
            SlashFx.Burst(base.Center, Speed.Angle());
        }
    }

    public bool Dangerous(HoldableCollider holdableCollider)
    {
        if (!Hold.IsHeld && Speed != Vector2.Zero)
        {
            return hitSeeker != holdableCollider;
        }
        return false;
    }

    public void HitSeeker(Seeker seeker)
    {
        if (!Hold.IsHeld)
        {
            Speed = (base.Center - seeker.Center).SafeNormalize(120f);
        }
        Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Position);
    }

    public void HitSpinner(Entity spinner)
    {
        if (!Hold.IsHeld && Speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (previousPosition - base.ExactPosition).Length() < 0.01f && OnGround())
        {
            int num = Math.Sign(base.X - spinner.X);
            if (num == 0)
            {
                num = 1;
            }
            Speed.X = (float)num * 120f;
            Speed.Y = -30f;
        }
    }

    public bool HitSpring(Spring spring)
    {
        if (!Hold.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
            {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                noGravityTimer = 0.15f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
        }
        return false;
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        }
        Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Position);
        Speed.X *= -0.4f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }
        if (Speed.Y > 0f)
        {
            if (hardVerticalHitSoundCooldown <= 0f)
            {
                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                hardVerticalHitSoundCooldown = 1f;
            }
            else
            {
                Audio.Play(SurfaceIndex.GetPathFromIndex(9) + "/landing", Position, "crystal_velocity", 0f);
            }
        }
        if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
        {
            Speed.Y *= -0.6f;
        }
        else
        {
            Speed.Y = 0f;
        }
    }

    public override bool IsRiding(Solid solid)
    {
        if (Speed.Y == 0f)
        {
            return base.IsRiding(solid);
        }
        return false;
    }

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
