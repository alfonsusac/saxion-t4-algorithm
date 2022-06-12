using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class RandomWayPointAgent : NodeGraphAgent
{
	private NodeLabelDrawer _labelDrawer;
	//Current target to move towards
	private Node _target = null;
	private Queue<Node> _targetsqueue = new Queue<Node>();
	public Queue<Node> TargetsQueue { get { return _targetsqueue; } }
	private Node currentNode;

	public RandomWayPointAgent(NodeGraph pNodeGraph, float _pscale = 1f) : base(pNodeGraph, _pscale)
	{
		SetOrigin(width / 2, height / 2);

		HighLevelDungeonNodeGraph nodegraph = pNodeGraph as HighLevelDungeonNodeGraph;

		//Console.WriteLine($"Node Graph Dimension {nodegraph.}, {pNodeGraph.height}");

		//position ourselves on a random node
		if (pNodeGraph.nodes.Count > 0)
		{
			JumpToNode(pNodeGraph.nodes[Utils.Random(0, pNodeGraph.nodes.Count)]);
		}

		//listen to nodeclicksxxxxxxxxxx
		pNodeGraph.OnNodeLeftClicked += onNodeClickHandler;
		pNodeGraph.OnNodeRightClicked += moveAgentonRightClick;
	}

	protected virtual void moveAgentonRightClick(Node pNode)
    {
		JumpToNode(pNode);
	}

	public void JumpToNode(Node pNode)
    {
		currentNode = pNode;
		SetXY(pNode.location.X, pNode.location.Y);
	}

	// The Update Function is dedicated to running the queue of the set of movement generated in onNodeClickHandler
	//  all the set of movement is precalculated in onNodeClickHandler which then the queue will be run in the Update() function.

	bool isMoving;
	public bool IsMoving { get { return isMoving; } }

	protected virtual void onNodeClickHandler(Node pNode)
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

		bool Found = false;
		Node traverse = currentNode;

		// This Stack is for keeping track the path traveled.
		Stack<Node> stack = new Stack<Node>(); 
		Stack<Node> travelPath = new Stack<Node>(); I(travelPath);

		// This Dictionaries is for keeping track if its in the stack or if its visited.
		Dictionary<string, bool> visited = new Dictionary<string, bool>();

		// initialize
		stack.Push(traverse);
		visited[traverse.id] = true;



		while (!Found)
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
				Found = true;
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
					Found = true;
					isMoving = true;
					break;
				}
			}
		}
	}




	protected override void Update()
	{
		// FOR EVERY FRAME


		// Check if currently there is no queue
		if (_targetsqueue.Count == 0 && _target == null)
			return;

		// If there is a queue in the _targetsqueue, then peek and set that as the target.
		if (_target == null) currentNode = _target = _targetsqueue.Peek();

		// the Agent will start moving to _target while checking the queue and the target.

		// Once done moving,
		if (moveTowardsNode(_target))
		{
			//Console.WriteLine("Moving");
			// set the current node to the current node. For this case the variable is still useless
			// also dequeue the current Node
			_targetsqueue.Dequeue();

			_labelDrawer.drawQueueLabels();

			if (_targetsqueue.Count > 0)
            {
				// if there are more nodes queueing, then dequeue them as the next target
				//_labelDrawer.clearConnectionLabel(_target, _targetsqueue.Peek());
				_labelDrawer.markNode(_target);
				currentNode = _target = _targetsqueue.Peek();
            }
            else
            {
				// else, set it to null to stop the agent.
				_target = null;
				isMoving = false;
				_labelDrawer.clearQueueLabels();
			}
		}
	}

	private Stack<Node> _stack;
	const bool debug = false;
	void I(Stack<Node> a)
	{
		_stack = a;
	}
	string _P()
	{
		string t = "";
		for (int j = 0; j < _stack.Count; j++)
			t += "| ";
		return t;
	}
	void P(string s)
	{
		if (debug) Console.WriteLine(_P() + s);
	}
	void P(string s, IEnumerable<Node> l)
	{
		P(s, l, " ");
	}
	void P(string s, IEnumerable<Node> l, string sep)
	{
		if (debug) Console.WriteLine($"{_P()}{s}\n{_P()}          [ {string.Join(sep, l)} ]");
	}

	public void SetLabelDrawer(NodeLabelDrawer n)
    {
		_labelDrawer = n;
	}
}
