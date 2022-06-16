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

	private bool isMoving;
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
		pNodeGraph.OnNodeLeftClicked += _onNodeClickHandler;

		pNodeGraph.OnNodeRightClicked += jumpToNode;
	}
	protected override void jumpToNode(Node pNode)
	{
		base.jumpToNode(pNode);
		currentNode = pNode;
	}


	private void _onNodeClickHandler(Node pNode)
    {
		// Interrupting the queue
		if (TargetsQueue.Count > 0 && _target != null) currentNode = _target;

		TargetsQueue.Clear();

		_labelDrawer.clearMark();

		onNodeClickHandler(pNode);
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

	const bool isFinished = true;

	protected override void Update()
	{
		// FOR EVERY FRAME

		if (_target == null)

			if(_targetsqueue.Count > 0)

				DequeueNextNode();

			else

				toggleMovingStatus(false);

		else //if (_target != null)

			if (moveTowardsNode(_target) == isFinished)

				UpdateOnceArrived();

	}
	private void DequeueNextNode()
	{
		// Graphic Stuff

		_labelDrawer.drawQueuePath(TargetsQueue);

		_labelDrawer.markNode(TargetsQueue.Peek());


		if (currentNode != TargetsQueue.Peek() && !currentNode.connections.Contains(TargetsQueue.Peek()))
			
			Console.WriteLine($"WARNING!: At{currentNode} The next target {TargetsQueue.Peek()} is not neighboring node");

		_target = TargetsQueue.Dequeue();

		
		
		//toggleMovingStatus(true);
	}
	private void UpdateOnceArrived()
	{
		_target = null;
	}

	bool strictDebug = false;
	protected void toggleMovingStatus(bool b)
    {
        if (strictDebug)

			if (isMoving == false && b == true) isMoving = b;

			else if (isMoving == true && b == false) isMoving = b;

			else throw new Exception($"isMoving is already at {isMoving}!! (b = {b})");

		else

			isMoving = b;
    }
}
