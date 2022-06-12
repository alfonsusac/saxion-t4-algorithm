using GXPEngine;
using System.Collections.Generic;

/**
 * This is an example subclass of the TiledView that just generates random tiles.
 */
class TiledDungeonView : TiledView
{
	/**
	 * This constructor takes a dungeon but doesn't do anything with it, it is just an example of how
	 * to initialize the TiledView parameters with size and scale data from the dungeon,
	 * make sure you understand what is happening here before you continue.
	 */
	private ExcellentDungeon dungeon;

	public TiledDungeonView(Dungeon pDungeon, TileType pDefaultTileType) : base(pDungeon.size.Width, pDungeon.size.Height, (int)pDungeon.scale, TileType.VOID)
	{
		dungeon = (ExcellentDungeon)pDungeon; 
	}

	/**
	 * Fill the tileview with random data instead.
	 * In your subclass, you should set the tiletype correctly based on the provided dungeon contents.
	 */
	bool[,] visited;

	protected override void generate()
	{
		TileType[,] TTMap = new TileType[columns, rows];
		visited = new bool[columns, rows];

		foreach(Room r in dungeon.rooms)
        {
			for (int i = 0; i < r.area.Width; i++)
			{
				for (int j = 0; j < r.area.Height; j++)
				{
					// vertical walls
					if (i == 0 || i == r.area.Width - 1 || j == 0 || j == r.area.Height - 1)
					{
						SetTileType(i + r.area.X, j + r.area.Y, TileType.WALL);
						visited[i + r.area.X, j + r.area.Y] = true;
					}
					else
					{
						SetTileType(i + r.area.X, j + r.area.Y, TileType.GROUND);
						visited[i + r.area.X, j + r.area.Y] = true;
					}
				}
			}
		}

		foreach(ExcellentDungeon.Corridor c in ExcellentDungeon.Corridor.corridors)
        {
			if(c.Direction == ExcellentDungeon.HORIZONTAL)
            {
				for (int i = 0; i <= c.ExtensionTopLeft + c.ExtensionBottomRight; i++)
				{
					SetTileType(c.X - c.ExtensionTopLeft + i, c.Y - 1, TileType.WALL);
					SetTileType(c.X - c.ExtensionTopLeft + i, c.Y    , TileType.GROUND);
					SetTileType(c.X - c.ExtensionTopLeft + i, c.Y + 1, TileType.WALL);
					visited[c.X - c.ExtensionTopLeft + i, c.Y - 1] = true;
					visited[c.X - c.ExtensionTopLeft + i, c.Y] = true;
					visited[c.X - c.ExtensionTopLeft + i, c.Y + 1] = true;
				}
			}
			else if(c.Direction == ExcellentDungeon.VERTICAL)
            {
				for (int i = 0; i <= c.ExtensionTopLeft + c.ExtensionBottomRight; i++)
				{
					SetTileType(c.X - 1, c.Y - c.ExtensionTopLeft + i, TileType.WALL);
					SetTileType(c.X,     c.Y - c.ExtensionTopLeft + i, TileType.GROUND);
					SetTileType(c.X + 1, c.Y - c.ExtensionTopLeft + i, TileType.WALL);
					visited[c.X - 1, c.Y - c.ExtensionTopLeft + i] = true;
					visited[c.X,     c.Y - c.ExtensionTopLeft + i] = true;
					visited[c.X + 1, c.Y - c.ExtensionTopLeft + i] = true;
				}
			}
        }
	}
}

