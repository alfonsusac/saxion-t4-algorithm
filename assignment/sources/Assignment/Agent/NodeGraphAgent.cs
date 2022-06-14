using GXPEngine;
using GXPEngine.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

/**
 * NodeGraphAgent provides a starting point for your own agents that would like to navigate the nodegraph.
 * It provides convenience methods such as moveTowardsNode & jumpToNode.
 * 
 * Create a subclass of this class, override Update and call these methods as required for your specific assignment.
 * See SampleNodeGraphAgent for an example.
 */
abstract class NodeGraphAgent : AnimationSprite
{
	protected const int REGULAR_SPEED = 1;
	protected const int FAST_TRAVEL_SPEED = 10;
	protected const int SPEED_UP_KEY = Key.LEFT_CTRL;

	float dungeonScale;

	public NodeGraphAgent(NodeGraph pNodeGraph, float _pscale = 1f) : base("assets/orc.png", 4, 2, 7)
	{
		Debug.Assert(pNodeGraph != null, "Please pass in a node graph.");

		//SetScaleXY(pNodeGraph.nodeSize * 3f / 36f);
		//scale = pNodeGraph.nodeSize * 3f / 36f;

		dungeonScale = pNodeGraph.nodeSize * 3f;

        //Console.WriteLine($"AGENT SPRITE DIMENSION {width} {height}");
		//Console.WriteLine($"AGENT SPRITE DIMENSION {width} {height}");

        SetScaleXY(dungeonScale/36f, dungeonScale / 36f);

        //EasyDraw e = new EasyDraw(width, height);
        //e.SetOrigin(e.width / 2, e.height / 2);
        //AddChild(e);
        //e.graphics.DrawRectangle(new Pen(Brushes.White, 5), 0, 0, e.width - 1, e.height - 1);


        //Console.WriteLine($"TRUE AGENT WIDTH {TransformPoint(width, 0).x - TransformPoint(0,0).x} {TransformPoint(0, height).y - TransformPoint(0, 0).y}");

		//EasyDraw h = new EasyDraw(width, height);
		//h.SetOrigin(h.width / 2, h.height / 2);
		//AddChild(h);
		//h.graphics.DrawRectangle(new Pen(Brushes.White), 0, 0, h.width - 1, h.height - 1);

		// This does not do anything
		//SetOrigin(0, 0);

		texture.Bind();
		GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
		GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
		texture.Unbind();

		Console.WriteLine(this.GetType().Name + " created.");
	}

	//override in subclass to implement any functionality
	protected abstract void Update();

	/////////////////////////////////////////////////////////////////////////////////////////
	///	Movement helper methods

	/**
	 * Moves towards the given node with either REGULAR_SPEED or FAST_TRAVEL_SPEED 
	 * based on whether the RIGHT_CTRL key is pressed.
	 */
	protected virtual bool moveTowardsNode(Node pTarget)
	{
		float speed = Input.GetKey(SPEED_UP_KEY) ? FAST_TRAVEL_SPEED : REGULAR_SPEED;
		//increase our current frame based on time passed and current speed
		SetFrame((int)(speed * (Time.time / 100)) % frameCount);

		//standard vector math as you had during the Physics course
		Vec2 targetPosition = new Vec2(pTarget.location.X, pTarget.location.Y);
		Vec2 currentPosition = new Vec2(__x, __y);
		Vec2 delta = targetPosition.Sub(currentPosition);

		if (delta.Length() < speed)
		{
			jumpToNode(pTarget);
			return true;
		}
		else
		{
			Vec2 velocity = delta.Normalize().Scale(speed);
			SetXY(__x + velocity.x, __y + velocity.y);
			//x += velocity.x;
			//y += velocity.y;

			scaleX = (velocity.x >= 0) ? Math.Abs(scaleX) : -Math.Abs(scaleX);

			return false;
		}
	}

	/**
	 * Jumps towards the given node immediately
	 */
	protected virtual void jumpToNode(Node pNode)
	{
		SetXY(pNode.location.X, pNode.location.Y);
	}

	float __x;
	float __y;
	

	public new void SetXY(float _x, float _y)
    {
		float xOffset = (Mathf.Abs(scaleX) - 1) * width * 0.5f;
		x = _x + (scaleX < 0 ? -xOffset : xOffset);
		y = _y + (scaleY - 1) * height * 0.5f;
		__x = _x;
		__y = _y;
    }

	protected NodeLabelDrawer _labelDrawer;

	public void SetLabelDrawer(NodeLabelDrawer n)
	{
		_labelDrawer = n;
	}


	/// Debug stuff
	/// 
	static private Stack<Node> _stack;
	static public bool debug = false;
	static protected void I(Stack<Node> a)
	{
		_stack = a;
	}
	static protected string _P()
	{
		string t = "";
		for (int j = 0; j < _stack.Count; j++)
			t += "| ";
		return t;
	}
	static protected void P(string s)
	{
		if (debug) Console.WriteLine(_P() + s);
	}
	static protected void P(string s, IEnumerable<Node> l)
	{
		P(s, l, " ");
	}
	static protected void P(string s, IEnumerable<Node> l, string sep)
	{
		if (debug) Console.WriteLine($"{_P()}{s}\n{_P()}          [ {string.Join(sep, l)} ]");
	}
}

