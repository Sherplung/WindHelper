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
using static Celeste.WindController;
using System.Collections;

namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/Pinwheel")]
public class Pinwheel : Entity
{
    public static ParticleType P_Ambience;

    public static ParticleType P_Launch;

    public enum BehaviorTypes
    {
        AnyAngle = 0,
        Cardinals = 1
    }

    private BehaviorTypes behavior;

    private Sprite sprite;

    private float respawnTimer;

    private float windTimer;

    private Vector2 trueHitDir;

    private Vector2 hitDir;

    private float windStrength;

    private float windDuration;

    private int uses;

    private string displayText;

    private Level level;

    public WindController.Patterns Pattern;

    private SpriteBank bank;

    public Pinwheel(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Depth = 1;
        windStrength = data.Float("wind_strength", 400f);
        windDuration = data.Float("wind_duration", 1f);
        uses = data.Int("uses", -1);
        behavior = data.Enum<BehaviorTypes>("behaviorType", BehaviorTypes.AnyAngle);
        Collider = new Circle(12f);
        Add(new PlayerCollider(OnPlayer));
        switch (behavior)
        {
            case BehaviorTypes.AnyAngle:
                sprite = GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelBlue");
                break;
            case BehaviorTypes.Cardinals:
                sprite = GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelRedWhite");
                break;
        }
        Add(sprite);
        if (uses == 0)
        {
            sprite.Play("disabled", false, true);
        }
        else
        {
            sprite.Play("idle", false, true);
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        sprite.Visible = true;
        level = SceneAs<Level>();
    }

    public override void Update()
    {
        //entity updates
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                if (uses == 0) { sprite.Play("spinDownFinal"); }
                else { sprite.Play("spinDown"); }
                Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
            }
        }

    }

    public override void Render()
    {
        base.Render();
        if (uses > 0)
        {
            Vector2 justification = new Vector2(0.5f, 0.5f);
            ActiveFont.Draw(uses.ToString(), Position, justification, Vector2.One * 0.3f, Color.White, 0f, Color.Black, 1f, Color.Black);
        }
    }
    private void OnPlayer(Player player)
    {
        if (respawnTimer <= 0f && uses != 0)
        {
            trueHitDir = (player.Center - base.Center);
            switch (behavior)
            {
                case BehaviorTypes.AnyAngle:
                    hitDir = new Vector2(trueHitDir.X, trueHitDir.Y);
                    break;
                case BehaviorTypes.Cardinals:
                    if (Math.Abs(trueHitDir.X) >= Math.Abs(trueHitDir.Y))
                    {
                        hitDir = new Vector2(trueHitDir.X, 0f);
                    }
                    else
                    {
                        hitDir = new Vector2(0f, trueHitDir.Y);
                    }
                    break;
            }
            hitDir.Normalize();
            ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
            if (windController == null)
            {
                windController = new ExtendedWindController(Pattern);
                base.Scene.Add(windController);
            }
            windController.AddWind((hitDir * windStrength), windDuration);
            Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
            respawnTimer = windDuration;
            windTimer = windDuration;
            Vector2 vector2 = player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
            SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
            sprite.Play("fast");
            if (uses > 0) { uses--; }
        }
    }
}
