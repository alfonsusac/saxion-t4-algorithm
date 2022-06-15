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
	public List<Node> ShortestPath { get { return shortestPath; } }

	// private Attribute
	public bool IsRunning { get { return running; } }
	bool running;

	// [] Visualization
	// this will enable visualizing the graphs and updating the frame
	// !! This will be run for every frame !!
	public readonly bool visualized = false;

	// These are required since the recursion now happens for every frame
	protected virtual void initialize(Node start, Node dest)
	{
		// Diagnostics
		diagnostic = new BasicDiagnostic();

		// necessary to reset: for encapsulation
		initializeForRecursion();

		// necessary to reset: initialization
		destination = dest;
		shortestPath = null;
		shortestDist = int.MaxValue;
		running = false;

		// necessary to reset: visualization
		initializeVisualization();

		// Resetting Graphic Stuff
		_labelDrawer.clearQueueLabels();
	}

	protected virtual void initializeForRecursion()
    {
		if(this is RecursivePathFinder)
        {
			functionForCallingFromList = CallfromStack;
			callstack = new Stack<TraverseRecursively>();
			functionCollection = callstack;
		}
	}


	protected override List<Node> generate(Node pFrom, Node pTo)
	{

		// Initialization
		initialize(pFrom, pTo);

		// Start diagnosting
		diagnostic.startDiagnostic($"{GetType()}");

		if (visualized)
		{
			running = true;
			generateWithVisual(pFrom);
		}
		else generateWithoutVisual(pFrom);

		// if visualized, return null first. Then render later
		// if not visualize, then shortestPath list would be populated.
		return shortestPath;
	}

	protected virtual void generateWithVisual(Node start)
    {
		new TraverseRecursively(this, start);
	}
	protected virtual void generateWithoutVisual(Node start)
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

		path.Add(n); diagnostic.nodeVisited++; _labelDrawer.countVisits(n);

		if (n.connections.Count != 0)
        {
            // Add current node to the traveled path FOR VISUALIZATION
            

            // Iterate to every unvisted child
            foreach (Node child in n.connections)
				if (!path.Contains(child))
					traverseThrough(child, path, dist + 1);

		}

	}

	protected void markAsShortest(Node n, int dist, List<Node> path)
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
		readonly protected RecursivePathFinder pf;

		public TraverseRecursively(RecursivePathFinder r, Node n, List<Node> l = null, int i = 0)
		{
			pf = r;
			Add(this);

			currentNode = n;
			if (l != null) travelPath = new List<Node>(l); else travelPath = new List<Node>();
			distance = i;

		}
		public virtual void Add(TraverseRecursively t)
        {
			pf.callstack.Push(t);
        }

        public virtual void Run(){
			pf.traverse(currentNode, travelPath, distance); 
		}

        public override string ToString()
        {
			string s;
			if (travelPath == null) s = $"= Node: {currentNode} | List: null | Distance: {distance}";
			else { s = $"= Node: {currentNode} | List: "; travelPath.ForEach(e => s += e + " "); s += $"({travelPath.Count}) | Distance: {distance}"; }
			return s;
        }

	}

	//////////////////////////////////////////////////////////////////////////////
	// for visualization
	protected int lastRun;
	protected Stack<TraverseRecursively> callstack = new Stack<TraverseRecursively>();
	protected IEnumerable<TraverseRecursively> functionCollection;

	protected void initializeVisualization()
    {
		functionForCallingFromList = CallfromStack;
	}

	protected virtual void CallfromStack() {
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
			if (Time.now - lastRun > 1 && functionCollection != null)
			{
				lastRun = Time.now;

				// If there is something in the stack? then call it.
				if (funcCollectionCount() > 0) 
					functionForCallingFromList();

				// If the Recursion is finally done
				if (running == true && funcCollectionCount() == 0)
				{
					diagnostic.endDiagnostic($"N = {diagnostic.nodeVisited}, E = {diagnostic.edgeVisited}, T = {diagnostic.traverseCalls}");
					Console.WriteLine("Recursive Generation Completed!");
					Console.WriteLine(funcCollectionCount());

					// apply the last calculated path AND draw it
					returnShortestPath();

					// turn the machine off!
					running = false;
				}
			}
		}
	}
	protected virtual int funcCollectionCount()
    {
		return functionCollection.Count();
    }

	protected virtual void returnShortestPath()
    {
		_lastCalculatedPath = shortestPath;
		draw();
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
		public bool disabled = true;

		public BasicDiagnostic()
        {
			nodeVisited = 0;
			edgeVisited = 0;
			traverseCalls = 0;
		}

		// Public Method
		public void startDiagnostic(string s)
		{
            if (!disabled)
            {
				Console.WriteLine($"\n>---------------\n/ Start: {s}");
				sw.Restart();
            }
		}

		public void endDiagnostic(string s = "")
		{
            if (!disabled)
            {
				sw.Stop();
				TimeSpan ts = sw.Elapsed;
				Console.WriteLine($"\\ End: {ts} (N = {nodeVisited}, E = {edgeVisited}, T = {traverseCalls})\n>---------------");
				elapses.Add(ts);
				Console.WriteLine($"\\ Avg: {Average(elapses)}\n>---------------");
            }
		}
	}
}

