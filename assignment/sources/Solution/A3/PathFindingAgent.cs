using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class PathFindingAgent : SampleNodeGraphAgent
{
	RecursivePathFinder _pf;

	public PathFindingAgent(NodeGraph pNodeGraph, RecursivePathFinder pPathFinder, float _pscale = 1f) : base(pNodeGraph)
	{
		_pf = pPathFinder;
		if (_pf is BreadthFirstPathFinder)
            (_pf as BreadthFirstPathFinder).pregenerate(currentNode);
	}

	protected override void onNodeClickHandler(Node pNode)
	{
		if (TargetsQueue.Count > 0) currentNode = _target;
		TargetsQueue.Clear();

		if (IsMoving) return;


		// On Click on the nodes
		// check their immediate neighbors. If yes then move.
		foreach (Node n in currentNode.connections)
		{
			if (n.location == pNode.location)
			{
				_targetsqueue.Enqueue(n);

				Console.WriteLine("Neighbor node found");
				return;
			}
		}

		_pf.Generate(currentNode, pNode);

		if(_pf.getShortestPath() != null)
        {
			foreach (Node n in _pf.getShortestPath())
				
				_targetsqueue.Enqueue(n);
        }
        else
			
			waitForGeneration = true;
	}

	private bool waitForGeneration;

	protected override void Update()
    {
        if (waitForGeneration && !_pf.IsRunning)
        {
			waitForGeneration = false;

			// Extract the path to the agent's walk queue
			if(_pf.getShortestPath() != null) foreach (Node n in _pf.getShortestPath())
				
				_targetsqueue.Enqueue(n);

		}

		// execute the walk queue.
		base.Update();
    }

}
