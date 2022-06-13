using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class RecursivePathFinder : PathFinder
{

	public RecursivePathFinder(NodeGraph pGraph) : base(pGraph) { }

	// Diagnostics
	public int nodeVisited = 0;
	public int edgeVisited = 0;
	public int traverseCalls = 0;

	// Attributes
	Node destination { get; set; }
	int shortestDist = int.MaxValue;
	List<Node> shortestPath;
	bool running;

	// [] Visualization
	// this will enable visualizing the graphs and updating the frame
	bool visualized = true;

	private void init(Node dest)
	{
		_labelDrawer.clearQueueLabels();
		shortestPath = null;
		shortestDist = int.MaxValue;
		destination = dest;
		callstack = new Stack<Traverse>();
		nodeVisited = 0;
		edgeVisited = 0;
		traverseCalls = 0;
		running = false;
	}

	protected override List<Node> generate(Node pFrom, Node pTo)
	{
		init(pTo);
		startDiagnostic("Recursive Path Finder");
		if (visualized)
		{
			running = true;
			new Traverse(this, pFrom);
		}
		else traverse(pFrom, null);
		return shortestPath;
	}



	private void traverse(Node n, List<Node> path, int dist = 0)
    {
		if (destination == null) return;
		traverseCalls++;

		// Initialize the List if it is called for the first time
		if (path == null) path = new List<Node>();
		_labelDrawer.drawPaths(path, n);


		// If this node is the final node, 
		if (n == destination)
        {
			if( dist < shortestDist)
				// then copy path to the global shortestpath
				markAsShortest(n, dist, path);
			return;
        }

		if (n.connections.Count != 0)
        {
			// Add current node to the traveled path
			path.Add(n); nodeVisited++; _labelDrawer.countVisits(n);

            // Iterate to every child
            foreach (Node child in n.connections)
				if (!path.Contains(child))
					traverseThrough(child, path, dist + 1);

			// Remove current node from the traveled path as we need to traverse back
			path.RemoveAt(path.Count - 1);
		}

	}

    private void markAsShortest(Node n, int dist, List<Node> path)
    {
		//Console.WriteLine($"[{dist}] Traversing ... : Found Shortest Destination! {shortestPath.Count}");
		shortestDist = dist;
		shortestPath = new List<Node>(path);
		shortestPath.Add(n);
    }
	private void traverseThrough(Node child, List<Node> path, int dist)
    {
		if (visualized) new Traverse(this, child, path, dist + 1); else traverse(child, path, dist + 1);
		edgeVisited++;
	}

    internal class Traverse
	{
		
		readonly Node currentNode;
		readonly List<Node> travelPath;
		readonly int distance;
        readonly RecursivePathFinder recpathfinder;

		public Traverse(RecursivePathFinder r, Node n, List<Node> l = null, int i = 0)
		{
			currentNode = n;
			if (l != null) travelPath = new List<Node>(l); else travelPath = new List<Node>();
			distance = i;
			recpathfinder = r;

			r.callstack.Push(this);
		}
        public override string ToString()
        {
			string s;
			if (travelPath == null) s = $"= Node: {currentNode} | List: null | Distance: {distance}";
			else { s = $"= Node: {currentNode} | List: "; travelPath.ForEach(e => s += e + " "); s += $"({travelPath.Count}) | Distance: {distance}"; }
			return s;
        }
        public void Run(){ recpathfinder.traverse(currentNode, travelPath, distance); }

	}

	int lastRun;
	Stack<Traverse> callstack = new Stack<Traverse>();
	public void CallfromStack() { callstack.Pop().Run(); }

	//////////////////////////////////////////////////////////////////////////////
	// for visualization
	static NodeLabelDrawer _labelDrawer;
	public void SetLabelDrawer(NodeLabelDrawer n){_labelDrawer = n;	}

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
					Console.WriteLine("Recursive Generation Completed!");
					_lastCalculatedPath = shortestPath;
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

