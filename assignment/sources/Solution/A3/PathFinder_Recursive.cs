using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class PathFinder_Recursive : SamplePathFinder
{

	public PathFinder_Recursive(NodeGraph pGraph, bool visualized) : base(pGraph,visualized)
	{ }

	protected int shortestDist = int.MaxValue;

	// for recursive stuff
	private List<Node> tempShortestPath;


	// These are required since the recursion now happens for every frame
	protected override void initialize(Node start, Node dest)
	{
		// Diagnostics
		diagnostic = new BasicDiagnostic();

		// necessary to reset: initialization
		destination = dest;
		shortestDist = int.MaxValue;
		running = false;

		// Resetting Graphic Stuff
		_labelDrawer?.clearQueueLabels();

		tempShortestPath = null;

		functionForCallingFromList = CallfromStack;
		callstack = new Stack<TraverseRecursively>();
		functionCollection = callstack;
	}
	protected virtual void CallfromStack()
	{
		Console.WriteLine("CallfromStack");
		callstack.Pop().Run();
	}

	protected override void generateWithVisual(Node start)
    {
		new TraverseRecursively(this, start);
	}
	protected override void generateWithoutVisual(Node start)
    {
		traverse(start, null);
    }

	protected override void traverse(Node n, List<Node> path, int dist = 0)
    {
		diagnostic.traverse();
		// Deny entry if destination is null
		if (destination == null) return;

		// Initialize the List if it is called for the first time
		if (path == null) path = new List<Node>();
		_labelDrawer.drawPaths(path, n);


		// If this node is the final node, 
		if (n == destination)
        {
			// If the distance to this node is the shortest node.
			if( dist < shortestDist )

				// then copy path to the global shortestpath
				markAsShortest(n, dist, path);

			return;
        }

		path.Add(n); diagnostic.visitNode(); _labelDrawer.countVisits(n);

		if (n.connections.Count != 0)
        {
            // Iterate to every unvisted child
            foreach (Node child in n.connections)
				if (!path.Contains(child))
					traverseThrough(child, path, dist + 1);

		}

		path.RemoveAt(path.Count-1);

	}

	protected override void traverseThrough(Node child, List<Node> path, int dist)
	{
		if (visualized) new TraverseRecursively(this, child, path, dist + 1); else traverse(child, path, dist + 1);
		diagnostic.visitEdge();
	}
	// The way to get shortest path based on the algorithm.
	protected override List<Node> getShortestPath()
	{
		// bcs recursive path finder has to manually udpate ShortestPath.
		shortestPath = tempShortestPath;
		return shortestPath;
	}


	private void markAsShortest(Node n, int dist, List<Node> path)
    {
        shortestDist = dist;
		tempShortestPath = new List<Node>(path);
		tempShortestPath.Add(n);	
    }


	//////////////////////////////////////////////////////////////////////////////
	// for visualization

}

