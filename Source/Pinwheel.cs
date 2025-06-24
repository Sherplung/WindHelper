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

    private const float RespawnTime = 0.6f;

    private Sprite sprite;

    private float respawnTimer;

    private float windTimer;

    private Vector2 hitDir;

    private float windStrength;

    private float windDuration;

    private Level level;

    public WindController.Patterns Pattern;

    private SpriteBank bank;

    public Pinwheel(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        windStrength = data.Float("wind_strength");
        windDuration = data.Float("wind_duration");

        Collider = new Circle(12f);
        Add(new PlayerCollider(OnPlayer));
        sprite = GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelBlue");
        Add(sprite);
        sprite.Play("idle", false, true);
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
                sprite.Play("spinDown");
                Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
            }
        }

    }

    public override void Render()
    {
        base.Render();
    }
    private void OnPlayer(Player player)
    {
        if (respawnTimer <= 0f)
        {
            hitDir = (player.Center - base.Center);
            hitDir.Normalize();
            ExtendedWindController windController = base.Scene.Entities.FindFirst<ExtendedWindController>();
            if (windController == null)
            {
                windController = new ExtendedWindController(Pattern);
                base.Scene.Add(windController);
            }
            windController.AddWind((hitDir * windStrength), windDuration);
            //Audio.Play("event:/game/general/thing_booped", Position);
            Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
            respawnTimer = windDuration;
            windTimer = windDuration;
            Vector2 vector2 = player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
            sprite.Play("fast");
            SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
        }
    }
}
