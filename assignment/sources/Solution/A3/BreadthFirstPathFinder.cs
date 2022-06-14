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
	Dictionary<Node, Node> prevNode = new Dictionary<Node, Node>();

    // Overriding parent class
    protected override void initialize(Node dest)
    {
        base.initialize(dest);
		functionForCallingFromList = CallfromStack;

	}

    protected override void processWithVisual(Node start)
    {
		setRunning(true);
		prevNode[start] = null;
		new Step(this, start);
	}
	protected override void processWithoutVisual(Node start)
    {
		traverse(start, null);
		shortestPath = generateShortestPath(destination);
	}

	// Overriding traverse method
	protected override void traverse(Node curr, List<Node> path = null, int dist = 0)
	{
		if (destination == null) return;
		diagnostic.traverseCalls++;

		// Initialize the List if it is called for the first time
		if (path == null) path = new List<Node>();
		_labelDrawer.drawPaths(path, curr);

		if (curr.connections.Count != 0)
		{
			// Add current node to the traveled path
			path.Add(curr); diagnostic.nodeVisited++; _labelDrawer.countVisits(curr);

			// Iterate to every child
			foreach (Node child in curr.connections)
				if (!prevNode.ContainsKey(child))
                {
					prevNode[child] = curr;
					traverseThrough(child, path, dist + 1);
				}

			// Remove current node from the traveled path as we need to traverse back
			path.RemoveAt(path.Count - 1);
		}
	}
	
	protected override void traverseThrough(Node child, List<Node> path, int dist)
	{
		if (visualized) new Step(this, child, path, dist + 1); else traverse(child, path, dist + 1);
		diagnostic.edgeVisited++;
	}

    private List<Node> generateShortestPath(Node dest)
    {
		Node curr = dest;
		List<Node> path = new List<Node>();
		path.Insert(0, curr);

		while (prevNode.ContainsKey(curr))
        {
			Console.WriteLine($"Curr {curr} <- Prev {prevNode[curr]}");
			curr = prevNode[curr];
			if (curr == null) break;
			path.Insert(0, curr);
		}

		return path;
    }

	new protected Queue<Step> callstack = new Queue<Step>();
	new protected void CallfromStack() { callstack.Dequeue().Run(); }

	internal class Step
	{
		readonly Node currentNode;
		readonly List<Node> travelPath;
		readonly int distance;
		readonly BreadthFirstPathFinder recpathfinder;

		public Step(BreadthFirstPathFinder r, Node n, List<Node> l = null, int i = 0)
		{
			currentNode = n;
			if (l != null) travelPath = new List<Node>(l); else travelPath = new List<Node>();
			distance = i;
			recpathfinder = r;

			r.callstack.Enqueue(this);
		}


		public override string ToString()
		{
			string s;
			if (travelPath == null) s = $"= Node: {currentNode} | List: null | Distance: {distance}";
			else { s = $"= Node: {currentNode} | List: "; travelPath.ForEach(e => s += e + " "); s += $"({travelPath.Count}) | Distance: {distance}"; }
			return s;
		}
		public void Run() { recpathfinder.traverse(currentNode, travelPath, distance); }

	}

	protected override void iterateSteps()
	{
		// Only do this if its visualzied.
		if (visualized)
		{
			// Delay the visualization
			if (lastRun == 0) lastRun = Time.now;
			if (Time.now - lastRun > 10)
			{
				lastRun = Time.now;

				// If there is something in the stack? then call it.
				if (callstack.Count > 0) CallfromStack();

				// If the Recursion is finally done
				if (IsRunning && callstack.Count == 0)
				{
					diagnostic.endDiagnostic($"N = {diagnostic.nodeVisited}, E = {diagnostic.edgeVisited}, T = {diagnostic.traverseCalls}");
					Console.WriteLine("Recursive Generation Completed!");

					// apply the last calculated path AND draw it
					_lastCalculatedPath = shortestPath;
					draw();

					// turn the machine off!
					setRunning(false);
				}
			}
		}
	}
}

