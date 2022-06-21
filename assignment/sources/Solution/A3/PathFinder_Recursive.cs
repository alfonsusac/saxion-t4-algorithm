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

	protected double shortestDist = int.MaxValue;

	// for recursive stuff
	private List<Node> tempShortestPath;

    protected Stack<Step> callstack = new Stack<Step>();


    // These are required since the recursion now happens for every frame
    protected override void initialize(Node start, Node dest)
	{
		callstack = new Stack<Step>();
		functionForCallingFromList = CallfromStack;
		functionCollection = callstack;

        // necessary to reset: initialization
        shortestDist = int.MaxValue;
		tempShortestPath = null;

	}
	protected virtual void CallfromStack()
	{
		Console.WriteLine("CallfromStack");
		callstack.Pop().Run();
	}

	protected override void traverse(Node n, List<Node> path, double dist = 0)
    {
		Console.Write($"VISINT NODE: {n} | "); path.ForEach(p => Console.WriteLine(p + " ")); Console.WriteLine();

		if (!n.isolated)

			// Iterate to every unvisted child
			foreach (Node child in n.active_connections)

				if (!path.Contains(child))

					if (n == destination && dist + 1 < shortestDist)

						markAsShortest(dist + 1, path);

					else

						traverseThrough(child, path, dist + 1);
	}

	protected override void traverseThrough(Node child, List<Node> path, double dist)
	{
		new Step(this, child, path, dist + 1);

		if(!visualized) CallfromStack();
	}
	// The way to get shortest path based on the algorithm.
	protected override List<Node> getShortestPath()
	{
		// bcs recursive path finder has to manually udpate ShortestPath.
		shortestPath = tempShortestPath;

		return shortestPath;
	}


	private void markAsShortest(double dist, List<Node> path)
    {
        shortestDist = dist;
		tempShortestPath = new List<Node>(path);
    }


	//////////////////////////////////////////////////////////////////////////////
	// for visualization
	internal class Step : TraverseRecursively
	{
		public Step(PathFinder_Recursive r, Node n, List<Node> l = null, double i = 0)
			: base(r, n, l, i) { }

		public override void Add()
		{
			(pf as PathFinder_Recursive).callstack.Push(this);
		}
	}
}

