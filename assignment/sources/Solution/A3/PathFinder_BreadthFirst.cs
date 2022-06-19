using System.Collections.Generic;


class PathFinder_BreadthFirst : SamplePathFinder
{

	public PathFinder_BreadthFirst(NodeGraph pGraph, bool visualized, bool pregenerate) : base(pGraph, visualized) 
	{ }

	// Data Structure necessary for BFS
	Dictionary<Node, Node> prevNode;
	Node lastStartNode;

	protected void CallfromStack() { callqueue.Dequeue().Run(); }
	protected Queue<Step> callqueue;

	// Overriding parent class
	protected override void initialize(Node start, Node dest)
    {
		// necessary to reset: for encapsulation
		callqueue = new Queue<Step>();
		functionForCallingFromList = CallfromStack;
		functionCollection = callqueue;

		prevNode = new Dictionary<Node, Node>();

		prevNode[start] = null;
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

		while (prevNode.ContainsKey(curr))
		{
			//Console.WriteLine($"Curr {curr} <- Prev {prevNode[curr]}");
			curr = prevNode[curr];
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
	protected override void traverse(Node curr, List<Node> path, int dist = 0)
	{	
        if (curr.connections.Count != 0)
            // Iterate to every child
            foreach (Node child in curr.connections)

                if (!prevNode.ContainsKey(child))
                {
					prevNode[child] = curr;

					if (destination == null || (child != destination && !prevNode.ContainsKey(destination)) )

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
	
	protected override void traverseThrough(Node child, List<Node> path = null, int dist = 0)
	{
		new Step(this, child, path, dist + 1);

		diagnostic.visitEdge();
	}



	private List<Node> generateShortestPath(Node dest)
    {
		if(dest == null) return null; //for pregenerated paths

		Node curr = dest;
		List<Node> path = new List<Node>();
		path.Insert(0, curr);

		while (prevNode.ContainsKey(curr))
        {
			//Console.WriteLine($"Curr {curr} <- Prev {prevNode[curr]}");
			curr = prevNode[curr];
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
		public Step(PathFinder_BreadthFirst r, Node n, List<Node> l = null, int i = 0)
			: base(r,n,l,i){ }

		public override void Add(TraverseRecursively t)
		{
			(pf as PathFinder_BreadthFirst).callqueue.Enqueue(t as Step);
		}
	}


}

