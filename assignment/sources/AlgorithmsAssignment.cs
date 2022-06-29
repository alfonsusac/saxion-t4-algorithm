using System.Drawing;
using GXPEngine;
using GXPEngine.OpenGL;

/**
 * This is the main 'game' for the Algorithms Assignment that accompanies the Algorithms course.
 * 
 * Read carefully through the assignment that you are currently working on
 * and then through the code looking for all pointers & TODO's that you have to implement.
 * 
 * The course is 6 weeks long and this is the only assignment/code that you will get,
 * split into 3 major parts (see below). This means that you have three 2 week sprints to
 * work on your assignments.
 */
class AlgorithmsAssignment : Game
{
	//Required for assignment 1
	Dungeon _dungeon = null;

	//Required for assignment 2
	NodeGraph _graph = null;
	TiledView _tiledView = null;
	NodeGraphAgent _agent = null;

	//Required for assignment 3
	PathFinder _pathFinder = null;

	//common settings
	private const int SCALE = 10;       //TODO: experiment with changing this
	private const int MIN_ROOM_SIZE = 7;    //TODO: use this setting in your dungeon generator
	private const int SEED = 23;

	public AlgorithmsAssignment() : base(1280, 768, false, true, -1, -1, false)
	{
		System.Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n--------------------------------------");
		
		// !!!! Important Toggles: !!!!
		Dungeon.autoDrawAfterGenerate = false;
		NodeGraph.doNotDraw = true;
		NodeGraphAgent.debug = false;
		NodeLabelDrawer.disableDrawing = false; // disable for LowLevel
		NodeLabelDrawer.disableLabelDrawing = false; 
		NodeLabelDrawer.setNodeSize(SCALE / 3 + 1);
		NodeLabelDrawer.setTileSize(SCALE);

		/////////////////////////////////////////////////////////////////////////////////////////
		///	?? BASE SETUP - FEEL FREE TO SKIP

		//set our default background color and title
		GL.ClearColor(0, 0, 0, 1);
		GL.glfwSetWindowTitle("Algorithms Game");

		
		// ??
		Grid grid = new Grid(width, height, SCALE);

		/////////////////////////////////////////////////////////////////////////////////////////
		///	ASSIGNMENT 1 : DUNGEON - READ CAREFULLY
		///
		Size size = new Size(width / SCALE, height / SCALE);


		////////////////////////////////////////
		//Assignment 1.1 Sufficient (Mandatory)
		//---------------------------------------------------
		//_dungeon = new SampleDungeon(size);
		//_dungeon = new SufficientDungeon(size);
		//_dungeon = new GoodDungeon(size);
		_dungeon = new ExcellentDungeon(size, SEED);

        if (_dungeon != null)
		{
			_dungeon.scale = SCALE; //assign the SCALE we talked about above, so that it no longer looks like a tinietiny stamp:
			_dungeon.Generate(MIN_ROOM_SIZE); //Tell the dungeon to generate rooms and doors with the given MIN_ROOM_SIZE
		}

		/////////////////////////////////////////////////////////////////////////////////////////
		/// ASSIGNMENT 2 : GRAPHS, AGENTS & TILES
		///							
		/// SKIP THIS BLOCK UNTIL YOU'VE FINISHED ASSIGNMENT 1 AND ASKED FOR TEACHER FEEDBACK !

		/////////////////////////////////////////////////////////////
		//Assignment 2.1 Sufficient (Mandatory) High Level NodeGraph
		// ---------------------------------------------------			
		//_graph = new SampleDungeonNodeGraph(_dungeon);
		//_graph = new HighLevelDungeonNodeGraph(_dungeon);
		_graph = new NodeGraph_LowLevelDungeon(_dungeon);

		NodeLabelDrawer _nodeLabelDrawer = new NodeLabelDrawer(_graph);
		NodeLabelDrawer _pathLabelDrawer = new NodeLabelDrawer(_graph);
		NodeLabelDrawer _tileLabelDrawer = new NodeLabelDrawer(_graph);

		if (_graph != null) _graph.Generate();

		/////////////////////////////////////////////////////////////
		//Assignment 2.1 Sufficient (Mandatory) OnGraphWayPointAgent
		// ---------------------------------------------------			
		//_agent = new SampleNodeGraphAgent(_graph);
		//_agent = new OnGraphWayPointAgent(_graph);

		////////////////////////////////////////////////////////////
		//Assignment 2.2 Good (Optional) TiledView
		// ---------------------------------------------------		
		//_tiledView = new SampleTiledView(_dungeon, TileType.GROUND);
		_tiledView = new TiledDungeonView(_dungeon, TileType.GROUND);
		if (_tiledView != null) _tiledView.Generate();

		//--

		if(_graph is NodeGraph_LowLevelDungeon)
        {
			(_graph as NodeGraph_LowLevelDungeon)._view = _tiledView as TiledDungeonView;
			(_graph as NodeGraph_LowLevelDungeon)?.SetLabelDrawer(_tileLabelDrawer);
			(_graph as NodeGraph_LowLevelDungeon).generateTiled();
		}
        //--


        ////////////////////////////////////////////////////////////
        //Assignment 2.2 Good (Optional) RandomWayPointAgent
        // ---------------------------------------------------		
        //_agent = new RandomWayPointAgent(_graph);
        //_agent2 = new RandomWayPointAgent(_graph, -1.0f);
        //_agent3 = new RandomWayPointAgent(_graph, -2f);

        //////////////////////////////////////////////////////////////
        //Assignment 2.3 Excellent (Optional) LowLevelDungeonNodeGraph
        // ---------------------------------------------------		

        /////////////////////////////////////////////////////////////////////////////////////////
        /// ASSIGNMENT 3 : PathFinding and PathFindingAgents
        ///							
        /// SKIP THIS BLOCK UNTIL YOU'VE FINISHED ASSIGNMENT 2 AND ASKED FOR TEACHER FEEDBACK !

        //_pathFinder = new PathFinder_Recursive(_graph, true);			// Sufficient
        //_pathFinder = new PathFinder_BreadthFirst(_graph, true);		// Sufficient
        //_pathFinder = new PathFinder_Dijkstra(_graph, true);			// Good
        _pathFinder = new PathFinder_Astar(_graph, true);				// Excellent
			


        _agent = new Agent_PathFinding(_graph, _pathFinder as SamplePathFinder);

        _pathFinder.SetLabelDrawer(_pathLabelDrawer);
		_agent.SetLabelDrawer(_nodeLabelDrawer);

		//------------------------------------------------------------------------------------------
		/// REQUIRED BLOCK OF CODE TO ADD ALL OBJECTS YOU CREATED TO THE SCREEN IN THE CORRECT ORDER
		/// LOOK BUT DON'T TOUCH :)

		if (grid != null)		AddChild(grid);	
		if (_dungeon != null)	AddChild(_dungeon);
        if (_tiledView != null) AddChild(_tiledView);
		if (_graph != null)		AddChild(_graph);
        if (_graph != null && _nodeLabelDrawer != null)		AddChild(_tileLabelDrawer); //node label display on top of that
        if (_pathFinder != null)AddChild(_pathFinder);             //pathfinder on top of that
        if (_graph != null && _nodeLabelDrawer != null)		AddChild(_nodeLabelDrawer); //node label display on top of that
        if (_graph != null && _nodeLabelDrawer != null)		AddChild(_pathLabelDrawer); //node label display on top of that
        if (_agent != null)		AddChild(_agent);                       //and last but not least the agent itself

		/////////////////////////////////////////////////
		//The end!
		////
	}
}


