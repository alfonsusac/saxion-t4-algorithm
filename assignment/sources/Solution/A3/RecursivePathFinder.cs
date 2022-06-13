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
	}

	protected override List<Node> generate(Node pFrom, Node pTo)
	{
		init(pTo);
		startDiagnostic("Recursive Path Finder");
		new Traverse(this, pFrom);
        endDiagnostic($"N = {nodeVisited}, E = {edgeVisited}, T = {traverseCalls}");
		return shortestPath;
	}



	private void traverse(Node n, List<Node> path, int dist = 0)
    {
		if (destination == null) return;

		// Initialize the List if it is called for the first time
		traverseCalls++; 

		// If this node is the final node, 
		if (n == destination && dist < shortestDist)
        {
			// then copy path to the global shortestpath
			shortestDist = dist;
			shortestPath = new List<Node>(path);
			shortestPath.Add(n);
			Console.WriteLine($"[{dist}] Traversing ... : Found Shortest Destination! {shortestPath.Count}");
			return;
        }
        else if (n == destination)
        {
			Console.WriteLine($"[{dist}] Traversing ... : Found Destination! ZZ {path.Count + 1}");
		}

		// If this node has been visited, then skip this node.
		// If this node has node children, then skip this node.
		// so
		// if nothing wrong,  if this node has NOT been visited AND has Childrens,
		// then traverse through its children
		//if (!path.Contains(n) && n.connections.Count != 0)
		if (n.connections.Count != 0)
        {
			// Add current node to the traveled path
			path.Add(n); nodeVisited++;
			_labelDrawer.countVisits(n);

			// Iterate to every child
			foreach (Node child in n.connections)
            {
				Console.WriteLine($"[{dist}] Traversing ... : Paths!");
				path.ForEach(e => Console.Write(e + " ")); Console.WriteLine("");

				if (!path.Contains(child))
                {
                    _labelDrawer.drawConnections(n, child, dist + 1);
					new Traverse(this, child, path, dist + 1); edgeVisited++;
                }
            }

			// Remove current node from the traveled path as we need to traverse back
			path.RemoveAt(path.Count - 1);
		}

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

			//Console.WriteLine("=== Traaverse Func queued! ===");
			//if (l == null) Console.WriteLine($"= Node: {n} | List: null | Distance: {i}");
			//else { Console.Write($"= Node: {n} | List: "); travelPath.ForEach(e => Console.Write(e + " ")); Console.WriteLine($"({travelPath.Count}) | Distance: {i}"); }
		}
		public void Run(){ recpathfinder.traverse(currentNode, travelPath, distance); }

	}

	int lastRun;
	Stack<Traverse> callstack = new Stack<Traverse>();
	public void CallfromStack() { callstack.Pop().Run(); }

	protected override void iterateSteps()
	{
		if (lastRun == 0) lastRun = Time.now;
		if (Time.now - lastRun > 10)
        {
            _labelDrawer.clearQueueLabels();
            lastRun = Time.now;
			if(callstack.Count > 0) CallfromStack();
		}
	}

	//////////////////////////////////////////////////////////////////////////////
	// for visualization
	NodeLabelDrawer _labelDrawer;
	public void SetLabelDrawer(NodeLabelDrawer n)
	{
		_labelDrawer = n;
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

