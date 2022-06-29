using System;
using System.Collections.Generic;

class PathFinder_Dijkstra : PathFinder_BreadthFirst
{
    public PathFinder_Dijkstra(NodeGraph graph, bool v) : base(graph, v)
    {

    }

    // Data Structure necessary for Dijkstra
    protected List<StepPQ> priorityList;
    protected Dictionary<Node, (double dist, StepPQ func)> inList;

    // necessary to reset: for encapsulation
    protected virtual void CallfromPriorityList() { 

        priorityList[0].Run();
        priorityList.RemoveAt(0);
    }

    protected override void initialize(Node start, Node dest)
    {
        base.initialize(start, dest);

        priorityList = new List<StepPQ>();
        inList = new Dictionary<Node, (double dist, StepPQ func)>();

        functionForCallingFromList = CallfromPriorityList;
        functionCollection = priorityList;
    }
    protected override void startTraverse(Node start)
    {
        new StepPQ(this, start, null);
    }

    protected double distanceToNode(Node n1, Node n2)
    {
        return System.Math.Sqrt(
            (n2.location.X - n1.location.X) * 
            (n2.location.X - n1.location.X) 
            + 
            (n2.location.Y - n1.location.Y) * 
            (n2.location.Y - n1.location.Y));
    }

    protected override void traverse(Node curr, List<Node> path, double dist = 0)
    {
        Console.WriteLine($"\n\n\nTraversing {curr}");
        Console.WriteLine("Priority List: ");
        priorityList.ForEach(e => Console.Write($" [{e.currentNode} : {e.distance:F1}]\n"));

        // if current node has neighbor
        if (!curr.isolated)

            // for each neighbor
            foreach(Node child in curr.active_connections)

                // if neighbor node hasnt been visited yet before
                if (!prevNodes.ContainsKey(child))
                {

                    if (destination == null || (!prevNodes.ContainsKey(destination)))

                        // node WILL be added to the Priority List.
                        new StepPQ(this, child, curr, path, dist + distanceToNode(curr, child));

                }

        Console.WriteLine("Priority List: "); 
        priorityList.ForEach( e => Console.Write($" [{e.currentNode} : {e.distance:F1}]\n"));

        iterateNext();

    }
    internal class StepPQ : Step
    {
        bool addToPathDict = false;

        public StepPQ(PathFinder_Dijkstra r, Node n, Node prev, List<Node> l = null, double i = 0)
            : base(r, n, l, i)
        {
            if(addToPathDict)
                (pf as PathFinder_Dijkstra).prevNodes[currentNode] = prev;

            Console.WriteLine($"Adding StepPQ {currentNode} from {prev} with distance {distance}");

        }

        public override void Add()
        {
            ref var priorityList = ref (pf as PathFinder_Dijkstra).priorityList;
            ref var inList = ref (pf as PathFinder_Dijkstra).inList;


            // CHECK IF NODE IS ALREADY IN QUEUE
            // find the node USING DICT so its faster

            // if node is already in the list
            if (inList.ContainsKey(currentNode))

                // if the to-be-updated distance is smaller than the known distance
                if (distance < inList[currentNode].dist)
                {

                    Console.WriteLine($"This node is shorter. So replace.");
                    // then remove it
                    priorityList.Remove(inList[currentNode].func);

                }
                else
                {

                    Console.WriteLine($"Node node added. Shortest Path exists. {inList[currentNode].dist}");

                    // BREAK IF EXISTING NODE WITH SHORTER DISTANCE EXISTS
                    return;

                }
            // NODE WILL BE ADDED
            // if priority list is not empty
            if (priorityList.Count > 0)
            {
                // for every node in priority list
                int count = priorityList.Count;

                for (int i = 0; i <= count; i++)

                    // if index is at the end of the list, then add it
                    if (priorityList.Count == i)

                        priorityList.Add(this);

                    else
                    // if distance at index is greater or equal to distance to be added then add it
                    if (distance < priorityList[i].distance)
                    {
                        priorityList.Insert(i, this);


                        break; // and stop iterating
                    }
            }
            // if priority list is empty
            else

                // add it directly.
                priorityList.Add(this);

            // MARK CURRENT NODE AS VISITED
            addToPathDict = true;
            inList[currentNode] = (distance, this);
        }
    }
}