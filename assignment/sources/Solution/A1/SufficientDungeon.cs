using GXPEngine;
using GXPEngine.OpenGL;
using System.Drawing;
using System;
using System.Collections.Generic;

class SufficientDungeon : Dungeon
{
    public SufficientDungeon(Size pSize) : base(pSize)
    {
    }

    static readonly bool VERTICAL = true;
    static readonly bool HORIZONTAL = false;

    class wall
    {
        public Point p1 { get; set; }
        public Point p2 { get; set; }

        public wall(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

    }


    Random random = new Random();

    protected override void generate(int pMinimumRoomSize)
    {
        Console.WriteLine($"size {size.Width} | {size.Height} and MinimumRoomSize {pMinimumRoomSize} ");
        
        //Stack for the BSP
        Queue<Room> room_queue = new Queue<Room>();

        // Queue the initial room
        Room initial_room = newRoom(0, 0, size.Width, size.Height);
        room_queue.Enqueue(initial_room);

        // Lists of corners
        Dictionary<Point,bool> corner_exist = new Dictionary<Point,bool>();
        List<Point> corner_list = new List<Point>();
        
        // a stack Consisting of the tuple for the wall. Which is described with corner1 and corner2, 
        Stack<wall> bspWalls = new Stack<wall>();

        //direction
        while (room_queue.Count > 0)
        {
            // get the room from queue
            Room room = room_queue.Dequeue();
            Room room_1 = null;
            Room room_2 = null;
            int halfpoint1 = 0, halfpoint2= 0;
            int x1 = 0, y1 = 0, w1 = 0, h1 = 0, x2 = 0, y2 = 0, w2 = 0, h2 = 0;
            Point corner1 = new Point(), corner2 = new Point();

            Console.WriteLine($"Splitting Room {room.area.Width},{room.area.Height}");
            if(room.area.Width >= pMinimumRoomSize * 2 || room.area.Height >= pMinimumRoomSize * 2)
            {
                bool splitDir = room.area.Width > room.area.Height ? VERTICAL : HORIZONTAL;
                if (splitDir == VERTICAL)
                {
                    int splitAxis = room.area.Width;
                    int keepAxis = room.area.Height;

                    halfpoint1 = random.Next(pMinimumRoomSize, splitAxis - pMinimumRoomSize);
                    halfpoint2 = splitAxis - halfpoint1;

                    x1 = room.area.X;
                    y1 = room.area.Y;
                    w1 = halfpoint1 + 1;
                    h1 = keepAxis;

                    x2 = room.area.X + halfpoint1;
                    y2 = room.area.Y;
                    w2 = halfpoint2;
                    h2 = keepAxis;
                    
                    corner1 = new Point(x2, y1);
                    corner2 = new Point(x2, y1 + h2 - 1);
                }
                else if(splitDir == HORIZONTAL)
                {
                    int keepAxis = room.area.Width;
                    int splitAxis = room.area.Height;

                    halfpoint1 = random.Next(pMinimumRoomSize, splitAxis - pMinimumRoomSize);
                    halfpoint2 = splitAxis - halfpoint1;

                    x1 = room.area.X;
                    y1 = room.area.Y;
                    w1 = keepAxis;
                    h1 = halfpoint1 + 1;

                    x2 = room.area.X;
                    y2 = room.area.Y + halfpoint1;
                    w2 = keepAxis;
                    h2 = halfpoint2;

                    corner1 = new Point(x1, y2);
                    corner2 = new Point(x1 + w1 - 1, y2);

                }
                bspWalls.Push(new wall(corner1,corner2) );
                //corner_exist[corner1] = true;

                try
                {
                    corner_exist.Add(corner1, true);
                    corner_list.Add(corner1);
                }
                catch (Exception e) { }
                try
                {
                    corner_exist.Add(corner2, true);
                    corner_list.Add(corner2);
                }
                catch (Exception e) { }

                room_1 = newRoom(x1, y1, w1, h1);
                room_2 = newRoom(x2, y2, w2, h2);
                
                room_queue.Enqueue(room_1);
                room_queue.Enqueue(room_2);

            }
            else
            {
                Console.WriteLine($"Room added to <rooms>!");
                rooms.Add(room);
            }
        }

        // Adding doors
        while (bspWalls.Count > 0)
        {
            int x = 0, y = 0;
            wall w = bspWalls.Pop();
            do
            {
                x = random.Next(w.p1.X, w.p2.X);
                y = random.Next(w.p1.Y, w.p2.Y);
            } while (corner_exist.ContainsKey(new Point(x, y)));

            doors.Add(new Door(new Point(x, y)));
        }
        Console.WriteLine($"{rooms.Count} rooms generated");
    }


    ///////////////////////////////////////////////////
    // HELPER METHOD: new room
    public static Room newRoom(int x, int y, int width, int height)
    {
        return new Room(new Rectangle(x, y, width, height));
    }
}


