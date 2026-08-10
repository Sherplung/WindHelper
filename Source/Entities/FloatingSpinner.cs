namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/FloatingSpinner")]
[UsedImplicitly]
[Tracked]
internal class FloatingSpinner : Entity
{
    private readonly float Mass;
    private readonly int randomSeed;
    private readonly bool lockX;
    private readonly bool lockY;
    private readonly string enableFlag;
    private readonly string disableFlag;
    private readonly int ID;

    private Entity filler;
    private CrystalStaticSpinner.Border border;
    private bool expanded;

    private Level level;

    public FloatingSpinner(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Mass = data.Float("mass", 1f);
        randomSeed = Calc.Random.Next();
        lockX = data.Bool("lockX");
        lockY = data.Bool("lockY");
        enableFlag = data.Attr("enableFlag");
        disableFlag = data.Attr("disableFlag");
        ID = data.ID;
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Add(new PlayerCollider(OnPlayer));
        Add(new HoldableCollider(OnHoldable));
        Add(new LedgeBlocker());
        Add(new WindMover(Move));
        Depth = -8502;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        level = SceneAs<Level>();
        ForceInstantiate();
    }

    private void ForceInstantiate()
    {
        CreateSprites();
        Visible = true;
    }

    public override void Update()
    {
        if (!Visible)
        {
            Collidable = false;
            if (InView())
            {
                Visible = true;
                if (!expanded)
                {
                    CreateSprites();
                }
            }
        }
        else
        {
            base.Update();
            if (Scene.OnInterval(0.25f) && !InView())
            {
                Visible = false;
            }
            if (Scene.OnInterval(0.05f))
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    Collidable = Math.Abs(entity.X - X) < 128f && Math.Abs(entity.Y - Y) < 128f;
                }
            }
        }
        filler?.Position = Position;
    }

    private bool InView()
    {
        Camera camera = ((Level)Scene).Camera;
        if (X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + Utils.GameplayBufferWidth + 16f)
        {
            return Y < camera.Y + Utils.GameplayBufferHeight + 16f;
        }
        return false;
    }

    private void CreateSprites()
    {
        if (expanded)
        {
            return;
        }
        Calc.PushRandom(randomSeed);
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("Sherplung/WindHelper/FloatingSpinner/fg_FloatingSpinner");
        MTexture mTexture = Calc.Random.Choose(atlasSubtextures);
        if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f));
        }
        if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f));
        }
        if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f));
        }
        if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f));
        }
        foreach (FloatingSpinner entity in Scene.Tracker.GetEntities<FloatingSpinner>().Cast<FloatingSpinner>())
        {
            if (entity.ID > ID && entity.Mass.nearlyEqual(Mass) && entity.lockX == lockX && entity.lockY == lockY && entity.enableFlag.Equals(enableFlag) && entity.disableFlag.Equals(disableFlag) &&
                (entity.Position - Position).LengthSquared() < 576f)
            {
                AddSprite((Position + entity.Position) / 2f - Position);
            }
        }
        Scene.Add(border = new CrystalStaticSpinner.Border(this, filler));
        expanded = true;
        Calc.PopRandom();
    }

    private void AddSprite(Vector2 offset)
    {
        if (filler == null)
        {
            Scene.Add(filler = new Entity(Position));
            filler.Depth = Depth + 1;
        }
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("Sherplung/WindHelper/FloatingSpinner/bg_FloatingSpinner");
        Image image = new Image(Calc.Random.Choose(atlasSubtextures))
        {
            Position = offset,
            Rotation = Calc.Random.Choose(0, 1, 2, 3) * (MathF.PI / 2f)
        };
        image.CenterOrigin();
        filler.Add(image);
    }

    private bool SolidCheck(Vector2 position) => Scene.CollideAll<Solid>(position).OfType<SolidTiles>().Any();

    private void OnPlayer(Player player)
    {
        player.Die((player.Position - Position).SafeNormalize());
    }

    private void OnHoldable(Holdable h)
    {
        h.HitSpinner(this);
    }

    public override void Removed(Scene scene)
    {
        if (filler?.Scene == scene)
        {
            filler?.RemoveSelf();
        }
        if (border?.Scene == scene)
        {
            border?.RemoveSelf();
        }
        base.Removed(scene);
    }

    private void Move(Vector2 strength)
    {
        if (string.IsNullOrEmpty(enableFlag) || level.Session.GetFlag(enableFlag))
        {
            if (string.IsNullOrEmpty(disableFlag) || !level.Session.GetFlag(disableFlag))
            {
                if (!lockX)
                {
                    Position.X += strength.X / Mass;
                }
                if (!lockY)
                {
                    Position.Y += strength.Y / Mass;
                }
            }
        }
    }
}