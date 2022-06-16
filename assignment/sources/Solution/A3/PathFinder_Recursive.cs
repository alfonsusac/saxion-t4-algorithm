using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class PathFinder_Recursive : PathFinder
{

	public PathFinder_Recursive(NodeGraph pGraph, bool visualized) : base(pGraph) {
		this.visualized = visualized;
	}

	// Diagnostics
	protected BasicDiagnostic diagnostic;

	// Attributes to be inherited
	protected Node destination { get; set; }
	protected int shortestDist = int.MaxValue;
	protected List<Node> shortestPath;

	// for recursive stuff
	private List<Node> tempShortestPath;
	bool running;
	public bool IsRunning { get { return running; } }

	// [] Visualization
	// this will enable visualizing the graphs and updating the frame
	// !! This will be run for every frame !!
	public readonly bool visualized = false;

	// These are required since the recursion now happens for every frame
	protected virtual void _initialize(Node start, Node dest)
	{
		// Diagnostics
		diagnostic = new BasicDiagnostic();

		// necessary to reset: for encapsulation
		initializeForRecursion();

		// necessary to reset: initialization
		destination = dest;
		shortestDist = int.MaxValue;
		running = false;

		 // Resetting Graphic Stuff
		if(_labelDrawer != null)
			_labelDrawer.clearQueueLabels();

		if(GetType() != typeof(PathFinder_Recursive))

			initialize(start, dest);
	}
	protected virtual void initialize(Node start, Node dest)
    {
		throw new NotImplementedException();
    }

	protected virtual void initializeForRecursion()
    {
		if(GetType() == typeof(PathFinder_Recursive))
        {
			tempShortestPath = null;

			functionForCallingFromList = CallfromStack;
			callstack = new Stack<TraverseRecursively>();
			functionCollection = callstack;
		}
	}


	protected override List<Node> generate(Node pFrom, Node pTo)
	{

		// Initialization
		_initialize(pFrom, pTo);

		// Start diagnosting
		diagnostic.startDiagnostic($"{GetType()}");

		if (visualized)
		{
			running = true;

			generateWithVisual(pFrom);
		}
		else
		{
			generateWithoutVisual(pFrom);

			getShortestPath();
		}

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
			if( dist < shortestDist )

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

		path.RemoveAt(path.Count-1);

	}

	protected virtual void traverseThrough(Node child, List<Node> path, int dist)
	{
		if (visualized) new TraverseRecursively(this, child, path, dist + 1); else traverse(child, path, dist + 1);
		diagnostic.edgeVisited++;
	}
	// The way to get shortest path based on the algorithm.
	public virtual List<Node> getShortestPath()
	{
		if (GetType() == typeof(PathFinder_Recursive)) // bcs recursive path finder has to manually udpate ShortestPath.
			shortestPath = tempShortestPath;

		if (shortestPath == null) 
			Console.WriteLine("Shortest Path is Not Found!");

		return shortestPath;
	}

	private void drawSolution()
    {
		_lastCalculatedPath = shortestPath;
		draw();
	}

	private void markAsShortest(Node n, int dist, List<Node> path)
    {
        shortestDist = dist;
		tempShortestPath = new List<Node>(path);
		tempShortestPath.Add(n);	
    }





    internal class TraverseRecursively
	{
		readonly Node currentNode;
		readonly List<Node> travelPath;
		readonly int distance;
		readonly protected PathFinder_Recursive pf;

		public TraverseRecursively(PathFinder_Recursive r, Node n, List<Node> l = null, int i = 0)
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
			if (Time.now - lastRun > 0 && functionCollection != null)
			{
				lastRun = Time.now;

				// If there is something in the stack? then call it.
				if (_labelDrawer != null && funcCollectionCount() > 0) 
					functionForCallingFromList();

				// If the Recursion is finally done
				if (running == true && funcCollectionCount() == 0)
				{
					diagnostic.endDiagnostic($"N = {diagnostic.nodeVisited}, E = {diagnostic.edgeVisited}, T = {diagnostic.traverseCalls}");
					Console.WriteLine("Recursive Generation Completed!");
					Console.WriteLine(funcCollectionCount());

					getShortestPath();

					// apply the last calculated path AND draw it
					drawSolution();

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

