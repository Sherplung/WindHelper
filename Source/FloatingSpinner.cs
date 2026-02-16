using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WindHelper.Entities;

[Tracked]
[CustomEntity("WindHelper/FloatingSpinner")]
internal class FloatingSpinner : Entity
{
    private class Border : Entity
    {
        private Entity[] drawing = new Entity[2];

        public Border(Entity parent, Entity filler)
        {
            drawing[0] = parent;
            drawing[1] = filler;
            base.Depth = parent.Depth + 2;
        }

        public override void Render()
        {
            if (drawing[0].Visible)
            {
                DrawBorder(drawing[0]);
                DrawBorder(drawing[1]);
            }
        }

        private void DrawBorder(Entity entity)
        {
            if (entity == null)
            {
                return;
            }
            foreach (Component component in entity.Components)
            {
                if (component is Image { Color: var color, Position: var position } image)
                {
                    image.Color = Microsoft.Xna.Framework.Color.Black;
                    image.Position = position + new Vector2(0f, -1f);
                    image.Render();
                    image.Position = position + new Vector2(0f, 1f);
                    image.Render();
                    image.Position = position + new Vector2(-1f, 0f);
                    image.Render();
                    image.Position = position + new Vector2(1f, 0f);
                    image.Render();
                    image.Color = color;
                    image.Position = position;
                }
            }
        }
    }

    public static ParticleType P_Move;

    public const float ParticleInterval = 0.02f;

    private Entity filler;

    private Border border;

    private float offset;

    private bool expanded;

    private int randomSeed;

    public int ID;

    public float Mass;

    public bool lockX;

    public bool lockY;

    public string enableFlag;

    public string disableFlag;

    private Level level;

    public FloatingSpinner(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        ID = data.ID;
        base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Add(new PlayerCollider(OnPlayer));
        Add(new HoldableCollider(OnHoldable));
        Add(new LedgeBlocker());
        Add(new WindMover(Move));
        base.Depth = -8502;
        Mass = data.Float("mass", 1f);
        randomSeed = Calc.Random.Next();
        lockX = data.Bool("lockX", false);
        lockY = data.Bool("lockY", false);
        enableFlag = data.Attr("enableFlag", "");
        disableFlag = data.Attr("disableFlag", "");
    }

    public float GetMass()
    {
        return Mass;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        level = SceneAs<Level>();
        ForceInstantiate();
    }
    public void ForceInstantiate()
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
            if (base.Scene.OnInterval(0.25f, offset) && !InView())
            {
                Visible = false;
            }
            if (base.Scene.OnInterval(0.05f, offset))
            {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    Collidable = Math.Abs(entity.X - base.X) < 128f && Math.Abs(entity.Y - base.Y) < 128f;
                }
            }
        }
        if (filler != null)
        {
            filler.Position = Position;
        }
    }

    private bool InView()
    {
        Camera camera = (base.Scene as Level).Camera;
        if (base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f)
        {
            return base.Y < camera.Y + 180f + 16f;
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
        if (!SolidCheck(new Vector2(base.X - 4f, base.Y - 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f));
        }
        if (!SolidCheck(new Vector2(base.X + 4f, base.Y - 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f));
        }
        if (!SolidCheck(new Vector2(base.X + 4f, base.Y + 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f));
        }
        if (!SolidCheck(new Vector2(base.X - 4f, base.Y + 4f)))
        {
            Add(new Image(mTexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f));
        }
        foreach (FloatingSpinner entity in base.Scene.Tracker.GetEntities<FloatingSpinner>())
        {
            if (entity.ID > ID && entity.Mass == Mass && entity.lockX == lockX && entity.lockY == lockY && entity.enableFlag.Equals(enableFlag) && entity.disableFlag.Equals(disableFlag) && (entity.Position - Position).LengthSquared() < 576f)
            {
                AddSprite((Position + entity.Position) / 2f - Position);
            }
        }
        base.Scene.Add(border = new Border(this, filler));
        expanded = true;
        Calc.PopRandom();
    }

    private void AddSprite(Vector2 offset)
    {
        if (filler == null)
        {
            base.Scene.Add(filler = new Entity(Position));
            filler.Depth = base.Depth + 1;
        }
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("Sherplung/WindHelper/FloatingSpinner/bg_FloatingSpinner");
        Image image = new Image(Calc.Random.Choose(atlasSubtextures));
        image.Position = offset;
        image.Rotation = (float)Calc.Random.Choose(0, 1, 2, 3) * (MathF.PI / 2f);
        image.CenterOrigin();
        filler.Add(image);
    }

    private bool SolidCheck(Vector2 position)
    {
        foreach (Solid item in base.Scene.CollideAll<Solid>(position))
        {
            if (item is SolidTiles)
            {
                return true;
            }
        }
        return false;
    }

    private void ClearSprites()
    {
        if (filler != null)
        {
            filler.RemoveSelf();
        }
        filler = null;
        if (border != null)
        {
            border.RemoveSelf();
        }
        border = null;
        foreach (Image item in base.Components.GetAll<Image>())
        {
            item.RemoveSelf();
        }
        expanded = false;
    }

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
        if (filler != null && filler.Scene == scene)
        {
            filler.RemoveSelf();
        }
        if (border != null && border.Scene == scene)
        {
            border.RemoveSelf();
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
                    base.Position.X += strength.X / Mass;
                }
                if (!lockY)
                {
                    base.Position.Y += strength.Y / Mass;
                }
            }
        }
    }
}
