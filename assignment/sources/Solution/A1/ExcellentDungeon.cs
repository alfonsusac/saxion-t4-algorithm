using GXPEngine;
using GXPEngine.OpenGL;
using System.Drawing;
using System;
using System.Collections.Generic;

class ExcellentDungeon : Dungeon
{

    //Random random = new Random(15512);
    //4
    Random random = new Random(13);
    bool debug = false;

    public ExcellentDungeon(Size pSize) : base(pSize)
    {
        canvas = new EasyDraw(pSize.Width, pSize.Height);

        canvas.texture.Bind();
        GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
        GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
        canvas.texture.Unbind();

        AddChild(canvas);

        canvas.Fill(Color.Red);
        canvas.Stroke(Color.Red);
    }
    public static readonly bool VERTICAL = true;
    public static readonly bool HORIZONTAL = false;
    public static readonly int WIDTH = 0;
    public static readonly int HEIGHT = 1;

    EasyDraw canvas;




    // link the doors of a room. i.e Which doors belongs to a room
    private Dictionary<Room, Stack<Corridor>[]> doorsOfRoom = new Dictionary<Room, Stack<Corridor>[]>();
    static readonly int ALL = 0;
    static readonly int TOP = 1;
    static readonly int WEST = 2;
    static readonly int EAST = 3;
    static readonly int BOTTOM = 4;

    // link the allowed inflation of a room
    private Dictionary<Room, int[]> allowedInflatability = new Dictionary<Room, int[]>();
    //  - default inflatability
    private int MAX_INFLATABILITY;

    // |=====-----------D-----------------=====|
    // Left MAX leftdiv d.loc rightdiv   MAX  Right

    // Maps which room exists according to the x and y position of the map
    private LinkedList<Room>[,] MapOfRoom;

    // DATA STRUCTURE SUMMARY:
    // - A dungeon has list of ROOMS and list of DOORS
    // - A room is associated to doors and can be quickly accessed
    //    via Dictionary<Room, Stack<Door>> doorsOfRoom
    //   - A stack is used instead of list to list down every door associated to a room
    //      in order to save memory space as it is only needed to store and
    //      sort and search is not needed.
    // - An array of linked list is used to map down the corresponding room
    //    in a specific coordinate in the dungeon.
    //   - This is for fast access to know what room exists at x,y position.
    //   - If linkedlist.count at given x,y is 2 then it is a Wall
    //   - else if it is > 3 then it is a corner

    // Map of allowed door placement for fast access.
    private bool[,] DoorNotPlaceable;

    protected TileType[,] tileTypeMap;
    public TileType[,] TileTypeMap { get; }

    // Lists of dividing Walls
    private Stack<Wall> BPSWalls = new Stack<Wall>();

    //Stack for the BSP
    private Queue<Room> room_queue = new Queue<Room>();

    protected override void generate(int pMinimumRoomSize)
    {
        pMinimumRoomSize += 2;
        Console.WriteLine($"size {size.Width} | {size.Height} and MinimumRoomSize {pMinimumRoomSize} ");

        MAX_INFLATABILITY = pMinimumRoomSize / 4;
        Console.WriteLine($"Minimum Inflatabiliy {MAX_INFLATABILITY}");

        // Mapping of Rooms
        MapOfRoom = new LinkedList<Room>[size.Width, size.Height];

        tileTypeMap = new TileType[size.Width, size.Height];

        // BSP

        // BSP: initializing variable
        DoorNotPlaceable = new bool[size.Width + 2, size.Height + 2];

        // mark the boudnary so that doors cannot be placed on it
        for (int i = 0; i < size.Width; i++)
        {
            DoorNotPlaceable[i, 0] = true;
            DoorNotPlaceable[i, size.Height - 1] = true;
        }
        for (int i = 0; i <= size.Height; i++)
        {
            DoorNotPlaceable[0, i] = true;
            DoorNotPlaceable[size.Width - 1, i] = true;
        }

        // Queue the initial room
        Room initial_room = newRoom(0, 0, size.Width, size.Height);
        room_queue.Enqueue(initial_room);

        // BSP: The Iterating process

        // BEGIN BINARY SPACE PARTITIONING
        while (room_queue.Count > 0)
        {
            // get the room from queue
            Room room = room_queue.Dequeue();

            // child rooms
            Room room_1, room_2;

            //initializing variables
            int halfpoint1, halfpoint2;
            int x1 = 0, y1 = 0, w1 = 0, h1 = 0, x2 = 0, y2 = 0, w2 = 0, h2 = 0;
            Point corner1 = new Point(), corner2 = new Point();

            //Console.WriteLine($"Splitting Room {room.area.Width},{room.area.Height}");
            // Check if current room can be divided
            if (room.area.Width >= pMinimumRoomSize * 2 || room.area.Height >= pMinimumRoomSize * 2)
            {
                // Decide the direction of the division
                bool splitDir = room.area.Width > room.area.Height ? VERTICAL : HORIZONTAL;

                // the upper left most corner
                x1 = room.area.X;
                y1 = room.area.Y;

                // split top to bottom
                if (splitDir == VERTICAL)
                {
                    int splitAxis = room.area.Width;
                    int keepAxis = room.area.Height;
                    // randomize the dividing points in between the allowed range
                    halfpoint1 = random.Next(pMinimumRoomSize, splitAxis - pMinimumRoomSize);
                    halfpoint2 = splitAxis - halfpoint1;

                    w1 = halfpoint1 + 1;
                    h1 = keepAxis;

                    x2 = room.area.X + halfpoint1;
                    y2 = room.area.Y;
                    w2 = halfpoint2;
                    h2 = keepAxis;

                    corner1 = new Point(x2, y1);
                    corner2 = new Point(x2, y1 + h2 - 1);
                }

                // split left to right
                else if (splitDir == HORIZONTAL)
                {
                    int keepAxis = room.area.Width;
                    int splitAxis = room.area.Height;
                    // randomize the dividing points in between the allowed range
                    halfpoint1 = random.Next(pMinimumRoomSize, splitAxis - pMinimumRoomSize);
                    halfpoint2 = splitAxis - halfpoint1;

                    w1 = keepAxis;
                    h1 = halfpoint1 + 1;

                    x2 = room.area.X;
                    y2 = room.area.Y + halfpoint1;
                    w2 = keepAxis;
                    h2 = halfpoint2;

                    corner1 = new Point(x1, y2);
                    corner2 = new Point(x1 + w1 - 1, y2);

                }
                // Save the Wall into the list for later
                BPSWalls.Push(new Wall(corner1, corner2, splitDir));
                // Save the Wall into the dict for later
                DoorNotPlaceable[corner1.X, corner1.Y] = true;
                DoorNotPlaceable[corner2.X, corner2.Y] = true;

                room_1 = newRoom(x1, y1, w1, h1);
                room_2 = newRoom(x2, y2, w2, h2);

                // queue the chlid room into the queue to traverse to child nodes.
                room_queue.Enqueue(room_1);
                room_queue.Enqueue(room_2);
            }
            else
            {
                // add room to the roomlist if it cannot be separated.
                addRoomToDungeon(room);
            }
        }
        // - Draw Background;
        drawBackground();

        deleteSmallestAndLargestRoom();

        addDoors();

        inflateRooms();

        // - Draw the rooms
        drawRooms();

        //  - Draw the door
        Corridor.drawCorridors(this);
        //drawDoors();

        // - Paints the Background
        paintNullSpace();


        Console.WriteLine($"{rooms.Count} rooms generated");
    }
    
    void inflateRooms()
    {
        foreach (Room room in rooms)
        {
            // - inflate the rooms
            room.area.Inflate(
                -allowedInflatability[room][WIDTH],
                -allowedInflatability[room][HEIGHT]);

            // - joins the top corridors into room
            //Console.WriteLine($"Inflating Room {room.area.Location}");
            foreach (Corridor d in doorsOfRoom[room][TOP])
            {
                d.SetExtensionBottomRight(allowedInflatability[room][HEIGHT], MAX_INFLATABILITY);
            }
            // - joins the west corridors into room
            foreach (Corridor d in doorsOfRoom[room][WEST])
            {
                d.SetExtensionBottomRight(allowedInflatability[room][WIDTH], MAX_INFLATABILITY);
            }
            // - joins the east corridors into room
            foreach (Corridor d in doorsOfRoom[room][EAST])
            {
                d.SetExtensionTopLeft(allowedInflatability[room][WIDTH], MAX_INFLATABILITY);
            }
            // - joins the bottom corridors into room
            foreach (Corridor d in doorsOfRoom[room][BOTTOM])
            {
                d.SetExtensionTopLeft(allowedInflatability[room][HEIGHT], MAX_INFLATABILITY);
            }
        }
    }

    Dictionary<Wall, bool> unsuitableWalls = new Dictionary<Wall, bool>();
    void addDoors()
    {
        // While there are rooms without doors
        while (BPSWalls.Count > 0)
        {
            int x = 0, y = 0;
            // - Add doors starting from the most recently added Wall
            Wall w = BPSWalls.Pop();

            // - Generate random location within the Walls
            x = random.Next(w.p1.X, w.p2.X);
            y = random.Next(w.p1.Y, w.p2.Y);

            // - Check if door location can be placed
            int x2 = x, y2 = y;
            while (DoorNotPlaceable[x2, y2] && x != -1 && y != -1)
            {
                if (w.direction == HORIZONTAL)
                {
                    // - if not, then move to next available location
                    x2++;

                    // - if the next available location is the end of Wall, 
                    // then move to start of Wall
                    if (x2 == w.p2.X) x2 = w.p1.X;
                    
                    // - if the next available location is the initial random location
                    // then mark as invalid
                    if (x2 == x) x = -1;
                }
                if (w.direction == VERTICAL)
                {
                    y2++;
                    if (y2 == w.p2.Y) y2 = w.p1.Y;
                    if (y2 == y) y = -1;
                }
            }
            if (x == -1 || y == -1)
            {
                // - If invalid then dont add doors
                unsuitableWalls[w] = true;
            }
            else
            {
                addDoorToDungeon(new Corridor(new Point(x2, y2),!w.direction), w.direction);
            }
        }
        Console.WriteLine();
        // Making sure all room has Wall
        foreach (Room r in rooms)
        {
            Wall[] Walls = new Wall[4]
            {
                //Top Wall
                new Wall(new Point(r.area.Left, r.area.Top), new Point(r.area.Right - 1, r.area.Top), HORIZONTAL),
                //Bottom Wall
                new Wall(new Point(r.area.Left, r.area.Bottom - 1), new Point(r.area.Right - 1, r.area.Bottom - 1), HORIZONTAL),
                //Left Wall
                new Wall(new Point(r.area.Left, r.area.Top), new Point(r.area.Left, r.area.Bottom - 1), VERTICAL),
                //Right Wall
                new Wall(new Point(r.area.Right - 1, r.area.Top), new Point(r.area.Right - 1, r.area.Bottom - 1), VERTICAL)
            };

            // the bigger the rarer.
            int ChanceOfGeneratingNewDoors = 4;

            if (doorsOfRoom[r][WEST].Count == 0)
                if (random.Next(0, ChanceOfGeneratingNewDoors) == 0)
                    addDoorToDungeon(new Corridor(new Point(random.Next(Walls[2].p1.X, Walls[2].p2.X), random.Next(Walls[2].p1.Y + 1, Walls[2].p2.Y) - 1), !Walls[2].direction), Walls[2].direction);

            if (doorsOfRoom[r][EAST].Count == 0)
                if (random.Next(0, ChanceOfGeneratingNewDoors) == 0)
                    addDoorToDungeon(new Corridor(new Point(random.Next(Walls[3].p1.X, Walls[3].p2.X), random.Next(Walls[3].p1.Y + 1, Walls[3].p2.Y) - 1), !Walls[3].direction), Walls[3].direction);

            if (doorsOfRoom[r][TOP].Count == 0)
                if (random.Next(0, ChanceOfGeneratingNewDoors) == 0)
                    addDoorToDungeon(new Corridor(new Point(random.Next(Walls[0].p1.X + 1, Walls[0].p2.X - 1), random.Next(Walls[0].p1.Y, Walls[0].p2.Y)), !Walls[0].direction), Walls[0].direction);

            if (doorsOfRoom[r][BOTTOM].Count == 0)
                if (random.Next(0, ChanceOfGeneratingNewDoors) == 0)
                    addDoorToDungeon(new Corridor(new Point(random.Next(Walls[1].p1.X + 1, Walls[1].p2.X - 1), random.Next(Walls[1].p1.Y, Walls[1].p2.Y)), !Walls[1].direction), Walls[1].direction);

        }
    }

    bool isRoomsSorted = false;
    public void deleteSmallestAndLargestRoom(bool multiple = true)
    {
        // This is if we just want to remove one of the largest/smallest
        if (!multiple)
        {
            // - Sort the rooms list if not sorted
            if (!isRoomsSorted)
                rooms.Sort((Room a, Room b) => a.area.Width * a.area.Height - b.area.Width * b.area.Height);

            // - Remove the smallest room
            removeRoom(rooms[0]);

            // - Remove the largest room
            removeRoom(rooms[rooms.Count - 1]);
        }

        // This is if we want to remove ALL largest & smallest
        else
        {
            // - Find the smallest area and the largest area
            int smallestArea = int.MaxValue;
            int largestArea = 0;
            List<Room> temprooms = new List<Room>(rooms);
            foreach (Room r in temprooms)
            {
                if (r.area.Width * r.area.Height < smallestArea)
                    smallestArea = r.area.Width * r.area.Height;
                if (r.area.Width * r.area.Height > largestArea)
                    largestArea = r.area.Width * r.area.Height;
            }

            // - For every room that is the smallest or the largest, remove the room.
            foreach (Room r in temprooms)
            {
                if (r.area.Width * r.area.Height == smallestArea
                    || r.area.Width * r.area.Height == largestArea)
                {
                    removeRoom(r);
                }
            }
        }
    }

    public void removeRoom(Room r, bool _removeMissingDoor = false)
    {
        //Console.WriteLine($"Room with {doorsOfRoom[r][ALL].Count} doors are removed");

        // - labeling Walls as something that cant have doors
        for (int i = r.area.Left; i < r.area.Right; i++)
        {
            // horizontal Walls
            DoorNotPlaceable[i, r.area.Top] = true;
            DoorNotPlaceable[i, r.area.Bottom - 1] = true;
            //doors.Add(new Door(new Point(i, r.area.Bottom-1)));
        }
        for (int i = r.area.Top; i < r.area.Bottom; i++)
        {
            // vertical Walls
            DoorNotPlaceable[r.area.Right - 1, i] = true;
            DoorNotPlaceable[r.area.Left, i] = true;
        }

        // - Clear the map from the room
        for (int i = r.area.X; i < r.area.X + r.area.Width; i++)
            for (int j = r.area.Y; j < r.area.Y + r.area.Height; j++)
                MapOfRoom[i, j].Remove(r);

        // - Clear the doors associated to the room before removing the room
        doorsOfRoom[r][0].Clear();
        doorsOfRoom[r][1].Clear();
        doorsOfRoom[r][2].Clear();
        doorsOfRoom[r][3].Clear();
        doorsOfRoom[r][4].Clear();
        doorsOfRoom.Remove(r);

        //BRdoorsOfRoom[r].Clear();
        //BRdoorsOfRoom.Remove(r);

        //TLdoorsOfRoom[r].Clear();
        //TLdoorsOfRoom.Remove(r);

        // - Clear the inflatibility associated to the room
        allowedInflatability.Remove(r);

        // - Removing the room
        rooms.Remove(r);
    }

    void addDoorToDungeon(Corridor d, bool WallDirection)
    {
        if (DoorNotPlaceable[d.X, d.Y])
        {
            //Console.WriteLine($"{DoorNotPlaceable[d.X, d.Y]} {d.X}, {d.Y}");
            //drawDot(d.X, d.Y, Pens.Red);
            //addDebugDoors(d.X, d.Y);
            //Console.WriteLine($"TRIED TO PLACE DOOR AT FORBIDDEN PLACE -> {d.X}, {d.Y}");
            return;
        }
        // - add doors to the master list
        //doors.Add(d);
        Corridor.corridors.Add(d);

        // - Assigning rooms to the door

        LinkedList<Room> roomlist = MapOfRoom[d.location.X, d.location.Y];
        // get first room and second room from the linkedlist
        Room room1 = roomlist.First.Value;
        Room room2 = roomlist.Last.Value;

        // d.roomA is the left or the top room of the door
        // d.roomB is the right or bottom room of the door
        if (WallDirection == HORIZONTAL)
        {
            if(room1.area.Y < room2.area.Y)
            {
                d.roomA = room1;
                d.roomB = room2;
            }
            else
            {
                d.roomA = room2;
                d.roomB = room1;
            }
        } 
        else if (WallDirection == VERTICAL) 
        {
            if (room1.area.X < room2.area.X)
            {
                d.roomA = room1;
                d.roomB = room2;
            }
            else
            {
                d.roomA = room2;
                d.roomB = room1;
            }
        }
        //Console.WriteLine($"ADTD {d.roomA.area} |d| {d.roomB.area}\n");

        foreach (Room r in MapOfRoom[d.location.X, d.location.Y])
        {
            // - associate door to neighboring room
                                                doorsOfRoom[r][ALL].Push(d);
            if (d.X == r.area.Left)             doorsOfRoom[r][WEST].Push(d);
            else if (d.X == r.area.Right - 1)   doorsOfRoom[r][EAST].Push(d);
            else if (d.Y == r.area.Top)         doorsOfRoom[r][TOP].Push(d);
            else if (d.Y == r.area.Bottom - 1)  doorsOfRoom[r][BOTTOM].Push(d);
            else Console.WriteLine("WTFF?????");

            // if Wall direction is horizontal then the door is vertical (top room to bottom room)
            if (WallDirection == HORIZONTAL)
            {
                int leftdiv = d.location.X - r.area.Left - 1;
                int rightdiv = r.area.Right - d.location.X - 2;
                if (leftdiv < allowedInflatability[r][WIDTH])
                {
                    //Console.WriteLine($"Changing Allowed Inflatability of {r.area.Location} LD{leftdiv} {allowedInflatability[r][WIDTH]}");
                    allowedInflatability[r][WIDTH] = leftdiv;
                }
                if (rightdiv < allowedInflatability[r][WIDTH])
                {
                    //Console.WriteLine($"Changing Allowed Inflatability of {r.area.Location} RD{rightdiv} {allowedInflatability[r][WIDTH]}");
                    allowedInflatability[r][WIDTH] = rightdiv;
                }
                //Console.WriteLine($"Box ({r.area.X},{r.area.Y})" +
                //    $" ({r.area.Bottom},{r.area.Right}) with door at ({d.location.X},{d.location.Y})" +
                //    $" has horizontal division {leftdiv}|{rightdiv}");
            }
            if(WallDirection == VERTICAL)
            {
                int topdiv = d.location.Y - r.area.Top - 1;
                int bottomdiv = r.area.Bottom - d.location.Y - 2;
                if (topdiv < allowedInflatability[r][HEIGHT])
                {
                    //Console.WriteLine($"Changing Allowed Inflatability of {r.area.Location} TD{topdiv} {allowedInflatability[r][HEIGHT]}");
                    allowedInflatability[r][HEIGHT] = topdiv;
                }
                if (bottomdiv < allowedInflatability[r][HEIGHT])
                {
                    //Console.WriteLine($"Changing Allowed Inflatability of {r.area.Location} BD{bottomdiv} {allowedInflatability[r][HEIGHT]}");
                    allowedInflatability[r][HEIGHT] = bottomdiv;
                }
                //Console.WriteLine($"Box ({r.area.X},{r.area.Y})" +
                //    $" ({r.area.Bottom},{r.area.Right}) with door at ({d.location.X},{d.location.Y})" +
                //    $" has vertical division {topdiv}|{bottomdiv}");
            }
            //Console.WriteLine($"Inflatability of room {r.area.X}, {r.area.Y} is {allowedInflatability[r][WIDTH]}, {allowedInflatability[r][HEIGHT]}");
        }

        // - mark the door position as not-door-placeable
        DoorNotPlaceable[d.location.X, d.location.Y] = true;

    }

    protected void addRoomToDungeon(Room r)
    {
        // - add room to the master list
        rooms.Add(r);
        
        // - Add room to the dictionary to keep track of associated doors
        doorsOfRoom.Add(r, new Stack<Corridor>[5]
        {
            new Stack<Corridor>(), // for general
            new Stack<Corridor>(), // top doors
            new Stack<Corridor>(), // west doors
            new Stack<Corridor>(), // east doors
            new Stack<Corridor>()  // bottom doors
        });

        // - Add room to the dictionary to keep track of how much can it inflate
        allowedInflatability[r] = new int[2] { MAX_INFLATABILITY, MAX_INFLATABILITY };

        // - Increase inflatiability to MAX by default
        allowedInflatability[r][WIDTH] = MAX_INFLATABILITY;
        allowedInflatability[r][HEIGHT] = MAX_INFLATABILITY;

        // - mark the region of the map with the room
        for (int i = r.area.X; i < r.area.X + r.area.Width; i++)
            for (int j = r.area.Y; j < r.area.Y + r.area.Height; j++)
            {
                if (MapOfRoom[i, j] == null) MapOfRoom[i, j] = new LinkedList<Room>();
                MapOfRoom[i, j].AddLast(r);
            }

        // - reset the sorted labael
        isRoomsSorted = false;
    }

    public static Room newRoom(int x, int y, int width, int height)
    {
        Room temp = new Room(new Rectangle(x, y, width, height));
        return temp;
    }
    private void addDebugDoors(int x, int y)
    {
        doors.Add(new Door(new Point(x, y)));
    }

    void drawRectangle(Rectangle RectArea, Pen pWallColor, Brush pFillColor)
    {
        if (pFillColor != null) graphics.FillRectangle(pFillColor, RectArea.Left, RectArea.Top, RectArea.Width - 0.5f, RectArea.Height - 0.5f);
        graphics.DrawRectangle(pWallColor, RectArea.Left, RectArea.Top, RectArea.Width - 0.5f, RectArea.Height - 0.5f);
    }
    void drawDot(int x, int y, Pen pColor)
    {
        graphics.DrawRectangle(pColor, x, y, 0.5f, 0.5f);
    }
    void drawBackground()
    {
        graphics.Clear(Color.FromArgb(200,Color.Black));
    }

    void drawDoors()
    {
        foreach (Door door in doors)
            drawDoor(door, Pens.LightGray);
        foreach(Corridor c in Corridor.corridors)
        {
            Console.WriteLine($"{c.Direction} ({c.X}, {c.Y})");
        }
    }

    void drawRooms()
    {
        foreach (Room r in rooms)
        {
            Pen Wallcolor = new Pen(Color.FromArgb(255, Color.Black));
            Brush floorcolor = Brushes.White;

            Color[] floorcolors = {
                debug ? Color.Transparent : Color.FromArgb(165, 73, 79), // red
                debug ? Color.Transparent : Color.FromArgb(173, 126, 72), // orange
                debug ? Color.Transparent : Color.FromArgb(167, 161, 90), // yellow
                debug ? Color.Transparent : Color.FromArgb(72, 134, 77) // green
            };

            drawRoom(r, Wallcolor, new SolidBrush(
                doorsOfRoom[r][ALL].Count < 3 ? floorcolors[doorsOfRoom[r][ALL].Count] : floorcolors[3]));


        }
    }
    void paintNullSpace()
    {
        for(int i = 0; i < MapOfRoom.GetLength(0); i++)
            for(int j = 0; j < MapOfRoom.GetLength(1); j++)
            {
                if(MapOfRoom[i, j].Count == 0)
                    drawDot(i, j, new Pen(Color.FromArgb(20,Color.White), 3));
            }
    }
    // Internal Class for straight Walls
    internal class Wall
    {
        public Point p1 { get; set; }
        public Point p2 { get; set; }
        public bool direction { get; }
        public int size { get { return p1.X == p2.X ? Math.Abs(p1.X - p2.X) : Math.Abs(p1.Y - p2.Y); } }

        public Wall(Point p1, Point p2, bool direction)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.direction = direction;
        }
        public override int GetHashCode()
        {
            return p1.GetHashCode() ^ p2.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Wall)) return false;
            Wall w = (Wall)obj;
            if (w.p1 == p1)
                return w.p2 == p2;
            return false;
        }
    }

    // Internal Class for straight corridors
    internal class Corridor : Door
    {
        public int X { get { return location.X; } }
        public int Y { get { return location.Y; } }
        public bool Direction { get { return horizontal; } }
        // Direction == VERTICAL (true) means rooms connecting top room to bottom room
        // Direction == HORIZONTAL (false) means rooms connecting left room to right room

        public readonly static Color defaultColor = Color.Gray;

        public static List<Corridor> corridors = new List<Corridor>();

        public Point entry1;
        public Point entry2;

        private int extensiontopleft = 0;
        private int extensionbottomright = 0;

        public int ExtensionTopLeft { get { return extensiontopleft; } }
        public int ExtensionBottomRight { get { return extensionbottomright; } }

        public static Pen wallcolor = Pens.Black;
        public static Pen floorcolor = Pens.Gray;

        public Corridor(Point pLocation, bool direction) : base(pLocation)
        {
            horizontal = direction;
            entry1 = new Point(pLocation.X, pLocation.Y);
            entry2 = new Point(pLocation.X, pLocation.Y);
        }

        public void SetExtensionTopLeft(int i, int maxInflate)
        {
            if(i > 0 && i <= maxInflate)
                extensiontopleft = i;
            if(Direction == VERTICAL)
            {
                entry1.Offset(0, -i);
            }
            if (Direction == HORIZONTAL)
            {
                entry1.Offset(-i, 0);
            }
            //Console.WriteLine($"({X}, {Y}, {Direction}) Exdtend topleft by {i}");
        }
        public void SetExtensionBottomRight(int i, int maxInflate)
        {
            if(i > 0 && i <= maxInflate)
                extensionbottomright = i;
            if (Direction == VERTICAL)
            {
                entry2.Offset(0, i);
            }
            if (Direction == HORIZONTAL)
            {
                entry2.Offset(i, 0);
            }
            //Console.WriteLine($"({X}, {Y}, {Direction}) Exdtend bottomright by {i}");
        }

        public Corridor(int x, int y,bool direction) : base(new Point(x, y))
        {
            corridors.Add(this);
            horizontal = direction;
        }
        public static void drawCorridors(ExcellentDungeon g)
        {
            foreach (Corridor corridor in corridors) corridor.drawCorridor(g);
        }

        public void drawCorridor(ExcellentDungeon g)
        {


            if (Direction == HORIZONTAL)
            {
                g.graphics.DrawRectangle(
                    pen: wallcolor,
                    x: X - extensiontopleft,
                    y: Y - 1,
                    width: extensiontopleft + extensionbottomright + 0.5f,
                    height: 3 - 0.5f);
                g.graphics.DrawRectangle(
                    pen: floorcolor,
                    x: X - extensiontopleft,
                    y: Y,
                    width: extensiontopleft + extensionbottomright + 0.5f,
                    height: 0.5f);
            }

            if (Direction == VERTICAL)
            {
                g.graphics.DrawRectangle(
                    pen: wallcolor,
                    x: X - 1,
                    y: Y - extensiontopleft,
                    width: 3 - 0.5f,
                    height: extensiontopleft + extensionbottomright + 0.5f);

                g.graphics.DrawRectangle(
                    pen: floorcolor,
                    x: X,
                    y: Y - extensiontopleft,
                    width: 0.5f,
                    height: extensiontopleft + extensionbottomright + 0.5f);
            }
        }
    }
}

