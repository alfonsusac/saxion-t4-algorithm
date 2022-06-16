using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class Agent_OnGraphWayPoint : NodeGraphAgent
{
    //Current target to move towards
    private Node _target = null;
    private Queue<Node> _targetsqueue = new Queue<Node>();
	private Node lastlyAddedNode;
	private Node currentNode;

	public Agent_OnGraphWayPoint(NodeGraph pNodeGraph) : base(pNodeGraph)
	{
		SetOrigin(width / 2, height / 2);
		
		NodeGraph_HighLevelDungeon nodegraph = pNodeGraph as NodeGraph_HighLevelDungeon;

		//Console.WriteLine($"Node Graph Dimension {nodegraph.}, {pNodeGraph.height}");

		//position ourselves on a random node
		if (pNodeGraph.nodes.Count > 0)
		{
			currentNode = pNodeGraph.nodes[Utils.Random(0, pNodeGraph.nodes.Count)];
			lastlyAddedNode = currentNode;
			jumpToNode(currentNode);
		}

		//listen to nodeclicks
		pNodeGraph.OnNodeLeftClicked += onNodeClickHandler;
	}



	protected virtual void onNodeClickHandler(Node pNode)
	{
		// On Click on the nodes


		// Check if the clicked nodes are the neighboring, reachable nodes from current node
		if (lastlyAddedNode.connections.Contains(pNode) )
        {
			// If yes, queue the clicked nodes.
			_targetsqueue.Enqueue(pNode);

			// Override the lastly added node to check if the next clicked node is connected to the lastly queued node.
			lastlyAddedNode = pNode;

        }
        else
        {
            Console.WriteLine("Nodes are not directly connected!!");
        }
    }

	protected override void Update()
	{
		// FOR EVERY FRAME

		// Check if currently there is no queue
		if (_targetsqueue.Count == 0 && _target == null)
			return;

		// If there is a queue in the _targetsqueue, then peek and set that as the target.
		if (_target == null) _target = _targetsqueue.Peek();

		// the Agent will start moving to _target while checking the queue and the target.

		// Once done moving,
        if (moveTowardsNode(_target))
        {
			// set the current node to the current node. For this case the variable is still useless
			// also dequeue the current Node
			currentNode = _targetsqueue.Dequeue();

			if (_targetsqueue.Count > 0)
				// if there are more nodes queueing, then dequeue them as the next target
				_target = _targetsqueue.Peek();
			else
				// else, set it to null to stop the agent.
				_target = null;
        }
	}
}
