using System.Collections.Generic;


class PathFinder_BreadthFirst : SamplePathFinder
{

	public bool dopregenerate;

	public PathFinder_BreadthFirst(NodeGraph pGraph, bool visualized) : base(pGraph, visualized) 
	{
		dopregenerate = true;
	}

	// Data Structure necessary for BFS
	protected Dictionary<Node, Node> prevNodes;
	Node lastStartNode;

	protected Queue<Step> callqueue;
	protected virtual void CallfromStack() { callqueue.Dequeue().Run(); }

	// Overriding parent class
	protected override void initialize(Node start, Node dest)
    {
		// necessary to reset: for encapsulation
		callqueue = new Queue<Step>();
		functionForCallingFromList = CallfromStack;
		functionCollection = callqueue;

		prevNodes = new Dictionary<Node, Node>();

		prevNodes[start] = null;
	}

	public void pregenerate(Node start)
    {
		generate(start, null);
	}
	protected override List<Node> generate(Node pFrom, Node pTo)
    {
		if (pFrom == lastStartNode)
		{
			destination = pTo;

			generateShortestPath(pTo);

			return shortestPath; // and generate() will return shortestPath;
		}

		shortestPath = base.generate(pFrom, pTo);

		lastStartNode = pFrom;

		return shortestPath;
    }

	protected override List<Node> getShortestPath()
	{
		if (destination == null) return null; //for pregenerated paths

		Node curr = destination;

		List<Node> path = new List<Node>();

		path.Insert(0, curr);

		while (prevNodes.ContainsKey(curr))
		{
			//Console.WriteLine($"Curr {curr} <- Prev {prevNodes[curr]}");
			curr = prevNodes[curr];
			if (curr == null)
			{
				lastStartNode = path[0];
				return path;
			}
			path.Insert(0, curr);
		}

		return null;
	}



	// Overriding traverse method
	protected override void traverse(Node curr, List<Node> path, double dist = 0)
	{	
        if (!curr.isolated)
            // Iterate to every child
            foreach (Node child in curr.active_connections)

                if (!prevNodes.ContainsKey(child))
                {
					prevNodes[child] = curr;

					if (destination == null || (child != destination && !prevNodes.ContainsKey(destination)) )

						new Step(this, child, path, dist + 1);
				}

		iterateNext();
	}
	protected virtual void iterateNext()
    {
		if (!visualized)

			if (callqueue.Count > 0)
				// iterate to next loop
				CallfromStack();
			else
				getShortestPath();
	}
	
	protected override void traverseThrough(Node child, List<Node> path = null, double dist = 0)
	{

	}



	private List<Node> generateShortestPath(Node dest)
    {
		if(dest == null) return null; //for pregenerated paths

		Node curr = dest;
		List<Node> path = new List<Node>();
		path.Insert(0, curr);

		while (prevNodes.ContainsKey(curr))
        {
			//Console.WriteLine($"Curr {curr} <- Prev {prevNodes[curr]}");
			curr = prevNodes[curr];
			if (curr == null)
			{
				lastStartNode = path[0];
				return path;
			}
            path.Insert(0, curr);
		}
		return null;
    }





	internal class Step : TraverseRecursively
	{
		public Step(PathFinder_BreadthFirst r, Node n, List<Node> l = null, double i = 0)
			: base(r,n,l,i){ }

		public override void Add()
		{
			(pf as PathFinder_BreadthFirst).callqueue.Enqueue(this);
		}
	}


}

