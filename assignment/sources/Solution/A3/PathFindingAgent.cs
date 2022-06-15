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
	}

	protected override void onNodeClickHandler(Node pNode)
	{
		if (IsMoving) 
			return;
		// On Click on the nodes
		foreach (Node n in currentNode.connections)
		{
			if (n.location == pNode.location)
			{
				_targetsqueue.Enqueue(n);

				Console.WriteLine("Neighbor node found");
				return;
			}
		}

		List<Node> generatedShortestPath = _pf.Generate(currentNode, pNode);

		if(generatedShortestPath != null)
        {
			if (_pf.visualized) waitForGeneration = true;
			foreach (Node n in generatedShortestPath)
				_targetsqueue.Enqueue(n);
		}
	}

	private bool waitForGeneration;

	protected override void Update()
    {
        if (waitForGeneration && !_pf.IsRunning)
        {
			waitForGeneration = false;
			foreach (Node n in _pf.ShortestPath)
				_targetsqueue.Enqueue(n);
		}
		base.Update();
    }

}
