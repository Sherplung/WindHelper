namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/WindBooster")]
[UsedImplicitly]
internal class WindBooster : Booster
{
    private readonly float windStrength;
    private readonly bool dashBased;
    private readonly float windDuration;
    private readonly bool OneUse;

    private readonly Sprite spriteFG;
    private readonly Sprite spriteBG;

    private bool wasBoosting;

    public WindBooster(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("red"))
    {
        windStrength = data.Float("windStrength", 400f);
        dashBased = data.Bool("dashBased");
        windDuration = data.Float("windDuration", 1f);
        OneUse = data.Bool("oneUse");
        Remove(sprite);
        Add(spriteBG = GFX.SpriteBank.Create("Sherplung_WindHelper_windBoosterBG"));
        Add(sprite = GFX.SpriteBank.Create(red ? "boosterRed" : "booster"));
        Add(spriteFG = GFX.SpriteBank.Create("Sherplung_WindHelper_windBoosterFG"));
    }

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Booster.PlayerBoosted += PlayerBoosted;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Booster.PlayerBoosted -= PlayerBoosted;
    }

    private static void PlayerBoosted(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
    {
        if (self is not WindBooster windBooster)
        {
            orig(self, player, direction);
            return;
        }

        Audio.Play(windBooster.red ? "event:/game/05_mirror_temple/redbooster_dash" : "event:/game/04_cliffside/greenbooster_dash", windBooster.Position);
        if (windBooster.red)
        {
            windBooster.loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
            windBooster.loopingSfx.DisposeOnTransition = false;
        }
        if (windBooster.Ch9HubBooster && direction.Y < 0f)
        {
            bool flag = true;
            List<LockBlock> list = windBooster.Scene.Entities.FindAll<LockBlock>();
            if (list.Count > 0)
            {
                if (list.Any(item => !item.UnlockingRegistered))
                {
                    flag = false;
                }
            }
            if (flag)
            {
                windBooster.Ch9HubTransition = true;
                windBooster.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
                {
                    windBooster.Add(new SoundSource("event:/new_content/timeline_bubble_to_remembered")
                    {
                        DisposeOnTransition = false
                    });
                }, 2f, true));
            }
        }
        windBooster.BoostingPlayer = true;
        windBooster.Tag = Tags.Persistent | Tags.TransitionUpdate;
        windBooster.sprite.Play("spin");
        windBooster.sprite.FlipX = player.Facing == Facings.Left;
        windBooster.outline.Visible = true;
        windBooster.wiggler.Start();
        windBooster.dashRoutine.Replace(windBooster.BoostRoutine(player, direction));
        if (windBooster.dashBased)
        {
            Utils.AddExtendedWindControllerIfNone(windBooster.Scene, out ExtendedWindController windController);
            Vector2 trueDirection = new Vector2(direction.X, GravityHelperImports.InvertIfPlayerInverted(direction.Y));

            windController.AddWind(trueDirection * windBooster.windStrength, windBooster.windDuration);
        }
        if (windBooster.OneUse)
        {
            windBooster.outline.RemoveSelf();
            windBooster.Remove(windBooster.light);
            windBooster.Remove(windBooster.bloom);
        }
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    private void base_Update()
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
                if (OneUse)
                {
                    RemoveSelf();
                }
                else
                {
                    Respawn();
                }
            }
        }
        if (!dashRoutine.Active && respawnTimer <= 0f)
        {
            Vector2 target = Vector2.Zero;
            Player entity = Scene.Tracker.GetEntity<Player>();
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
        if (!dashBased)
        {
            switch (BoostingPlayer)
            {
                case true when !wasBoosting:
                {
                    Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
                    windController.ChangeControllableWind(windStrength);
                    break;
                }
                case false when wasBoosting:
                {
                    Utils.AddExtendedWindControllerIfNone(Scene, out ExtendedWindController windController);
                    windController.ChangeControllableWind(windStrength, false);
                    break;
                }
            }
            wasBoosting = BoostingPlayer;
        }
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