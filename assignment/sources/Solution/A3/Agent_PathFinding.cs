using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class Agent_PathFinding : SampleNodeGraphAgent
{
	SamplePathFinder _pf;

	bool pregenerate = false;

	public Agent_PathFinding(NodeGraph pNodeGraph, SamplePathFinder pPathFinder, float _pscale = 1f) : base(pNodeGraph)
	{
		_pf = pPathFinder;

		if (_pf.GetType() == typeof(PathFinder_BreadthFirst))

			pregenerate = true;

	}

	protected override void _onNodeLCWhileD(Node pNode)
	{
		interruptAndReGenerate();
	}

	protected override void _onNodeRCWhileD(Node pNode)
	{
		interruptAndReGenerate();
	}

	protected override void _onNodeClickJumpToNode(Node pNode)
    {
		base._onNodeClickJumpToNode(pNode);

		_pf.StopFinding();
		waitForGeneration = false;
		dopregenerate(pNode);
	}

	public void interruptAndReGenerate()
	{
		_pf.StopFinding();
		waitForGeneration = false;
		dopregenerate(currentNode);
	}


	protected override void onNodeClickHandler(Node pNode)
	{
		// On Click on the nodes
		// check their immediate neighbors. If yes then move.
		foreach (Node n in currentNode.active_connections)

			if (n.location == pNode.location)
			{
				_targetsqueue.Enqueue(n);

				Console.WriteLine("Neighbor node found");

				return;
			}


		List<Node> generatedPath = _pf.Generate(currentNode, pNode);

		// if instantly available, then the PATHFINDER must have generated it in one frame.
		if (generatedPath != null)

			// if so, iterate every solution and put it into the queue.
			generatedPath.ForEach(n => _targetsqueue.Enqueue(n));

		// if it returned null. Either PF is running or it is not running
		else
			
			waitForGeneration = true;
	}

	private bool waitForGeneration;

	protected override void Update()
    {
		// if pregenerate is true, then generate the path at end node.
		if (pregenerate == true) {
			pregenerate = false;

			if (_pf.GetType() == typeof(PathFinder_BreadthFirst))

				(_pf as PathFinder_BreadthFirst).pregenerate(currentNode);
		}

		// if we are waiting for generation to complete,  and pf is NO LONGER running, then get shortest path
        if (waitForGeneration && !_pf.IsRunning)
        {
			waitForGeneration = false;

			// Extract the path to the agent's walk queue\
			if(_pf.GetShortestPath() != null)
            {
				_pf.GetShortestPath()?.ForEach(n => _targetsqueue.Enqueue(n));

				dopregenerate(_pf.Destination);
			}


		}

		// execute the walk queue.
		base.Update();
    }

	protected void dopregenerate(Node start)
    {
		if (_pf.GetType() == typeof(PathFinder_BreadthFirst))

			(_pf as PathFinder_BreadthFirst).pregenerate(start);
	}



}
