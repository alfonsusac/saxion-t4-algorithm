using GXPEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

/**
 * Helper class that draws nodelabels for a nodegraph.
 */
class NodeLabelDrawer : Canvas
{
	private Font _labelFont;
	private bool _showLabels = true;
	private bool _smallMode = true;

	private NodeGraph _graph = null;
	private NodeGraphAgent _agent = null;
	private RandomWayPointAgent _rwagent = null;

	public bool disableDrawing = false;

	public NodeLabelDrawer(NodeGraph pNodeGraph, NodeGraphAgent pNodeGraphAgent) : base(pNodeGraph.width, pNodeGraph.height)
	{
		Console.WriteLine("\n-----------------------------------------------------------------------------");
		Console.WriteLine("NodeLabelDrawer created.");
		Console.WriteLine("* L key to toggle node label display.");
		Console.WriteLine("-----------------------------------------------------------------------------");

		_labelFont = new Font(SystemFonts.DefaultFont.FontFamily, Math.Min(_smallMode ? pNodeGraph.nodeSize : pNodeGraph.nodeSize * 2, 12));
		_graph = pNodeGraph;
		_agent = pNodeGraphAgent;
		_rwagent = _agent as RandomWayPointAgent;
		drawLabels();
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	///							Update loop
	///							

	//this has to be virtual otherwise the subclass won't pick it up
	protected virtual void Update()
    {
        //toggle label display when L is pressed
        if (Input.GetKeyDown(Key.L))
        {
            _showLabels = !_showLabels;
            graphics.Clear(Color.Transparent);
            if (_showLabels && !disableDrawing) drawLabels();
        }
	}

	/////////////////////////////////////////////////////////////////////////////////////////
	/// PathAgent visualization helper methods
	internal void drawPaths(List<Node> l, Node m = null)
    {
		graphics.Clear(Color.Transparent);
		if (m != null) l.Add(m);
		if (l == null || l.Count == 0 ) return;
		Node prevN = null;
		foreach(Node n in l)
        {
			if (prevN == null) prevN = n;
			else
            {
				drawConnections(prevN, n, 10);
            }
			prevN = n;
        }
		if (m != null) l.Remove(m);
    }

	/////////////////////////////////////////////////////////////////////////////////////////
	/// NodeGraph visualization helper methods

	internal void drawQueueLabels()
    {
		if (disableDrawing) return;

		graphics.Clear(Color.Transparent);

		if (_showLabels) drawLabels();

		if (_rwagent.IsMoving)
		{
			if (_rwagent.TargetsQueue.Count > 0)
			{
				Queue<Node> q = _rwagent.TargetsQueue;
				for (int i = 0; i < q.Count; i++)
				{
					if (_showLabels) drawNode(q.ToArray()[i], Brushes.Black);
					if (i < q.Count - 1)
					{
						drawConnections(q.ToArray()[i], q.ToArray()[i + 1]);
					}
				}
			}
		}
		drawMarkedNode();
	}
	internal void drawConnections(Node n1, Node n2, int depth = 0, Pen q = null)
    {
		if(q == null)
        {
			if (depth == 0) depth = nodeSize * 2 -2;
			q = new Pen(Color.FromArgb(100, Color.White), depth);
			q.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);
		}
		graphics.DrawLine(q, n1.location, n2.location);
    }

    internal void clearQueueLabels()
    {
		graphics.Clear(Color.Transparent);
		marked.Clear();
		resetCounts();
		if (_showLabels) drawLabels();
	}

	Dictionary<Node, int> visitedCount = new Dictionary<Node, int>();
	internal void resetCounts()
    {
		visitedCount.Clear();
	}

    internal void countVisits(Node pNode)
    {
		if(!visitedCount.ContainsKey(pNode)) visitedCount[pNode] = 0;
		visitedCount[pNode]++;
		SizeF size = graphics.MeasureString("" + visitedCount[pNode], _labelFont);
		graphics.FillRectangle(Brushes.White, pNode.location.X, pNode.location.Y - size.Height / 2 - 20, size.Width, size.Height);
		graphics.DrawString("" + visitedCount[pNode], _labelFont, Brushes.Blue, pNode.location.X, pNode.location.Y - size.Height / 2 - 20);
	}

    private int nodeSize = 5;
	public void setNodeSize(int i)
    {
		nodeSize = i + 2;
    }

	List<Node> marked = new List<Node>();

	internal void markNode(Node n)
    {
		if (disableDrawing) return;

		marked.Add(n);

	}
	internal void drawMarkedNode()
    {
		if (disableDrawing) return;

		if (marked.Count > 0)
			foreach (Node m in marked)
			{
				graphics.FillEllipse(
					Brushes.White,
					m.location.X - nodeSize,
					m.location.Y - nodeSize,
					2 * nodeSize,
					2 * nodeSize
				);
			}
	}


	protected virtual void drawLabels()
	{
		if (!disableDrawing)
			foreach (Node node in _graph.nodes) if (_showLabels) drawNode(node, Brushes.White);
    }

	protected virtual void drawNode(Node pNode, Brush b)
	{
		SizeF size = graphics.MeasureString(pNode.id, _labelFont);
		graphics.DrawString(pNode.id, _labelFont, b, pNode.location.X - size.Width / 2 - 20, pNode.location.Y - size.Height / 2 - 20);
	}


}
