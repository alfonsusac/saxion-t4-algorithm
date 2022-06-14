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
	protected BasicDiagnostic diagnostic;

	// Attributes to be inherited
	protected Node destination { get; set; }
	protected int shortestDist = int.MaxValue;
	protected List<Node> shortestPath;

	// private Attribute
	bool running;
	public bool IsRunning { get { return running; } }

	// [] Visualization
	// this will enable visualizing the graphs and updating the frame
	// !! This will be run for every frame !!
	protected readonly bool visualized = true;

	// These are required since the recursion now happens for every frame
	protected virtual void initialize(Node dest)
	{
		// Diagnostics
		diagnostic = new BasicDiagnostic();

		// necessary to reset
		destination = dest;
		shortestPath = null;
		shortestDist = int.MaxValue;
		callstack = new Stack<TraverseRecursively>();
		running = false;
		initializeVisualization();
		functionForCallingFromList = CallfromStack;

		// Graphic Stuff
		_labelDrawer.clearQueueLabels();
	}

	protected override List<Node> generate(Node pFrom, Node pTo)
	{

		// Initialization
		initialize(pTo);

		// Start diagnosting
		diagnostic.startDiagnostic("Recursive Path Finder");

		if (visualized)	processWithVisual(pFrom);
		else processWithoutVisual(pFrom);

		// if visualized, return null first. Then render later
		// if not visualize, then shortestPath list would be populated.
		return shortestPath;
	}

	protected virtual void processWithVisual(Node start)
    {
		running = true;
		new TraverseRecursively(this, start);
	}
	protected virtual void processWithoutVisual(Node start)
    {
		traverse(start, null);
    }

	protected virtual void traverse(Node n, List<Node> path, int dist = 0)
    {
		// Deny entry if destination is null
		if (destination == null) return;
		diagnostic.traverseCalls++;

		// Initialize the List if it is called for the first time
		if (path == null) path = new List<Node>();
		_labelDrawer.drawPaths(path, n);


		// If this node is the final node, 
		if (n == destination)
        {
			// If the distance to this node is the shortest node.
			if( dist < shortestDist)

				// then copy path to the global shortestpath
				markAsShortest(n, dist, path);
			return;
        }

		if (n.connections.Count != 0)
        {
			// Add current node to the traveled path
			path.Add(n); diagnostic.nodeVisited++; _labelDrawer.countVisits(n);

            // Iterate to every unvisted child
            foreach (Node child in n.connections)
				if (!path.Contains(child))
					traverseThrough(child, path, dist + 1);

			// Remove current node from the traveled path as we need to traverse back
			path.RemoveAt(path.Count - 1);
		}

	}

	protected virtual void markAsShortest(Node n, int dist, List<Node> path)
    {
		//Console.WriteLine($"[{dist}] Traversing ... : Found Shortest Destination! {shortestPath.Count}");
		shortestDist = dist;
		shortestPath = new List<Node>(path);
		shortestPath.Add(n);
    }


	protected virtual void traverseThrough(Node child, List<Node> path, int dist)
    {
		if (visualized) new TraverseRecursively(this, child, path, dist + 1); else traverse(child, path, dist + 1);
		diagnostic.edgeVisited++;
	}


    internal class TraverseRecursively
	{
		
		readonly Node currentNode;
		readonly List<Node> travelPath;
		readonly int distance;
        readonly RecursivePathFinder recpathfinder;

		public TraverseRecursively(RecursivePathFinder r, Node n, List<Node> l = null, int i = 0)
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

	//////////////////////////////////////////////////////////////////////////////
	// for visualization
	protected int lastRun;
	protected Stack<TraverseRecursively> callstack = new Stack<TraverseRecursively>();

	protected void initializeVisualization()
    {
		functionForCallingFromList = CallfromStack;
	}

	protected void CallfromStack() {
		Console.WriteLine("CallfromStack");
		callstack.Pop().Run(); 
	}

	protected Action functionForCallingFromList;

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
				if (callstack.Count > 0) functionForCallingFromList();

				// If the Recursion is finally done
				if (running == true && callstack.Count == 0)
				{
					diagnostic.endDiagnostic($"N = {diagnostic.nodeVisited}, E = {diagnostic.edgeVisited}, T = {diagnostic.traverseCalls}");
					Console.WriteLine("Recursive Generation Completed!");

					// apply the last calculated path AND draw it
					_lastCalculatedPath = shortestPath;
					draw();

					// turn the machine off!
					running = false;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////
	// HELPER METHOD
	internal void setRunning(bool status) { running = status; }


	//////////////////////////////////////////////////////////////////////////////
	// for diagnostics

	internal class BasicDiagnostic
    {
		// Timing Attributes
		Stopwatch sw = new Stopwatch();
		List<TimeSpan> elapses = new List<TimeSpan>();
		private static TimeSpan Average(IEnumerable<TimeSpan> spans) => new TimeSpan(Convert.ToInt64(spans.Average(t => t.Ticks)));

		// Initialization
		public int nodeVisited = 0;
		public int edgeVisited = 0;
		public int traverseCalls = 0;

		public BasicDiagnostic()
        {
			nodeVisited = 0;
			edgeVisited = 0;
			traverseCalls = 0;
		}

		// Public Method
		public void startDiagnostic(string s)
		{
			Console.WriteLine($"\n>---------------\n/ Start: {s}");
			sw.Restart();
		}

		public void endDiagnostic(string s = "")
		{
			sw.Stop();
			TimeSpan ts = sw.Elapsed;
			Console.WriteLine($"\\ End: {ts} (N = {nodeVisited}, E = {edgeVisited}, T = {traverseCalls})\n>---------------");
			elapses.Add(ts);
			Console.WriteLine($"\\ Avg: {Average(elapses)}\n>---------------");
		}
	}
}

