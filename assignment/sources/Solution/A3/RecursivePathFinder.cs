using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * An example of a PathFinder implementation which completes the process by rolling a die 
 * and just returning the straight-as-the-crow-flies path if you roll a 6 ;). 
 */
class RecursivePathFinder : PathFinder
{

	public RecursivePathFinder(NodeGraph pGraph) : base(pGraph) { }

	//internal class NodeScore
 //   {
	//	internal Node Node { get; set; }
	//	internal int SmallestScore { get; set; }
	//	internal bool isVisited { get; set; }
	//	internal NodeScore(Node _node, int _smallestScore, bool _visited = false) { Node = _node; SmallestScore = _smallestScore; _visited = Visited;  }
	//	internal void Visits() { isVisited = true; }
 //   }

	protected override List<Node> generate(Node pFrom, Node pTo)
	{
		destination = pTo;
		_labelDrawer.clearQueueLabels();
		shortestPath = null;
		shortestDist = int.MaxValue;
		nodeVisited = 0;
		edgeVisited = 0;
		traverseCalls = 0;

		//at this point you know the FROM and TO node and you have to write an 
		//algorithm which finds the path between them
		//Console.WriteLine("For now I'll just roll a die for a random path!!");

		//int dieRoll = Utils.Random(1, 7);
		//Console.WriteLine("I rolled a ..." + dieRoll);

		//if (dieRoll == 6)
		//{
		//	Console.WriteLine("Yes 'random' path found!!");
		//	return new List<Node>() { pFrom, pTo };
		//}
		//else
		//{
		//	Console.WriteLine("Too bad, no path found !!");
		//	return null;
		//}
		startDiagnostic("Recursive Path Finder");
		// traversing every node
		traverse(pFrom, pTo);

		endDiagnostic($"N = {nodeVisited}, E = {edgeVisited}, T = {traverseCalls}");

		return shortestPath;
	}

	Node destination;
	int shortestDist = int.MaxValue;
	List<Node> shortestPath;
	int nodeVisited = 0;
	int edgeVisited = 0;
	int traverseCalls = 0;

	private void traverse(Node n, in Node dest, List<Node> path = null, int dist = 0)
    {
		traverseCalls++;
		if (path == null) path = new List<Node> ();

		// If this node is the final node, 
		if (n == destination && dist < shortestDist)
        {
			Console.WriteLine($"[{dist}] Traversing ... : Found Destination!");
			// then copy path to the global shortestpath
			shortestDist = dist;
			shortestPath = new List<Node>(path);
			shortestPath.Add(n);
			return;
        }

		// If this node has been visited, then skip this node.
		// If this node has node children, then skip this node.
		// so
		// if nothing wrong,  if this node has NOT been visited AND has Childrens,
		// then traverse through its children
		if (!path.Contains(n) && n.connections.Count != 0)
        {
			// Add current node to the traveled path
			path.Add(n);
			nodeVisited++;
			// Iterate to every child
			foreach (Node child in n.connections)
            {
				_labelDrawer.drawConnections(n, child);
				edgeVisited++;
				//Console.WriteLine($"[{dist}] Traversing ... {n}: Foreach");
				traverse(child, dest, path, dist + 1);
            }

			// Remove current node from the traveled path as we need to traverse back
			path.RemoveAt(path.Count - 1);
		}

    }

	NodeLabelDrawer _labelDrawer;
	public void SetLabelDrawer(NodeLabelDrawer n)
	{
		_labelDrawer = n;
	}

	// for diagnostics
	Stopwatch sw = new Stopwatch();
	public void startDiagnostic(string s)
    {
		Console.WriteLine($"\n>---------------\n/ Start: {s}");
		sw.Restart();
    }

	public void endDiagnostic(string s = "")
    {
		sw.Stop();
		TimeSpan ts = sw.Elapsed;
		Console.WriteLine($"\\ End: {sw.Elapsed} ({s})\n>---------------");
	}

}

