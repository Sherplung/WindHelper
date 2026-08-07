using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

// ReSharper disable once CheckNamespace
namespace Celeste.Mod.WindHelper.Entities;

[CustomEntity("WindHelper/FloatingDashBlock")]

public class FloatingDashBlock : Solid
{
	private TileGrid tiles;

	private readonly char tileType;

	private FloatingDashBlock master;

	private bool awake;

	//public bool sticky;

	private List<FloatingDashBlock> Group;

	private List<JumpThru> Jumpthrus;

	private Dictionary<Platform, Vector2> Moves;

	private Point GroupBoundsMin;

	private Point GroupBoundsMax;

	private readonly float Mass;

	private readonly bool lockX;

	private readonly bool lockY;

	private readonly string enableFlag;

	private readonly string disableFlag;

	private Level level;

	public bool HasGroup { get; private set; }

	public bool MasterOfGroup { get; private set; }
	
	public FloatingDashBlock(EntityData data, Vector2 offset)
		: base(data.Position + offset, data.Width, data.Height, safe: false)
	{
		tileType = data.Char("tiletype", '3');
		Depth = -12999;
		Add(new LightOcclude());
		Add(new WindMover(Move));
		SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
		Mass = data.Float("mass", 1f);
		lockX = data.Bool("lockX");
		lockY = data.Bool("lockY");
		enableFlag = data.Attr("enableFlag", null);
		disableFlag = data.Attr("disableFlag", null);
		OnDashCollide = OnDashed;
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
			Group = [];
			Jumpthrus = [];
			GroupBoundsMin = new Point((int)X, (int)Y);
			GroupBoundsMax = new Point((int)Right, (int)Bottom);
			AddToGroupAndFindChildren(this);
			_ = Scene;
			Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
			VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
			foreach (FloatingDashBlock item in Group)
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
					flag = true;
				}
			}
		}
		LiftSpeed = Vector2.Zero;
	}

	public override void Removed(Scene scene)
	{
		base.Removed(scene);
		Celeste.Freeze(0.05f);
	}

	private void Break(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true)
	{
		if (playSound)
		{
			switch (tileType)
			{
				case '1':
					Audio.Play("event:/game/general/wall_break_dirt", Position);
					break;
				case '3':
					Audio.Play("event:/game/general/wall_break_ice", Position);
					break;
				case '9':
					Audio.Play("event:/game/general/wall_break_wood", Position);
					break;
				default:
					Audio.Play("event:/game/general/wall_break_stone", Position);
					break;
			}
		}
		for (int i = 0; i < Width / 8f; i++)
		{
			for (int j = 0; j < Height / 8f; j++)
			{
				Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
			}
		}
		Collidable = false;
		
		RemoveSelf();
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
	
	private void AddToGroupAndFindChildren(FloatingDashBlock from)
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
		from.OnDashCollide = OnDashed;
		Group.Add(from);
		Moves.Add(from, from.Position);
		if (from != this)
		{
			from.master = this;
		}
		foreach (var item in Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)).Where(item => !Jumpthrus.Contains(item)))
		{
			AddJumpThru(item);
		}
		foreach (var item2 in Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)).Where(item2 => !Jumpthrus.Contains(item2)))
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
        jp.OnDashCollide = OnDashed;
        Jumpthrus.Add(jp);
        Moves.Add(jp, jp.Position);
        foreach (var entity1 in Scene.Tracker.GetEntities<FloatingDashBlock>())
        {
	        var entity = (FloatingDashBlock)entity1;
	        if (!entity.HasGroup && entity.tileType == tileType && Scene.CollideCheck(new Rectangle((int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height), entity))
            {
                AddToGroupAndFindChildren(entity);
            }
        }
    }
	
	private DashCollisionResults OnDashed(Player player, Vector2 direction)
	{
		Break(player.Center, direction);
		return DashCollisionResults.Rebound;
	}
	
	public override void OnShake(Vector2 amount)
	{
		if (!MasterOfGroup)
		{
			return;
		}
		base.OnShake(amount);
		tiles.Position += amount;
		foreach (var component in Jumpthrus.SelectMany(jumpthru => jumpthru.Components))
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