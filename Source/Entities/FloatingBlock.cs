namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/FloatingBlock")]
[UsedImplicitly]
[Tracked]
internal class FloatingBlock : Solid
{
    private readonly char tileType;
    private readonly float Mass;
    private readonly bool lockX;
    private readonly bool lockY;
    private readonly string enableFlag;
    private readonly string disableFlag;

    private FloatingBlock master;
    private List<FloatingBlock> Group;
    private List<JumpThru> Jumpthrus;

    private TileGrid tiles;

    //private Dictionary<Platform, Vector2> Moves;
    private bool HasGroup {get; set;}
    private bool MasterOfGroup {get; set;}
    private Point GroupBoundsMin;
    private Point GroupBoundsMax;

    private Level level;
    private bool awake;
    //public bool sticky;

    public FloatingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        tileType = data.Char("tiletype", '3');
        Mass = data.Float("mass", 1f);
        lockX = data.Bool("lockX");
        lockY = data.Bool("lockY");
        enableFlag = data.Attr("enableFlag", null);
        disableFlag = data.Attr("disableFlag", null);
        Add(new LightOcclude());
        Add(new WindMover(Move));
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        Depth = -9000;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        awake = true;
        level = SceneAs<Level>();
        if (!HasGroup)
        {
            MasterOfGroup = true;
            //Moves = new Dictionary<Platform, Vector2>();
            Group = [];
            Jumpthrus = [];
            GroupBoundsMin = new Point((int)X, (int)Y);
            GroupBoundsMax = new Point((int)Right, (int)Bottom);
            AddToGroupAndFindChildren(this);
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
            tiles.Position = new Vector2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);
            Add(tiles);
        }
        TryToInitPosition();
        if (CollideCheck<Player>())
        {
            foreach (JumpThru jumpThru in Jumpthrus)
            {
                jumpThru.RemoveSelf();
            }
            DestroyStaticMovers();
            RemoveSelf();
        }
    }

    public override void Update()
    {
        base.Update();
        if (MasterOfGroup)
        {
            bool flag = Group.Any(item => item.HasPlayerRider());
            if (!flag)
            {
                if (Jumpthrus.Any(jumpthru => jumpthru.HasPlayerRider()))
                {
                    //flag = true;
                }
            }
        }
        LiftSpeed = Vector2.Zero;
    }

    private void TryToInitPosition()
    {
        if (MasterOfGroup)
        {
            if (Group.Any(item => !item.awake))
            {
            }
        }
        else
        {
            master.TryToInitPosition();
        }
    }

    private void AddToGroupAndFindChildren(FloatingBlock from)
    {
        if (from.X < GroupBoundsMin.X)
        {
            GroupBoundsMin.X = (int)from.X;
        }
        if (from.Y < GroupBoundsMin.Y)
        {
            GroupBoundsMin.Y = (int)from.Y;
        }
        if (from.Right > GroupBoundsMax.X)
        {
            GroupBoundsMax.X = (int)from.Right;
        }
        if (from.Bottom > GroupBoundsMax.Y)
        {
            GroupBoundsMax.Y = (int)from.Bottom;
        }
        from.HasGroup = true;
        from.OnDashCollide = OnDash;
        Group.Add(from);
        //Moves.Add(from, from.Position);
        if (from != this)
        {
            from.master = this;
        }
        foreach (JumpThru item in Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)).Where(item => !Jumpthrus.Contains(item)))
        {
            AddJumpThru(item);
        }
        foreach (JumpThru item2 in Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)).Where(item2 => !Jumpthrus.Contains(item2)))
        {
            AddJumpThru(item2);
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
        //Moves.Add(jp, jp.Position);
        foreach (FloatingBlock entity in Scene.Tracker.GetEntities<FloatingBlock>().Cast<FloatingBlock>())
        {
            if (!entity.HasGroup && entity.tileType == tileType && Scene.CollideCheck(new Rectangle((int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height), entity))
            {
                AddToGroupAndFindChildren(entity);
            }
        }
    }

    private static DashCollisionResults OnDash(Player player, Vector2 direction) => DashCollisionResults.NormalOverride;

    public override void OnShake(Vector2 amount)
    {
        if (!MasterOfGroup)
        {
            return;
        }
        base.OnShake(amount);
        tiles.Position += amount;
        foreach (Component component in Jumpthrus.SelectMany(jumpthru => jumpthru.Components))
        {
            if (component is Image image)
            {
                image.Position += amount;
            }
        }
    }

    private void Move(Vector2 strength)
    {
        Vector2 origpos = Position;
        if (string.IsNullOrEmpty(enableFlag) || level.Session.GetFlag(enableFlag))
        {
            if (string.IsNullOrEmpty(disableFlag) || !level.Session.GetFlag(disableFlag))
            {
                if (!lockX)
                {
                    MoveHCollideSolidsAndBounds(level, strength.X / Mass, false);
                }
                if (!lockY)
                {
                    MoveVCollideSolidsAndBounds(level, strength.Y / Mass, false, checkBottom: true);
                }
            }
        }
        Vector2 newpos = Position;
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