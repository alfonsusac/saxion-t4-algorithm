using GXPEngine;
using System;
using System.Collections.Generic;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
abstract class SampleNodeGraphAgent : NodeGraphAgent
{
	//Current target to move towards
	protected Node _target = null;

	//The current node the agent is at
	protected Node currentNode;

	protected bool isMoving;
	public bool IsMoving { get { return isMoving; } }



	public SampleNodeGraphAgent(NodeGraph pNodeGraph) : base(pNodeGraph)
	{
		SetOrigin(width / 2, height / 2);

		//position ourselves on a random node
		if (pNodeGraph.nodes.Count > 0)
		{
			jumpToNode(pNodeGraph.nodes[Utils.Random(0, pNodeGraph.nodes.Count)]);
		}

		//listen to nodeclicks
		pNodeGraph.OnNodeLeftClicked += onNodeClickHandler;
		pNodeGraph.OnNodeRightClicked += jumpToNode;
	}
	protected override void jumpToNode(Node pNode)
	{
		base.jumpToNode(pNode);
		currentNode = pNode;
	}
	protected abstract void onNodeClickHandler(Node pNode);
	// 👇👇 for keepsake 👇👇
	//protected virtual void onNodeClickHandler(Node pNode)
	//{
	//	_target = pNode;
	//}

	///////////////////////////////////////////////////////////////
	// THE UPDATE FUNCTION
	//

	// The Update Function is dedicated to running the queue of the set of movement generated in onNodeClickHandler
	//  all the set of movement is precalculated in onNodeClickHandler which then the queue will be run in the Update() function.


	// Queue of the target to be dequeued one by one after visiting the node
	protected Queue<Node> _targetsqueue = new Queue<Node>();
	public Queue<Node> TargetsQueue { get { return _targetsqueue; } }

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
				//_labelDrawer.clearConnectionLabel(_target, _targetsqueue.Peek());
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



		// 👇👇 for keepsake 👇👇
		////no target? Don't walk
		//if (_target == null) return;

		////Move towards the target node, if we reached it, clear the target
		//if (moveTowardsNode(_target))
		//{
		//	_target = null;
		//}
	}
}
