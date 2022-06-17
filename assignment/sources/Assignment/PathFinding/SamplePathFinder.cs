using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/**
 * An example of a PathFinder implementation which completes the process by rolling a die 
 * and just returning the straight-as-the-crow-flies path if you roll a 6 ;). 
 */
class SamplePathFinder : PathFinder	{

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Protected Methods to be inheriteds
	// ------------------------------------------------
	protected Node destination { get; set; }
	protected List<Node> shortestPath;  
	protected bool running;				// Running Status for Frame Delegation
	protected Action functionForCallingFromList;
	protected Stack<TraverseRecursively> callstack = new Stack<TraverseRecursively>();
	protected IEnumerable<TraverseRecursively> functionCollection;
	///
	/////////////////////////////////////////////////////////////////////////////////////////////////////


	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Public
	/// 
	// --- Attributes ---------------------------------------------
	public Node Destination { get { return destination; } }
	public readonly bool visualized = false;
	public bool IsRunning { get { return running; } }
	// --- Constructors ---------------------------------------------
	public SamplePathFinder(NodeGraph pGraph, bool visualized) : base(pGraph) { 
	
		this.visualized = visualized;
		diagnostic = new BasicDiagnostic();
	}
	///
	/////////////////////////////////////////////////////////////////////////////////////////////////////


	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// GENERATE THE PATH
	// ------------------------------------------------
	protected override List<Node> generate(Node pFrom, Node pTo)
	{
		// Initialization
		_initialize(pFrom, pTo);

		// Start diagnosting
		diagnostic.start($"{GetType()}");

		if (visualized)
		{
			running = true;
			traverse(pFrom, new List<Node>());

			return null;
		}
		else
		{
			traverse(pFrom, new List<Node>());

			return GetShortestPath();
		}
	}
	// ------------------------------------------------
	/////////////////////////////////////////////////////////////////////////////////////////////////////

		void _initialize(Node start, Node dest)
		{
			// Resetting variable
			destination = dest; running = false;

			// Resetting Graphic Stuff
			if (_labelDrawer != null) _labelDrawer.clearQueueLabels();


			// Initialize for every subclass
			initialize(start, dest);

			// check for completeness
			if (functionForCallingFromList == null) throw new NotImplementedException();
			if (callstack == null) throw new NotImplementedException();
			if (functionCollection == null) throw new NotImplementedException();
	}

			protected virtual void initialize(Node start, Node dest) { throw new NotImplementedException(); }

	// ------------------------------------------------
			
		public List<Node> GetShortestPath()
		{
				if (running == true && visualized) throw new Exception("Machine is Still Running!!");

			List<Node> generatedPath = getShortestPath();

				if (generatedPath == null) Console.WriteLine("Shortest Path is Not Found!");

			return generatedPath;
		}

			protected virtual List<Node> getShortestPath()
			{
				throw new NotImplementedException();
			}

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// TRAVERSE THE PATH -> AFTER GENERATING
	// ------------------------------------------------
	protected virtual void traverse(Node n, List<Node> path, int dist = 0)
    {
		throw new NotImplementedException();
    }

	// ------------------------------------------------
	/////////////////////////////////////////////////////////////////////////////////////////////////////

		protected virtual void traverseThrough(Node child, List<Node> path, int dist)
		{
			throw new NotImplementedException();
		}

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// TRAVERSE THE PATH -> AFTER GENERATING
	// ------------------------------------------------
	protected int lastRun;
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
					diagnostic.end();

					Console.WriteLine(GetType().Name + ".Generate: Path generated.");

					getShortestPath();

					// apply the last calculated path AND draw it
					_lastCalculatedPath = shortestPath;

					draw();

					// turn the machine off!
					running = false;
				}
			}
		}
	}
	protected virtual int funcCollectionCount() { return functionCollection.Count(); }
	// ------------------------------------------------
	/////////////////////////////////////////////////////////////////////////////////////////////////////


	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// The "Function" Class
	// ------------------------------------------------
	internal class TraverseRecursively
	{
		readonly Node currentNode;
		readonly List<Node> travelPath;
		readonly int distance;
		readonly protected SamplePathFinder pf;

		public TraverseRecursively(SamplePathFinder r, Node n, List<Node> l = null, int i = 0)
		{
			pf = r;
			Add(this);

			currentNode = n;
			if (l != null) travelPath = new List<Node>(l); else travelPath = new List<Node>();
			distance = i;
		}
		public virtual void Add(TraverseRecursively t)
			{ pf.callstack.Push(t); }

		public void Run()
		{
			pf._labelDrawer.countVisits(currentNode);
			pf.diagnostic.traverse();
			travelPath.Add(currentNode);
			pf._labelDrawer.drawPaths(travelPath, currentNode);
			pf.traverse(currentNode, travelPath, distance);
			travelPath.RemoveAt(travelPath.Count - 1);
		}

		public override string ToString()
		{
			string s;
			if (travelPath == null) s = $"= Node: {currentNode} | List: null | Distance: {distance}";
			else { s = $"= Node: {currentNode} | List: "; travelPath.ForEach(e => s += e + " "); s += $"({travelPath.Count}) | Distance: {distance}"; }
			return s;
		}

	}
	// ------------------------------------------------
	/////////////////////////////////////////////////////////////////////////////////////////////////////




	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// DIAGNOSTICS
	/// ------------------------------------------------
	/// starts() timer and stops() timer
	protected BasicDiagnostic diagnostic;

	/// New Class
	internal class BasicDiagnostic
	{
		// ---- Timing Attributes -------------------------------------
		Stopwatch sw = new Stopwatch();

		List<TimeSpan> elapses = new List<TimeSpan>();

		private static TimeSpan Average(IEnumerable<TimeSpan> spans) => new TimeSpan(Convert.ToInt64(spans.Average(t => t.Ticks)));

		// ---- Initialization -------------------------------------
		int nodeVisited = 0;

		int edgeVisited = 0;

		int traverseCalls = 0;

		public bool disabled = true;

		// ---- Constructor -------------------------------------

		public BasicDiagnostic()
		{
			nodeVisited = 0;
			edgeVisited = 0;
			traverseCalls = 0;
		}

		// ---- Public Methods -------------------------------------
		public void start(string s = "")
		{
			if (!disabled)
			{
				Console.WriteLine($"\n>---------------\n/ Start! {s}");
				sw.Restart();
			}
		}

		public void end(string s = "")
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
		public void traverse()
        {
			traverseCalls++;
        }
		public void visitNode()
        {
			nodeVisited++;
        }
		public void visitEdge()
        {
			edgeVisited++;
        }
	}
}

