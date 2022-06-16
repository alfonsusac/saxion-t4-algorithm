using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

/**
 * An example of a dungeon nodegraph implementation.
 * 
 * This implementation places only three nodes and only works with the SampleDungeon.
 * Your implementation has to do better :).
 * 
 * It is recommended to subclass this class instead of NodeGraph so that you already 
 * have access to the helper methods such as getRoomCenter etc.
 * 
 * TODO:
 * - Create a subclass of this class, and override the generate method, see the generate method below for an example.
 */
class NodeGraph_LowLevelDungeon : NodeGraph
{
	protected Dungeon _dungeon;

	public NodeGraph_LowLevelDungeon(Dungeon pDungeon) : base((int)(pDungeon.size.Width * pDungeon.scale), (int)(pDungeon.size.Height * pDungeon.scale), ((int)pDungeon.scale / 3) + 1) // + 1 because it gets too small
	{
		Debug.Assert(pDungeon != null, "Please pass in a dungeon.");

		_dungeon = pDungeon;
	}

	private bool GenerateAfterTile = false;

	protected override void generate()
	{
		//Generate nodes, in this sample node graph we just add to nodes manually
		//of course in a REAL nodegraph (read:yours), node placement should 
		//be based on the rooms in the dungeon

		Dictionary<Point, Node> getNodefromRoom = new Dictionary<Point, Node>();

		//// Make node for all room
		//foreach (Room room in _dungeon.rooms)
		//{
		//	Node roomnode = new Node(getRoomCenter(room));
		//	nodes.Add(roomnode);
		//	getNodefromRoom[room.area.Location] = roomnode;
		//}

		//Console.WriteLine($"Dict Count: {getNodefromRoom.Count}");

		//// Make node for all corridors, which consists of two nodes per corridor
		//foreach (ExcellentDungeon.Corridor c in ExcellentDungeon.Corridor.corridors)
		//{
		//	// Create node on the entry point of the corridor
		//	Node nodeentry1 = new Node(getPointCenter(c.entry1));
		//	//getNodefromEntryPoint[c.entry1] = nodeentry1;
		//	nodes.Add(nodeentry1);
		//	AddConnection(nodeentry1, getNodefromRoom[c.roomA.area.Location]);

		//	// Create node on the entry point of the corridor
		//	Node nodeentry2 = new Node(getPointCenter(c.entry2));
		//	//getNodefromEntryPoint[c.entry2] = doorentry2;
		//	nodes.Add(nodeentry2);
		//	AddConnection(nodeentry2, getNodefromRoom[c.roomB.area.Location]);

		//	// Add connection between the entry points
		//	AddConnection(nodeentry1, nodeentry2);

		//	// Add connection between the entry points into the room
		//}

		GenerateAfterTile = true;

	}

	TiledDungeonView _view = null;
	public void generateTiled(TiledDungeonView v) { 

		_view = v;

		bool[,] visited = new bool[_dungeon.width, _dungeon.height];
		Node[,] nodeAt = new Node[_dungeon.width, _dungeon.height];

        if (GenerateAfterTile)
        {
			for (int i = 0; i < _dungeon.size.Width; i++)
			{
				for (int j = 0; j < _dungeon.size.Height; j++)
				{
					//Console.WriteLine($"checking tile at {i}, {j}");

					visited[i, j] = true;
					TileType t = _view.GetTileType(i, j);

					// if there is no node at current position
					if(t == TileType.GROUND && nodeAt[i,j] == null)
                    {

						Node groundNode = new Node(getTileCenter(i, j));

						nodes.Add(groundNode);

						nodeAt[i, j] = groundNode;

					}

					// if there is  anode at current position
					if(nodeAt[i,j] != null)
                    {
						Node currNode = nodeAt[i, j];

						//MATRIX of ADJACENT+CORNER NODES
						int[,,] adj = new int[3, 3, 2]
						{
							{ {i-1 ,j-1 } ,{i ,j-1 } ,{i+1 ,j-1 } },
							{ {i-1 ,j   } ,{i ,j   } ,{i+1 ,j   } },
							{ {i-1 ,j+1 }, {i ,j+1 } ,{i+1 ,j+1 } },
						};

						// for every neighboring nodes.
						for (int k = 0; k < 3; k++)
							for (int l = 0; l < 3; l++)
							{
								// if neighborin nodes are the curent one then continue
								if (adj[k, l, 0] == i && adj[k, l, 1] == j) continue;

								//Console.WriteLine($"checking neighboringtile at {adj[k, l, 0]}, {adj[k, l, 1]}");

								// if the neighboring nodes are ground and THERE IS NO NODE THERE YET
								if (_view.GetTileType(adj[k, l, 0], adj[k, l, 1]) == TileType.GROUND && nodeAt[adj[k, l, 0], adj[k, l, 1]] == null)
								{
									//Console.WriteLine($"node not found. Creating node");

									Node neighborNode = new Node(getTileCenter(adj[k, l, 0], adj[k, l, 1]));
									nodes.Add(neighborNode);

									visited[adj[k, l, 0], adj[k, l, 1]] = true;

									nodeAt[adj[k, l, 0], adj[k, l, 1]] = neighborNode;

								}

								// if THERE IS NEIGHBORING NODES.
								if(nodeAt[adj[k, l, 0], adj[k, l, 1]] != null)
                                {
									//Console.WriteLine($"linking node");
									AddConnection(currNode, nodeAt[adj[k, l, 0], adj[k, l, 1]]);
								}
							}
					}
				}
			}
		}

		draw();

	}

	/**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given room you can use for your nodes in this class
	 */
	//protected Point getRoomCenter(Room pRoom)
	//{
	//	float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f) * _dungeon.scale;
	//	float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f) * _dungeon.scale;
	//	return new Point((int)centerX, (int)centerY);
	//}

	protected Point getTileCenter(int x, int y)
    {
		return getPointCenter(new Point(x, y));

	}

	/**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given door you can use for your nodes in this class
	 */
	//protected Point getDoorCenter(Door pDoor)
	//{
	//	return getPointCenter(pDoor.location);
	//}

	/**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given point you can use for your nodes in this class
	 */
	protected Point getPointCenter(Point pLocation)
	{
		float centerX = (pLocation.X + 0.5f) * _dungeon.scale;
		float centerY = (pLocation.Y + 0.5f) * _dungeon.scale;
		return new Point((int)centerX, (int)centerY);
	}

}
