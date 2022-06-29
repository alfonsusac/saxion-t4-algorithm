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
    //protected Stack<TraverseRecursively> callstack = new Stack<TraverseRecursively>();
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
	// --- Methods ---------------------------------------------
	public void StopFinding()
    {
		functionCollection = null;
		running = false;
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
			startTraverse(pFrom);
			return null;
		}
		else
		{
			startTraverse(pFrom);
			//traverse(pFrom, new List<Node>());

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
			_labelDrawer?.clearQueueLabels();


			// Initialize for every subclass
			initialize(start, dest);

			// check for completeness
			if (functionForCallingFromList == null) throw new NotImplementedException();
			if (functionCollection == null) throw new NotImplementedException();
	}

			protected virtual void initialize(Node start, Node dest) { throw new NotImplementedException(); }

	// ------------------------------------------------
		
		protected virtual void startTraverse(Node start)
		{
			throw new NotImplementedException();
		}

	// ------------------------------------------------
			
		public List<Node> GetShortestPath()
		{
				//if (running == true && visualized) throw new Exception("Machine is Still Running!!");
			_labelDrawer?.clearQueueLabels();

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
	protected virtual void traverse(Node n, List<Node> path, double dist = 0)
    {
		throw new NotImplementedException();
    }

	// ------------------------------------------------
	/////////////////////////////////////////////////////////////////////////////////////////////////////

		protected virtual void traverseThrough(Node child, List<Node> path, double dist)
		{
			throw new NotImplementedException();
		}

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// TRAVERSE THE PATH -> AFTER GENERATING
	// ------------------------------------------------
	protected int lastRun;
	bool paused = false;
	bool step = false;
	int updateTime = 1000;
	int updatePerFrame = 5;
	protected override void iterateSteps()
    {
		if( Input.GetKey(Key.O))
        {
			updateTime = 1;
        }
        else
        {
			updateTime = 1000;
        }

		if(Input.GetKeyDown(Key.P))
        {
			if (paused) paused = false;
			else paused = true;
			Console.WriteLine($"Pause = {paused} ");
        }
        if (paused)
        {
            if (Input.GetKeyDown(Key.I))
            {
				step = true;
			}
        }
        if (Input.GetKey(Key.U))
        {
			step = true;
			updatePerFrame = 1000;
			updateTime = 1;
        }
        else
        {
			updatePerFrame = 1;

		}


		// Only do this if its visualzied.
		if (visualized)
		{
			// Delay the visualization
			if (lastRun == 0) lastRun = Time.now;
			if( functionCollection != null)

				// when it is time,,
				if (((Time.now - lastRun > updateTime) && !paused) || step)
				{
					step = false;
					lastRun = Time.now;

					if(running)

						for(int i = 0; i < updatePerFrame; i++)
						{
							Console.WriteLine("Test");
						
							// If delay is not 1ms, then skip I to the update per frame.
							if(updateTime != 1) i = updatePerFrame - 1;

							// If there is something in the stack? then call it.
							if (_labelDrawer != null && funcCollectionCount() > 0)

								functionForCallingFromList();

							// If the Recursion is finally done
							if (funcCollectionCount() == 0)
							{
								diagnostic.end();

								Console.WriteLine(GetType().Name + ".Generate: Path generated.");

								GetShortestPath();

								// apply the last calculated path AND draw it
								_lastCalculatedPath = shortestPath;
								draw();

								// turn the machine off!
								running = false;

								break;
							}

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
	abstract internal class TraverseRecursively
	{
		public readonly Node currentNode;
		protected readonly List<Node> travelPath;
		public readonly double distance;
		protected readonly SamplePathFinder pf;
		protected int labelOpacity = 2;

		public TraverseRecursively(SamplePathFinder r, Node n, List<Node> l = null, double i = 0)
		{
			pf = r;

			currentNode = n;
			if (l != null) travelPath = new List<Node>(l); else travelPath = new List<Node>();
			distance = i;

			Add();
		}

		public virtual void Add()
        {
			throw new NotImplementedException();
        }

		public void Run()
		{
			// PRE TRAVERSAL-----------------------------------------
			// Count number of visits for label drawing (graphical)
			pf._labelDrawer.countVisits(currentNode);

			// Register a traverse to diagnostic counter (diagnostic)
			pf.diagnostic.traverse();

			// Add current node to travel path
			travelPath.Add(currentNode);

			// and finally draw the path
			pf._labelDrawer.drawPaths(travelPath, currentNode, labelOpacity);
			// END of PRE TRAVERSAL-----------------------------------------

			// Traverse the node
			pf.traverse(currentNode, travelPath, distance);
			
			// remove current path  from travel path (for back tracking)
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

