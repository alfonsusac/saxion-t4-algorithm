using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class BreadthFirstPathFinder : PathFinder
{

	public BreadthFirstPathFinder(NodeGraph pGraph) : base(pGraph) { }

	// Diagnostics
	public int nodeVisited = 0;
	public int edgeVisited = 0;
	public int traverseCalls = 0;

	// Attributes
	Node destination { get; set; }
	bool running;

	// [] Visualization
	// this will enable visualizing the graphs and updating the frame
	bool visualized = true;

	private void init(Node dest)
	{
		_labelDrawer.clearQueueLabels();
		prevNode = new Dictionary<Node, Node>();
		destination = dest;
		callstack = new Queue<Step>();
		nodeVisited = 0;
		edgeVisited = 0;
		traverseCalls = 0;
		running = false;
	}

	protected override List<Node> generate(Node pFrom, Node pTo)
	{
		init(pTo);
		startDiagnostic("BFS Path Finder");
		if (visualized)
		{
			running = true;
			prevNode[pFrom] = null;
			new Step(this, pFrom);
			return null;
		}
		else
		{
			traverse(pFrom, null);
			return generateShortestPath(pTo);
		}
	}

	Dictionary<Node, Node> prevNode;

	private void traverse(Node curr, List<Node> path = null, int dist = 0)
	{
		if (destination == null) return;
		traverseCalls++;

		// Initialize the List if it is called for the first time
		if (path == null) path = new List<Node>();
		_labelDrawer.drawPaths(path, curr);

		if (curr.connections.Count != 0)
		{
			// Add current node to the traveled path
			path.Add(curr); nodeVisited++; _labelDrawer.countVisits(curr);

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
	private void traverseThrough(Node child, List<Node> path, int dist)
	{
		if (visualized) new Step(this, child, path, dist + 1); else traverse(child, path, dist + 1);
		edgeVisited++;
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

	int lastRun;
	Queue<Step> callstack = new Queue<Step>();
	public void CallfromStack() { callstack.Dequeue().Run(); }

	//////////////////////////////////////////////////////////////////////////////
	// for visualization
	static NodeLabelDrawer _labelDrawer;
	public void SetLabelDrawer(NodeLabelDrawer n) { _labelDrawer = n; }

	protected override void iterateSteps()
	{
		if (visualized)
		{
			if (lastRun == 0) lastRun = Time.now;
			if (Time.now - lastRun > 10)
			{
				lastRun = Time.now;
				if (callstack.Count > 0) CallfromStack();
				if (running == true && callstack.Count == 0)
				{
					endDiagnostic($"N = {nodeVisited}, E = {edgeVisited}, T = {traverseCalls}");
					Console.WriteLine("BFS Generation Completed!");
					_lastCalculatedPath = generateShortestPath(destination);
					draw();
					running = false;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////
	// for diagnostics
	Stopwatch sw = new Stopwatch();
	List<TimeSpan> elapses = new List<TimeSpan>();
	private static TimeSpan Average(IEnumerable<TimeSpan> spans) => new TimeSpan(Convert.ToInt64(spans.Average(t => t.Ticks)));
	public void startDiagnostic(string s)
	{
		Console.WriteLine($"\n>---------------\n/ Start: {s}");
		sw.Restart();
	}

	public void endDiagnostic(string s = "")
	{
		sw.Stop();
		TimeSpan ts = sw.Elapsed;
		Console.WriteLine($"\\ End: {ts} ({s})\n>---------------");
		elapses.Add(ts);
		Console.WriteLine($"\\ Avg: {Average(elapses)}\n>---------------");
	}


}

