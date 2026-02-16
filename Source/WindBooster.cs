using Celeste;
using Celeste.Editor;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Celeste.WindController;

namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/WindBooster")]

internal class WindBooster : Booster 
{

    private float windStrength;

    private Sprite spriteFG;

    private Sprite spriteBG;

    public WindController.Patterns Pattern;

    private Level level;

    private bool wasBoosting = false;

    public WindBooster(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Bool("red", false))
    {
        windStrength = data.Float("windStrength", 400f);
        Remove(sprite);
        Add(spriteBG = GFX.SpriteBank.Create("Sherplung_WindHelper_windBoosterBG"));
        Add(sprite = GFX.SpriteBank.Create(red ? "boosterRed" : "booster"));
        Add(spriteFG = GFX.SpriteBank.Create("Sherplung_WindHelper_windBoosterFG"));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Image image = new Image(GFX.Game["objects/booster/outline"]);
        image.CenterOrigin();
        image.Color = Color.White * 0.75f;
        outline = new Entity(Position);
        outline.Depth = 8999;
        outline.Visible = false;
        outline.Add(image);
        outline.Add(new MirrorReflection());
        scene.Add(outline);
        level = SceneAs<Level>();
    }

    public new void PlayerBoosted(Player player, Vector2 direction)
    {
        Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_dash" : "event:/game/04_cliffside/greenbooster_dash", Position);
        if (red)
        {
            loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
            loopingSfx.DisposeOnTransition = false;
        }
        if (Ch9HubBooster && direction.Y < 0f)
        {
            bool flag = true;
            List<LockBlock> list = base.Scene.Entities.FindAll<LockBlock>();
            if (list.Count > 0)
            {
                foreach (LockBlock item in list)
                {
                    if (!item.UnlockingRegistered)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                Ch9HubTransition = true;
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, [MethodImpl(MethodImplOptions.NoInlining)] () =>
                {
                    Add(new SoundSource("event:/new_content/timeline_bubble_to_remembered")
                    {
                        DisposeOnTransition = false
                    });
                }, 2f, start: true));
            }
        }
        BoostingPlayer = true;
        base.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
        sprite.Play("spin");
        sprite.FlipX = player.Facing == Facings.Left;
        outline.Visible = true;
        wiggler.Start();
        dashRoutine.Replace(BoostRoutine(player, direction));
    }

    public new void PlayerReleased()
    {
        Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_end" : "event:/game/04_cliffside/greenbooster_end", sprite.RenderPosition);
        sprite.Play("pop");
        cannotUseTimer = 0f;
        respawnTimer = 1f;
        BoostingPlayer = false;
        wiggler.Stop();
        loopingSfx.Stop();
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public void base_Update()
    {
    }

    public override void Update()
    {
        base_Update();
        if (cannotUseTimer > 0f)
        {
            cannotUseTimer -= Engine.DeltaTime;
        }
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        if (!dashRoutine.Active && respawnTimer <= 0f)
        {
            Vector2 target = Vector2.Zero;
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && CollideCheck(entity))
            {
                target = entity.Center + playerOffset - Position;
            }
            sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
        }
        if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>())
        {
            sprite.Play("loop");
        }
        if (BoostingPlayer && !wasBoosting)
        {
            ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
            if (windController == null)
            {
                windController = new ExtendedWindController(Pattern);
                base.Scene.Add(windController);
            }
            windController.ChangeControllableWind(windStrength, true);
        }
        else if (!BoostingPlayer && wasBoosting)
        {
            ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
            if (windController == null)
            {
                windController = new ExtendedWindController(Pattern);
                base.Scene.Add(windController);
            }
            windController.ChangeControllableWind(windStrength, false);
        }
        wasBoosting = BoostingPlayer;
        spriteFG.Position = sprite.Position;
        spriteBG.Position = sprite.Position;
        if (sprite.currentAnimation == sprite.animations["loop"] || sprite.currentAnimation == sprite.animations["spin"] || sprite.currentAnimation == sprite.animations["inside"])
        {
            spriteFG.Visible = sprite.Visible;
            spriteBG.Visible = sprite.Visible;
        }
        else 
        {
            spriteFG.Visible = false;
            spriteBG.Visible = false;
        }
        
    }
}
