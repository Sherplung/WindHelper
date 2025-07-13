using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/FloatingBlock")]
[Tracked(false)]

internal class FloatingBlock : Solid
{
    private TileGrid tiles;

    private char tileType;

    private FloatingBlock master;

    private bool awake;

    //public bool sticky;

    public List<FloatingBlock> Group;

    public List<JumpThru> Jumpthrus;

    public Dictionary<Platform, Vector2> Moves;

    public Point GroupBoundsMin;

    public Point GroupBoundsMax;

    private float Mass;

    private bool lockX;

    private bool lockY;

    private Level level;

    public bool HasGroup { get; private set; }

    public bool MasterOfGroup { get; private set; }

    public FloatingBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        this.tileType = data.Char("tiletype", '3');
        base.Depth = -9000;
        Add(new LightOcclude());
        Add(new WindMover(Move));
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        Mass = data.Float("mass", 1f);
        lockX = data.Bool("lockX", false);
        lockY = data.Bool("lockY", false);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        awake = true;
        level = SceneAs<Level>();
        if (!HasGroup)
        {
            MasterOfGroup = true;
            Moves = new Dictionary<Platform, Vector2>();
            Group = new List<FloatingBlock>();
            Jumpthrus = new List<JumpThru>();
            GroupBoundsMin = new Point((int)base.X, (int)base.Y);
            GroupBoundsMax = new Point((int)base.Right, (int)base.Bottom);
            AddToGroupAndFindChildren(this);
            _ = base.Scene;
            Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
            VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
            foreach (FloatingBlock item in Group)
            {
                int num = (int)(item.X / 8f) - rectangle.X;
                int num2 = (int)(item.Y / 8f) - rectangle.Y;
                int num3 = (int)(item.Width / 8f);
                int num4 = (int)(item.Height / 8f);
                for (int i = num; i < num + num3; i++)
                {
                    for (int j = num2; j < num2 + num4; j++)
                    {
                        virtualMap[i, j] = tileType;
                    }
                }
            }
            tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
            {
                EdgesExtend = false,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = false
            }).TileGrid;
            tiles.Position = new Vector2((float)GroupBoundsMin.X - base.X, (float)GroupBoundsMin.Y - base.Y);
            Add(tiles);
        }
        TryToInitPosition();
    }

    private void TryToInitPosition()
    {
        if (MasterOfGroup)
        {
            foreach (FloatingBlock item in Group)
            {
                if (!item.awake)
                {
                    return;
                }
            }
        }
        else
        {
            master.TryToInitPosition();
        }
    }

    private void AddToGroupAndFindChildren(FloatingBlock from)
    {
        if (from.X < (float)GroupBoundsMin.X)
        {
            GroupBoundsMin.X = (int)from.X;
        }
        if (from.Y < (float)GroupBoundsMin.Y)
        {
            GroupBoundsMin.Y = (int)from.Y;
        }
        if (from.Right > (float)GroupBoundsMax.X)
        {
            GroupBoundsMax.X = (int)from.Right;
        }
        if (from.Bottom > (float)GroupBoundsMax.Y)
        {
            GroupBoundsMax.Y = (int)from.Bottom;
        }
        from.HasGroup = true;
        from.OnDashCollide = OnDash;
        Group.Add(from);
        Moves.Add(from, from.Position);
        if (from != this)
        {
            from.master = this;
        }
        foreach (JumpThru item in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)))
        {
            if (!Jumpthrus.Contains(item))
            {
                AddJumpThru(item);
            }
        }
        foreach (JumpThru item2 in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)))
        {
            if (!Jumpthrus.Contains(item2))
            {
                AddJumpThru(item2);
            }
        }
        /*if (sticky)
        {
            foreach (FloatingBlock entity in base.Scene.Tracker.GetEntities<FloatingBlock>())
            {
                if (entity.sticky == true && !entity.HasGroup && entity.tileType == tileType && (base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), entity) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), entity)))
                {
                    AddToGroupAndFindChildren(entity);
                }
            }
        }*/
    }
    private void AddJumpThru(JumpThru jp)
    {
        jp.OnDashCollide = OnDash;
        Jumpthrus.Add(jp);
        Moves.Add(jp, jp.Position);
        foreach (FloatingBlock entity in base.Scene.Tracker.GetEntities<FloatingBlock>())
        {
            if (!entity.HasGroup && entity.tileType == tileType && base.Scene.CollideCheck(new Rectangle((int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height), entity))
            {
                AddToGroupAndFindChildren(entity);
            }
        }
    }

    private DashCollisionResults OnDash(Player player, Vector2 direction)
    {
        return DashCollisionResults.NormalOverride;
    }

    public override void Update()
    {
        base.Update();
        if (MasterOfGroup)
        {
            bool flag = false;
            foreach (FloatingBlock item in Group)
            {
                if (item.HasPlayerRider())
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                foreach (JumpThru jumpthru in Jumpthrus)
                {
                    if (jumpthru.HasPlayerRider())
                    {
                        flag = true;
                        break;
                    }
                }
            }
        }
        LiftSpeed = Vector2.Zero;
    }


    public override void OnShake(Vector2 amount)
    {
        if (!MasterOfGroup)
        {
            return;
        }
        base.OnShake(amount);
        tiles.Position += amount;
        foreach (JumpThru jumpthru in Jumpthrus)
        {
            foreach (Component component in jumpthru.Components)
            {
                if (component is Image image)
                {
                    image.Position += amount;
                }
            }
        }
    }

    private void Move(Vector2 strength)
    {
        Vector2 origpos = this.Position;
        if (!lockX)
        {
            this.MoveHCollideSolidsAndBounds(level, strength.X / Mass, false);
        }
        if (!lockY)
        {
            this.MoveVCollideSolidsAndBounds(level, strength.Y / Mass, false, checkBottom: true);
        }
        Vector2 newpos = this.Position;
        //if(MasterOfGroup)
        //{
        /*foreach(FloatingBlock item in Group)
        {
            item.MoveHCollideSolidsAndBounds(level, strength.X / Mass, false);
            item.MoveVCollideSolidsAndBounds(level, strength.Y / Mass, false, checkBottom:true);
        }*/
        foreach (JumpThru jumpthru in Jumpthrus)
            {
                jumpthru.MoveH(newpos.X - origpos.X);
                jumpthru.MoveV(newpos.Y - origpos.Y);
            }
        //}
    }
}
