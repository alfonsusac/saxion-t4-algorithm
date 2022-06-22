using System;
using System.Collections.Generic;

class PathFinder_Astar : PathFinder_Dijkstra
{
    public PathFinder_Astar(NodeGraph graph, bool v) : base(graph, v)
    {
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
                    prevNodes[child] = curr;

                    if (!prevNodes.ContainsKey(destination))

                        new StepPQ(this, child, curr, path, dist + distanceToNode(curr, child) + distanceToNode(child, destination));

                    else

                        return;
                }

        Console.WriteLine("Priority List: "); 
        priorityList.ForEach( e => Console.Write($" [{e.currentNode} : {e.distance:F1}]\n"));

        iterateNext();

    }
}