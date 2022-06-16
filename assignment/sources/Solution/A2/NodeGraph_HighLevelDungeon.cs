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
class NodeGraph_HighLevelDungeon : NodeGraph
{
	protected Dungeon _dungeon;

	public NodeGraph_HighLevelDungeon(Dungeon pDungeon) : base((int)(pDungeon.size.Width * pDungeon.scale), (int)(pDungeon.size.Height * pDungeon.scale), ((int)pDungeon.scale / 3)+1) // + 1 because it gets too small
	{
		Debug.Assert(pDungeon != null, "Please pass in a dungeon.");

		_dungeon = pDungeon;
	}

	protected override void generate()
	{
		//Generate nodes, in this sample node graph we just add to nodes manually
		//of course in a REAL nodegraph (read:yours), node placement should 
		//be based on the rooms in the dungeon

		//We assume (bad programming practice 1-o-1) there are two rooms in the given dungeon.
		//The getRoomCenter is a convenience method to calculate the screen space center of a room
		//nodes.Add(new Node(getRoomCenter(_dungeon.rooms[0])));
		//nodes.Add(new Node(getRoomCenter(_dungeon.rooms[1])));
		//The getDoorCenter is a convenience method to calculate the screen space center of a door
		//nodes.Add(new Node(getDoorCenter(_dungeon.doors[0])));
		Dictionary<Point, Node> getNodefromRoom = new Dictionary<Point, Node>();

		// Make node for all room
		foreach (Room room in _dungeon.rooms)
		{
			Node roomnode = new Node(getRoomCenter(room));
			nodes.Add(roomnode);
			getNodefromRoom[room.area.Location] = roomnode;
		}

			Console.WriteLine($"Dict Count: {getNodefromRoom.Count}");

		// Make node for all corridors, which consists of two nodes per corridor
		foreach (ExcellentDungeon.Corridor c in ExcellentDungeon.Corridor.corridors)
        {
			// Create node on the entry point of the corridor
			Node nodeentry1 = new Node(getPointCenter(c.entry1));
			//getNodefromEntryPoint[c.entry1] = nodeentry1;
			nodes.Add(nodeentry1);
			AddConnection(nodeentry1, getNodefromRoom[c.roomA.area.Location]);

			// Create node on the entry point of the corridor
			Node nodeentry2 = new Node(getPointCenter(c.entry2));
			//getNodefromEntryPoint[c.entry2] = doorentry2;
			nodes.Add(nodeentry2);
			AddConnection(nodeentry2, getNodefromRoom[c.roomB.area.Location]);

			// Add connection between the entry points
			AddConnection(nodeentry1, nodeentry2);

			// Add connection between the entry points into the room
		}



		//create a connection between the two rooms and the door...
		//AddConnection(nodes[0], nodes[2]);
		//AddConnection(nodes[1], nodes[2]);
	}

	/**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given room you can use for your nodes in this class
	 */
	protected Point getRoomCenter(Room pRoom)
	{
		float centerX = ((pRoom.area.Left + pRoom.area.Right) / 2.0f) * _dungeon.scale;
		float centerY = ((pRoom.area.Top + pRoom.area.Bottom) / 2.0f) * _dungeon.scale;
		return new Point((int)centerX, (int)centerY);
	}

	/**
	 * A helper method for your convenience so you don't have to meddle with coordinate transformations.
	 * @return the location of the center of the given door you can use for your nodes in this class
	 */
	protected Point getDoorCenter(Door pDoor)
	{
		return getPointCenter(pDoor.location);
	}

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
