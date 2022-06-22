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
	public TiledDungeonView _view { get; set; }

	public NodeGraph_LowLevelDungeon(Dungeon pDungeon) : base((int)(pDungeon.size.Width * pDungeon.scale), (int)(pDungeon.size.Height * pDungeon.scale), ((int)pDungeon.scale / 3) + 1) // + 1 because it gets too small
	{
		Debug.Assert(pDungeon != null, "Please pass in a dungeon.");

		_dungeon = pDungeon;

		OnNodeLeftClickedWhileHoldDKey += disableNode;
		OnNodeRightClickedWhileHoldDKey += enableNode;

	}

	protected override void disableNode(Node n)
    {
		if (n.disabled) return;
		n.disabled = true;
		Console.WriteLine($"Node {n} Disabled");
		drawNodeConnections(n);
		drawNode(n, Brushes.Gray);
		drawConnectedRooms();
	}	
	
	protected override void enableNode(Node n)
    {
		if (!n.disabled) return;
		n.disabled = false;
		Console.WriteLine($"Node {n} Enabled");
		drawNodeConnections(n);
		drawNode(n, Brushes.CornflowerBlue);
		drawConnectedRooms();
	}

	private bool GenerateAfterTile = false;

	protected override void generate()
	{
		GenerateAfterTile = true;
	}

	bool[,] visited;
	Node[,] nodeAt;
	internal int maxid;
	//int step = 0; 

	public void generateTiled() {

		maxid = 1;
		visited = new bool[_dungeon.size.Width, _dungeon.size.Height];
		nodeAt = new Node[_dungeon.size.Width, _dungeon.size.Height];

		Console.Write("Generating Tiled Dungones...");

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

						maxid += 1;
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
								int adjx = adj[k, l, 0];
								int adjy = adj[k, l, 1];
								// if neighborin nodes are the curent one then continue
								if (adjx == i && adjy == j) continue;

								//Console.WriteLine($"checking neighboringtile at {adj[k, l, 0]}, {adj[k, l, 1]}");

								// if the neighboring nodes are ground and THERE IS NO NODE THERE YET
								if (_view.GetTileType(adjx, adjy) == TileType.GROUND && nodeAt[adjx, adjy] == null)
								{
									//Console.WriteLine($"node not found. Creating node");

									Node neighborNode = new Node(getTileCenter(adjx, adjy));
									nodes.Add(neighborNode);

									visited[adjx, adjy] = true;

									nodeAt[adjx, adjy] = neighborNode;

								}

								// if THERE IS NEIGHBORING NODES.
								if (nodeAt[adjx, adjy] != null)
                                {
									//if(  colorid[adjx, adjy] < colorid[i,j] )
         //                           {
									//	Console.WriteLine($"nodeat{nodeAt[i, j]} colorid {colorid[adjx, adjy]} and {colorid[i, j]}");

									//	//colormap[colorid[adjx, adjy]] = colorid[i, j];

									//	colormap[colorid[i,j]].Add(colorid[adjx,adjy]);

									//	if(step == 8)
         //                               {
									//		//AddConnection(currNode, nodeAt[adjx, adjy]);
									//		//draw();
									//		//_labelDrawer.drawConnectedRooms();
									//		//return;
									//	}
									//	step++;

									//}


									//if (  colormap[colorid[adj[k, l, 0], adj[k, l, 1]]] != colorid[i,j])
									//colormap[colorid[adj[k, l, 0], adj[k, l, 1]]] = colorid[i, j];

									//Console.WriteLine($"linking node");
									AddConnection(currNode, nodeAt[adjx, adjy]);
								}
							}
					}
				}
			}
		}
		Console.WriteLine(" Done!");

		draw();

		drawConnectedRooms();
	}

	internal Dictionary<Node,int> visitedNodes;
	internal int coloridd = 0;

	protected void drawConnectedRooms()
	{
		Console.Write("Drawing Connected Rooms...");
		visitedNodes = new Dictionary<Node,int>();

		foreach (Node n in nodes)
        {
			if (!visitedNodes.ContainsKey(n) && !n.disabled)
            {
                //Console.WriteLine($"Doing Traverse at Node {n}");
                bfsqueue = new Queue<Node>();
				inQueue = new HashSet<Node>();
				bfsqueue.Enqueue(n);
				while(bfsqueue.Count > 0)
                {
					traverse();
                }
				coloridd++; 
			}
        }
		_labelDrawer.drawConnectedRooms2();
		Console.WriteLine(" Done!");
	}

	Queue<Node> bfsqueue;

	//int steps = 0;
	HashSet<Node> inQueue;

	protected void traverse()
	{
		//if (steps == 5) return;

		Node curr = bfsqueue.Dequeue();
		visitedNodes[curr] = coloridd;

		if (!curr.isolated)
			// Iterate to every child
			foreach (Node child in curr.active_connections)
            {
				Console.WriteLine($"Currently checking {curr} and {child}");

				if (!visitedNodes.ContainsKey(child) && !inQueue.Contains(child))
				{
					bfsqueue.Enqueue(child);
					inQueue.Add(child);
				}
				else
                {
					Console.WriteLine($"{child} is in visitedNodes");

				}
			}

		//steps++;
	}

	protected Point getTileCenter(int x, int y)
    {
		return getPointCenter(new Point(x, y));
	}
	protected Point getPointCenter(Point pLocation)
	{
		float centerX = (pLocation.X + 0.5f) * _dungeon.scale;
		float centerY = (pLocation.Y + 0.5f) * _dungeon.scale;
		return new Point((int)centerX, (int)centerY);
	}

	// Graphics Handling
	protected NodeLabelDrawer _labelDrawer;
	public void SetLabelDrawer(NodeLabelDrawer n) { _labelDrawer = n; }

}
