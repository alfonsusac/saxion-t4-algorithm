using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class PathFinder_BreadthFirst : PathFinder_Recursive
{

	public PathFinder_BreadthFirst(NodeGraph pGraph, bool visualized) : base(pGraph, visualized) {
	
	}

	// Data Structure necessary for BFS
	Dictionary<Node, Node> prevNode;
	Node lastStartNode;

    // Overriding parent class
    protected override void initialize(Node start, Node dest)
    {
        base.initialize(start, dest);

		// necessary to reset: for encapsulation
		callqueue = new Queue<Step>();
		functionForCallingFromList = CallfromStack;
		functionCollection = callqueue;

		if (lastStartNode != start || prevNode == null)
			prevNode = new Dictionary<Node, Node>();
			prevNode[start] = null;
	}

	public void pregenerate(Node start)
    {
		generate(start, null);
	}
	protected override List<Node> generate(Node pFrom, Node pTo)
    {
		if (pFrom == lastStartNode)
		{
			destination = pTo;

			generateShortestPath(pTo);

			return shortestPath; // and generate() will return shortestPath;
		}

		shortestPath = base.generate(pFrom, pTo);

		lastStartNode = pFrom;

		return shortestPath;
    }

	protected override void generateWithoutVisual(Node start)
	{
		traverse(start, null);

		shortestPath = generateShortestPath(destination);
	}

	protected override void generateWithVisual(Node start)
    {
		new Step(this, start);

		// call returnShortestPath() on the frame where the search is finished.
	}

	public override List<Node> getShortestPath()
	{
		shortestPath = generateShortestPath(destination);
		
		return base.getShortestPath();
	}



	// Overriding traverse method
	protected override void traverse(Node curr, List<Node> path = null, int dist = 0)
	{
		diagnostic.traverseCalls++;

		// Initialize the List if it is called for the first time
		if (path == null) path = new List<Node>();
		_labelDrawer.drawPaths(path, curr);

		// Add current node to the traveled path
		path.Add(curr); diagnostic.nodeVisited++; _labelDrawer.countVisits(curr);

		if (curr.connections.Count != 0)
			// Iterate to every child
			foreach (Node child in curr.connections)
				if (!prevNode.ContainsKey(child))
                {
					prevNode[child] = curr;
					traverseThrough(child, path, dist + 1);
				}

		if (!visualized)
		{
			if (callqueue.Count > 0)
				// iterate to next loop
				CallfromStack();
            else
            {
				getShortestPath();
			}
		}
	}
	
	protected override void traverseThrough(Node child, List<Node> path, int dist)
	{
		new Step(this, child, path, dist + 1);
		diagnostic.edgeVisited++;
	}



	private List<Node> generateShortestPath(Node dest)
    {
		if(dest == null) return null; //for pregenerated paths

		Node curr = dest;
		List<Node> path = new List<Node>();
		path.Insert(0, curr);

		while (prevNode.ContainsKey(curr))
        {
			//Console.WriteLine($"Curr {curr} <- Prev {prevNode[curr]}");
			curr = prevNode[curr];
			if (curr == null)
			{
				lastStartNode = path[0];
				return path;
			}
            path.Insert(0, curr);
		}
		
		return null;
    }



	protected override void CallfromStack() { callqueue.Dequeue().Run(); }
	protected Queue<Step> callqueue;

	internal class Step : TraverseRecursively
	{
		public Step(PathFinder_BreadthFirst r, Node n, List<Node> l = null, int i = 0)
			: base(r,n,l,i){ }

		public override void Add(TraverseRecursively t)
		{
			(pf as PathFinder_BreadthFirst).callqueue.Enqueue(t as Step);
		}
	}


}

