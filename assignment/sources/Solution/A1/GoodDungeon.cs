using GXPEngine;
using GXPEngine.OpenGL;
using System.Drawing;
using System;
using System.Collections.Generic;

class GoodDungeon : SufficientDungeon
{

    //Random random = new Random(15512);
    //4
    Random random = new Random(4);

    public GoodDungeon(Size pSize) : base(pSize)
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

    static readonly bool VERTICAL = true;
    static readonly bool HORIZONTAL = false;

    EasyDraw canvas;
    // Wrapper Class for straight walls
    internal class wall
    {
        public Point p1 { get; set; }
        public Point p2 { get; set; }
        public bool direction { get; }
        public int size { get { return p1.X == p2.X ? Math.Abs(p1.X - p2.X) : Math.Abs(p1.Y - p2.Y); } }

        public wall(Point p1, Point p2, bool direction)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.direction = direction;
        }
    }


    // link the doors of a room. i.e Which doors belongs to a room
    private Dictionary<Room, Stack<Door>> doorsOfRoom = new Dictionary<Room, Stack<Door>>();

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
    //   - If linkedlist.count at given x,y is 2 then it is a wall
    //   - else if it is > 3 then it is a corner

    // Map of allowed door placement for fast access.
    private bool[,] DoorNotPlaceable;

    // Lists of dividing walls
    private Stack<wall> room_with_door = new Stack<wall>();

    //Stack for the BSP
    private Queue<Room> room_queue = new Queue<Room>();

    protected override void generate(int pMinimumRoomSize)
    {
        pMinimumRoomSize += 2;
        Console.WriteLine($"size {size.Width} | {size.Height} and MinimumRoomSize {pMinimumRoomSize} ");
        
        // Mapping of Rooms
        MapOfRoom = new LinkedList<Room>[size.Width, size.Height];


        // BSP

        // BSP: initializing variable
        DoorNotPlaceable = new bool[size.Width+2, size.Height+2];
        for(int i = 0; i < size.Width; i++)
        {
            DoorNotPlaceable[i, 0] = true;
            DoorNotPlaceable[i, size.Height-1] = true;
        }
        for(int i = 0; i<= size.Height; i++)
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
            if(room.area.Width >= pMinimumRoomSize * 2 || room.area.Height >= pMinimumRoomSize * 2)
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
                else if(splitDir == HORIZONTAL)
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
                // Save the wall into the list for later
                room_with_door.Push(new wall(corner1,corner2, splitDir) );
                // Save the wall into the dict for later
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
                doorsOfRoom.Add(room, new Stack<Door>());
                addRoomToDungeon(room);
            }
        }

        deleteSmallestAndLargestRoom();

        addDoors();

        paintRoomsbyDoors(debug:false);

        Console.WriteLine($"{rooms.Count} rooms generated");
    }
    void addDoors()
    {
        // Adding doors
        while (room_with_door.Count > 0)
        {
            int x = 0, y = 0;
            // add doors starting from the most recently added wall
            wall w = room_with_door.Pop();

            x = random.Next(w.p1.X, w.p2.X);
            y = random.Next(w.p1.Y, w.p2.Y);

            int x2 = x, y2 = y;
            while (DoorNotPlaceable[x2, y2] && x != -1 && y != -1)
            {
                //Console.WriteLine($"{x2},{y2},{DoorNotPlaceable[x2, y2]}");

                if (w.direction == HORIZONTAL)
                {
                    x2++;
                    if (x2 == w.p2.X) x2 = w.p1.X;
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
                Console.WriteLine("Cannot find suitable position to place door");
            }
            else
            {
                addDoorsToDungeon(new Door(new Point(x2, y2)));
            }
        }

        // Making sure all room has wall
        bool AllHasWalls = true;
        foreach(Room r in rooms)
        {
            if(doorsOfRoom[r].Count == 0)
            {
                AllHasWalls = false;
                room_with_door.Push(new wall(new Point(r.area.Left, r.area.Top), new Point(r.area.Right-1, r.area.Top), HORIZONTAL));
                room_with_door.Push(new wall(new Point(r.area.Left, r.area.Bottom-1), new Point(r.area.Right-1, r.area.Bottom-1), HORIZONTAL));
                room_with_door.Push(new wall(new Point(r.area.Left, r.area.Top), new Point(r.area.Left, r.area.Bottom-1), VERTICAL));
                room_with_door.Push(new wall(new Point(r.area.Right-1, r.area.Top), new Point(r.area.Right-1, r.area.Bottom-1), VERTICAL));
            }
        }
        if (!AllHasWalls) addDoors();
    }


    void paintRoomsbyDoors(bool debug = false)
    {
        // - Fill background
        if(!debug) graphics.FillRectangle(Brushes.Black, 0, 0, size.Width - 0.5f, size.Height - 0.5f);

        // - Draw the rooms
        foreach (Room r in rooms)
        {
            Pen wallcolor = new Pen(Color.FromArgb(255, Color.Black));
            Brush floorcolor = Brushes.White;

            Color[] floorcolors = {
                debug ? Color.Transparent : Color.FromArgb(255, 102, 99),
                debug ? Color.Transparent : Color.FromArgb(254, 177, 68),
                debug ? Color.Transparent : Color.FromArgb(253, 253, 151),
                debug ? Color.Transparent : Color.FromArgb(158, 224, 158)
            };

            drawRoom(r, wallcolor, new SolidBrush(
                doorsOfRoom[r].Count < 3 ? floorcolors[doorsOfRoom[r].Count] : floorcolors[3]));
        }

        //  - Draw the door
        foreach(Door door in doors)
            drawDoor(door, Pens.LightGray);
    }

    ///////////////////////////////////////////////////
    // FEATURE: delete smallest and largest room
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
            foreach(Room r in temprooms)
            {
                if(r.area.Width * r.area.Height < smallestArea)
                    smallestArea = r.area.Width * r.area.Height;
                if (r.area.Width * r.area.Height > largestArea)
                    largestArea = r.area.Width * r.area.Height;
            }

            // - For every room that is the smallest or the largest, remove the room.
            foreach(Room r in temprooms)
            {
                if(r.area.Width * r.area.Height == smallestArea
                    || r.area.Width * r.area.Height == largestArea)
                {
                    removeRoom(r);
                }
            }
        }
    }

    ///////////////////////////////////////////////////
    // HELPER METHOD: remove room from dungeon
    public void removeRoom(Room r, bool _removeMissingDoor = false)
    {
        Console.WriteLine($"Room with {doorsOfRoom[r].Count} doors are removed");

        // - labeling walls as something that cant have doors
        for (int i = r.area.Left; i < r.area.Right; i++)
        {
            // horizontal walls
            DoorNotPlaceable[i, r.area.Top] = true;
            DoorNotPlaceable[i, r.area.Bottom-1] = true;
            //doors.Add(new Door(new Point(i, r.area.Bottom-1)));
        }
        for (int i = r.area.Top; i < r.area.Bottom; i++)
        {
            // vertical walls
            DoorNotPlaceable[r.area.Right-1, i] = true;
            DoorNotPlaceable[r.area.Left, i] = true;
        }

        // - Clear the doors associated to the room before removing the room
        doorsOfRoom[r].Clear();

        // - Removing the room
        rooms.Remove(r);
    }

    ///////////////////////////////////////////////////
    // HELPER METHOD: add doors and finalize to dungeon
    void addDoorsToDungeon(Door d)
    {
        // - add doors to the master list
        doors.Add(d);

        // - associate door to nearby room
        foreach(Room r in MapOfRoom[d.location.X, d.location.Y])
            doorsOfRoom[r].Push(d);

        // - mark the door position as not-door-placeable
        DoorNotPlaceable[d.location.X, d.location.Y] = true;

    }

    ///////////////////////////////////////////////////
    // HELPER METHOD: add room and finalize to dungeon
    protected void addRoomToDungeon(Room r)
    {
        // - add room to the master list
        rooms.Add(r);

        // - mark the region of the map with the room
        for(int i = r.area.X; i < r.area.X + r.area.Width; i++)
        {
            for(int j = r.area.Y; j < r.area.Y + r.area.Height; j++)
            {
                if (MapOfRoom[i, j] == null) MapOfRoom[i, j] = new LinkedList<Room>();
                MapOfRoom[i, j].AddLast(r);
            }
        }

        // - reset the sorted labael
        isRoomsSorted = false;
    }
}


