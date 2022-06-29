using System;
using System.Collections.Generic;

class PathFinder_Astar : PathFinder_Dijkstra
{
    public PathFinder_Astar(NodeGraph graph, bool v) : base(graph, v)
    {
    }

    protected override void initializeFunction(Node start, Node dest)
    {
        heuristics = new Dictionary<Node, double>();
        heuristics[start] = distanceToNode(start, destination);

        new StepPQ(this, start, prev: null, null, heuristics[start]);

        smallestCost = new Dictionary<Node, double>();
        smallestCost[start] = 0;

        prevNodes[start] = null;
    }


    Dictionary<Node, double> heuristics;
    Dictionary<Node, double> smallestCost;

    protected override void traverse(Node curr, List<Node> path, double dist) 
    {
        Console.WriteLine($"\n\n\nTraversing {curr} | d: {dist}");
        Console.WriteLine("Priority List: ");
        priorityList.ForEach(e => Console.Write($" [{e.currentNode} : {e.distance:F1}]\n"));

        if (dist == 0)
        {
            return;
        }

        if (prevNodes.ContainsKey(destination))
        {
            callqueue.Clear();
            functionCollection = callqueue;
            return;
        }

        // if current node has neighbor
        if (!curr.isolated)

            // for each neighbor
            foreach(Node child in curr.active_connections)
            {
                


                double currCostValue = dist - heuristics[curr] + distanceToNode(curr, child);



                if (!smallestCost.ContainsKey(child) || currCostValue < smallestCost[child])
                {
                    if(child == _startNode)
                    {
                        Console.WriteLine("??");
                    }

                    prevNodes[child] = curr;

                    smallestCost[child] = currCostValue;

                    heuristics[child] = distanceToNode(child, destination);

                    Console.WriteLine($" g: {currCostValue}");
                    Console.WriteLine($" h: {heuristics[child]}");

                    new StepPQ(this, child, curr, path, currCostValue + heuristics[child]);
                }
            }

                //// if neighbor node hasnt been visited yet before
                //if (!prevNodes.ContainsKey(child))
                //{
                //    prevNodes[child] = curr;


                //    if (!prevNodes.ContainsKey(destination))
                //    {



                //        heuristics[child] = distanceToNode(child, destination);

                //        new StepPQ(this, child, curr, path, dist + distanceToNode(curr, child) + heuristics[child] - heuristics[curr]);
                //    }
                //    else

                //        return;
                //}

        Console.WriteLine("Priority List: "); 
        priorityList.ForEach( e => Console.Write($" [{e.currentNode} : {e.distance:F1}]\n"));

        iterateNext();
    }
}