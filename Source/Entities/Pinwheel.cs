namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/Pinwheel")]
[UsedImplicitly]
public class Pinwheel : Entity
{
    private enum BehaviorTypes
    {
        AnyAngle = 0,
        Cardinals = 1,
        Diagonals = 2,
        EightWay = 3
    }

    private readonly float windStrength;
    private readonly float windDuration;
    private int uses;
    private readonly BehaviorTypes behavior;
    private readonly Sprite sprite;

    private Vector2 hitDir;
    private Vector2 trueHitDir;
    private float respawnTimer;

    public Pinwheel(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = 1;
        windStrength = data.Float("wind_strength", 400f);
        windDuration = data.Float("wind_duration", 1f);
        uses = data.Int("uses", -1);
        behavior = data.Enum<BehaviorTypes>("behaviorType");
        Collider = new Circle(12f);
        Add(new PlayerCollider(OnPlayer));
        sprite = behavior switch
        {
            BehaviorTypes.AnyAngle => GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelBlue"),
            BehaviorTypes.Cardinals => GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelRedWhite"),
            BehaviorTypes.Diagonals => GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelGreenWhite"),
            BehaviorTypes.EightWay => GFX.SpriteBank.Create("Sherplung_WindHelper_pinwheelRedGreen"),
            _ => throw new Exception("Impossible Enum Value! How did you do that?")
        };
        Add(sprite);
        sprite.Play(uses == 0 ? "disabled" : "idle", false, true);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        sprite.Visible = true;
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
                sprite.Play(uses == 0 ? "spinDownFinal" : "spinDown");
                Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
            }
        }
    }

    public override void Render()
    {
        base.Render();
        if (uses <= 0) return;

        Vector2 justification = new Vector2(0.5f, 0.5f);
        ActiveFont.Draw(uses.ToString(), Position, justification, Vector2.One * 0.3f, Color.White, 0f, Color.Black, 1f, Color.Black);
    }

    private void OnPlayer(Player player)
    {
        if (!(respawnTimer <= 0f) || uses == 0) return;

        trueHitDir = player.Center - Center;
        hitDir = behavior switch
        {
            BehaviorTypes.AnyAngle => trueHitDir.SafeNormalize(),
            BehaviorTypes.Cardinals => trueHitDir.FourWayNormal(),
            BehaviorTypes.Diagonals => trueHitDir switch
            {
                { X: >= 0, Y: >= 0 } => new Vector2(1, 1).SafeNormalize(),
                { X: >= 0, Y: <= 0 } => new Vector2(1, -1).SafeNormalize(),
                { X: <= 0, Y: <= 0 } => new Vector2(-1, -1).SafeNormalize(),
                { X: <= 0, Y: >= 0 } => new Vector2(-1, 1).SafeNormalize(),
                _ => trueHitDir.SafeNormalize()
            },
            BehaviorTypes.EightWay => trueHitDir.EightWayNormal(),
            _ => throw new Exception("Impossible Enum Value! How did you do that?")
        };
        Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
        windController.AddWind(hitDir * windStrength, windDuration);
        Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
        respawnTimer = windDuration;
        player.ExplodeLaunch(Position, false, false);
        SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
        sprite.Play("fast");
        if (uses <= 0) return;
        uses--;
    }
}