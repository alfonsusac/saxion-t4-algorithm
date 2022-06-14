using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class PathFindingAgent : NodeGraphAgent
{
	//Current target to move towards
	private Node _target = null;
	private Queue<Node> _targetsqueue = new Queue<Node>();
	public Queue<Node> TargetsQueue { get { return _targetsqueue; } }
	private Node currentNode;
	private PathFinder _pathFinder;

	public PathFindingAgent(NodeGraph pNodeGraph, PathFinder pPathFinder, float _pscale = 1f) : base(pNodeGraph, _pscale)
	{
		SetOrigin(width / 2, height / 2);

		_pathFinder = pPathFinder;

		HighLevelDungeonNodeGraph nodegraph = pNodeGraph as HighLevelDungeonNodeGraph;

		//Console.WriteLine($"Node Graph Dimension {nodegraph.}, {pNodeGraph.height}");

		//position ourselves on a random node
		if (pNodeGraph.nodes.Count > 0)
		{
			JumpToNode(pNodeGraph.nodes[Utils.Random(0, pNodeGraph.nodes.Count)]);
		}

		//listen to nodeclicksxxxxxxxxxx
		pNodeGraph.OnNodeLeftClicked += onNodeClickHandler;
		pNodeGraph.OnNodeRightClicked += moveAgentonRightClick;
	}

	protected virtual void moveAgentonRightClick(Node pNode)
	{
		JumpToNode(pNode);
	}

	public void JumpToNode(Node pNode)
	{
		currentNode = pNode;
		SetXY(pNode.location.X, pNode.location.Y);
	}

	// The Update Function is dedicated to running the queue of the set of movement generated in onNodeClickHandler
	//  all the set of movement is precalculated in onNodeClickHandler which then the queue will be run in the Update() function.

	bool isMoving;
	public bool IsMoving { get { return isMoving; } }

	protected virtual void onNodeClickHandler(Node pNode)
	{
		if (isMoving) return;
		// On Click on the nodes
		foreach (Node n in currentNode.connections)
		{
			if (n.location == pNode.location)
			{
				_targetsqueue.Enqueue(n);
				isMoving = true;
				Console.WriteLine("Neighbor node found");
				return;
			}
		}

		List<Node> generatedShortestPath = (_pathFinder as BreadthFirstPathFinder).Generate(currentNode, pNode);
		foreach(Node n in generatedShortestPath)
        {
			_targetsqueue.Enqueue(n);
        }
	}




	protected override void Update()
	{
		// FOR EVERY FRAME

		// Check if currently there is no queue
		if (_targetsqueue.Count == 0 && _target == null)
			return;

		// If there is a queue in the _targetsqueue, then peek and set that as the target.
		if (_target == null) currentNode = _target = _targetsqueue.Peek();

		// the Agent will start moving to _target while checking the queue and the target.

		// Once done moving,
		if (moveTowardsNode(_target))
		{
			//Console.WriteLine("Moving");
			// set the current node to the current node. For this case the variable is still useless
			// also dequeue the current Node
			_targetsqueue.Dequeue();

			_labelDrawer.drawQueueLabels();

			if (_targetsqueue.Count > 0)
			{
				// if there are more nodes queueing, then dequeue them as the next target
				_labelDrawer.markNode(_target);
				currentNode = _target = _targetsqueue.Peek();
			}
			else
			{
				// else, set it to null to stop the agent.
				_target = null;
				isMoving = false;
				_labelDrawer.clearQueueLabels();
			}
		}
	}
}
