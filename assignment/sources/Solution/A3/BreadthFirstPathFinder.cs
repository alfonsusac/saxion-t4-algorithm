using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class BreadthFirstPathFinder : RecursivePathFinder
{

	public BreadthFirstPathFinder(NodeGraph pGraph) : base(pGraph) { }

	// Data Structure necessary for BFS
	Dictionary<Node, Node> prevNode;

	Node lastCurrentNode;

    // Overriding parent class
    protected override void initialize(Node start, Node dest)
    {
        base.initialize(start, dest);
		callqueue = new Queue<Step>();
		prevNode = new Dictionary<Node, Node>();
		functionForCallingFromList = CallfromStack;
		functionCollection = callqueue;
		prevNode[start] = null;
		lastCurrentNode = null;
	}

	protected override void generateWithoutVisual(Node start)
	{
		if(lastCurrentNode != start)
			traverse(start, null);

		shortestPath = generateShortestPath(destination);
	}

	protected override void generateWithVisual(Node start)
    {
		if(lastCurrentNode != start)
			new Step(this, start);

		// call returnShortestPath() on the frame where the search is finished.
	}

	protected override void returnShortestPath()
	{
		shortestPath = generateShortestPath(destination);
		base.returnShortestPath();
	}



	// Overriding traverse method
	protected override void traverse(Node curr, List<Node> path = null, int dist = 0)
	{
		if (destination == null) return;
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
				CallfromStack();
            else
            {
				returnShortestPath();
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
		Node curr = dest;
		List<Node> path = new List<Node>();
		path.Insert(0, curr);

		while (prevNode.ContainsKey(curr))
        {
			//Console.WriteLine($"Curr {curr} <- Prev {prevNode[curr]}");
			curr = prevNode[curr];
			if (curr == null) break;
			path.Insert(0, curr);
		}

		lastCurrentNode = path[0];
		return path;
    }



	protected Queue<Step> callqueue;
	protected override void CallfromStack() { callqueue.Dequeue().Run(); }



	internal class Step : TraverseRecursively
	{
		public Step(BreadthFirstPathFinder r, Node n, List<Node> l = null, int i = 0)
			: base(r,n,l,i){ }

		public override void Add(TraverseRecursively t)
		{
			(pf as BreadthFirstPathFinder).callqueue.Enqueue(t as Step);
		}
	}


}

