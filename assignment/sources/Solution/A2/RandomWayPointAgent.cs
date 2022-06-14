using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class RandomWayPointAgent : SampleNodeGraphAgent
{
	public RandomWayPointAgent(NodeGraph pNodeGraph) : base(pNodeGraph)
	{ }

	protected override void onNodeClickHandler(Node pNode)
	{

		if (isMoving) return;
		// On Click on the nodes
		foreach (Node n in currentNode.connections)
		{
			if (n.location == pNode.location)
			{
				_targetsqueue.Enqueue(n);
				isMoving = true;
				Console.WriteLine("Neighbor node found");
				return;
			}
		}
		Node traverse = currentNode;

		// This Stack is for keeping track the path traveled.
		Stack<Node> stack = new Stack<Node>(); 
		Stack<Node> travelPath = new Stack<Node>(); I(travelPath);

		// This Dictionaries is for keeping track if its in the stack or if its visited.
		Dictionary<string, bool> visited = new Dictionary<string, bool>();

		// initialize
		stack.Push(traverse);
		visited[traverse.id] = true;



		while ( true )
		{
			// Pop the stack
			P($"");
			P($"| BEGIN: Popping stack");
			P($"|-------------------------");

			Node curr = stack.Pop();

            // , mark this node as traveled and put it into the stack.
            {
				travelPath.Push(curr);
				visited[curr.id] = true;
				_targetsqueue.Enqueue(curr);
            }

			P($"Node {curr.id}: Current Node: {curr.id}");
			P($"Node {curr.id}: TRAVEL PATH", travelPath, "<-");


			// If node is found
			if (curr == pNode)
			{
				P($"\nNode Found!!!\n");
				isMoving = true;
				_labelDrawer.drawQueueLabels();
				break;
			}

			P($"Node {curr.id}: Child Nodes: [ {string.Join(" ", curr.connections)} ]");

			// check if this node has any explorable nodes : HAVE NOT been visited before, and IS NOT IN THE STACK
			bool explorable = false;

			// for randomization
			Random r = new Random();
			List<Node> connectionsCopy = new List<Node>(curr.connections);

			for(int i = 0; i < curr.connections.Count; i++)
			//foreach (Node n in curr.connections)
			{

				// for randomization
				int removeAt = r.Next(connectionsCopy.Count);
				P($"removeAt {removeAt}");
				Node n = connectionsCopy[removeAt];
				connectionsCopy.RemoveAt(removeAt);

				P($"Node {curr.id}:  {curr.id}->[{n.id}]");

				if (!visited.ContainsKey(n.id))
				{
					P($"Node {curr.id}: [{n.id}] First time on node");
					visited[n.id] = false;
                }
                else
                {
					P($"Node {curr.id}: [{n.id}] Been checked before. Has it been visited?");
				}

				if (visited[n.id] == false)
				{
					P($"Node {curr.id}: [{n.id}] not yet visited");
					// if this node is explorable then push it to the potential stack.
					P($"Node {curr.id}: [{n.id}] Pushing to stack");
					stack.Push(n);
					//visited[n.location] = false;
					explorable = true;
                }
                else
                {
					P($"Node {curr.id}: [{n.id}] Been here before. Skip this");
				}
			}
			P($"Node {curr.id}: STACK", stack);
			
			if (!explorable)
			{
				// if no explorable node found, began back tracking
				travelPath.Pop();
				
				if (travelPath.Count > 0)
                {
					P($"|Node {curr.id}: No explorable node. Backtracking. Returning to {travelPath.Peek().id}");
					Node backtrack = travelPath.Pop();
					_targetsqueue.Enqueue(backtrack);
					stack.Push(backtrack);
                }
                else
                {
					Console.WriteLine($"\n.\n.\n.\n\nNode Not Found After a long search??? ");
					isMoving = true;
					break;
				}
			}
		}
	}
}
