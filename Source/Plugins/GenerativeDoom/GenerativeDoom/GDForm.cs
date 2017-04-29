using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Config;
using System.Threading;

namespace GenerativeDoom
{
    public partial class GDForm : Form
    {
        public const int TYPE_PLAYER_START = 1;

        private IList<DrawnVertex> points;
        private List<Line2D> pathRoomDirectionFaces = new List<Line2D>(); //The outward faces of the previous room sector. Used to check direction.
      
        private List<List<Line2D>> pathRooms = new List<List<Line2D>>(); //A list of previous face lists which could still be potentially used to find a direction
        private Line2D directionFace; //The face that we'll use as our jumping off point of our next room. We use the perpendiular direction vector of this face.  

        bool[] dirIntersect; //Array of bools which will be resized as the number of elements in faces for each new room. 

        //TODO Replace with General.Map.Map.Sectors TEST: if the most recent sector added will be the last one in this list.
        private List<Sector> allSectors = new List<Sector>();

        private Random r = new Random();

        enum SUBCATEGORIES
        {
            COMMON_MONSTERS,
            UNCOMMON_MONSTERS,
            RARE_MONSTERS,
            EPIC_MONSTERS,
            LEGEND_MONSTERS
        };

        DrawnVertex v = new DrawnVertex();

        float height;
        float width;
        float faceWidthRatio; //Used to get ratio between the connection lines of two rooms. This is necesssary to connect them without rounding errors. 
        float connectionPos;
        int minHeight;
        int minWidth;
        int maxWidth;
        int maxHeight;
        int ceil;
        int prevCeil;
        int floor;
        int prevFloor;
        int illumination;

        public GDForm()
        {
            InitializeComponent();
            points = new List<DrawnVertex>();
        }

        private void newSector(IList<DrawnVertex> points, int lumi, int ceil, int floor)
        {
            Console.Write("New sector: ");
            List<DrawnVertex> pSector = new List<DrawnVertex>();

            for (int i = 0; i < points.Count; ++i)
            {
                Console.Write(" p" + i +": " + points[i].pos);
                v.pos = points[i].pos;
                v.stitch = true;
                v.stitchline = true;
                pSector.Add(v);
            }

            Console.Write(" p" + 0 + ": " + points[0].pos);
            Console.Write("\n");

            v.pos = points[0].pos;
           
            v.stitch = true;
            v.stitchline = true;

            pSector.Add(v);

            Tools.DrawLines(pSector);
            
            //Clear selection
            General.Map.Map.ClearAllSelected();

            // Update cached values
            General.Map.Map.Update();

            // Edit new sectors?
            List<Sector> newsectors = General.Map.Map.GetMarkedSectors(true);

            allSectors.AddRange(newsectors);//TODO delete this probably. It was used for doors but i don't think i'll need it. 

            foreach (Sector s in newsectors)
            {
                s.CeilHeight = ceil;
                s.FloorHeight = floor;
                s.Brightness = lumi;
            }

            // Update the used textures
            General.Map.Data.UpdateUsedTextures();

            // Map is changed
            General.Map.IsChanged = true;

            General.Interface.RedrawDisplay();

            //Ajoute, on enleve la marque sur les nouveaux secteurs
            General.Map.Map.ClearMarkedSectors(false); 
        }

        private bool checkIntersect(Line2D measureline)
        {
            bool inter = false;
            foreach (Linedef ld2 in General.Map.Map.Linedefs)
            {
                // Intersecting?
                // We only keep the unit length from the start of the line and
                // do the real splitting later, when all intersections are known
                float u;
                if (ld2.Line.GetIntersection(measureline, out u))
                {
                    if (!float.IsNaN(u) && (u > 0.0f) && (u < 1.0f))
                    {
                        inter = true;
                        break;
                    }
                }
            }

            Console.WriteLine("Check inter " + measureline + " is " + inter);

            return inter;
        }

        private bool checkIntersect(Line2D measureline, out float closestIntersect)
        {
            // Check if any other lines intersect this line
            List<float> intersections = new List<float>();
            foreach (Linedef ld2 in General.Map.Map.Linedefs)
            {
                // Intersecting?
                // We only keep the unit length from the start of the line and
                // do the real splitting later, when all intersections are known
                float u;
                if (ld2.Line.GetIntersection(measureline, out u))
                {
                    if (!float.IsNaN(u) && (u > 0.0f) && (u < 1.0f))
                        intersections.Add(u);
                }
            }

            if (intersections.Count() > 0)
            {
                // Sort the intersections
                intersections.Sort();

                closestIntersect = intersections.First<float>();

                return true;
            }

            closestIntersect = 0.0f;
            return false;
        }

        //Uses the planned dimensions of the new shape in order to more accurately detect collisions.
        private void CheckDirection(List<Line2D> shape)
        {
            if (pathRooms.Count > 0)
                pathRoomDirectionFaces = pathRooms.Last<List<Line2D>>(); //The faces of the room which we last created. We remove it from the list if no directions are available.

            bool oneDirOk = false;
            dirIntersect = new bool[pathRoomDirectionFaces.Count];
            for (int i = 0; i < pathRoomDirectionFaces.Count; ++i)
            {
                List<Line2D> potentialShape = new List<Line2D>();
                for (int j = 0; j < shape.Count; ++j)
                {
                    //Rotate and translate line
                    {
                        double angle = Math.Atan(pathRoomDirectionFaces[i].GetDelta().y / pathRoomDirectionFaces[i].GetDelta().x);

                        if (pathRoomDirectionFaces[i].GetDelta().GetSign().x < 0)
                            angle += Math.PI;

                        float rotX1 = (float)(shape[j].v1.x * Math.Cos(angle) - shape[j].v1.y * Math.Sin(angle));
                        float rotY1 = (float)(shape[j].v1.x * Math.Sin(angle) + shape[j].v1.y * Math.Cos(angle));
                        float rotX2 = (float)(shape[j].v2.x * Math.Cos(angle) - shape[j].v2.y * Math.Sin(angle));
                        float rotY2 = (float)(shape[j].v2.x * Math.Sin(angle) + shape[j].v2.y * Math.Cos(angle));
                        Vector2D translation = pathRoomDirectionFaces[i].GetCoordinatesAt(connectionPos);

                        potentialShape.Add(new Line2D(new Vector2D(rotX1, rotY1) + translation, new Vector2D(rotX2, rotY2) + translation));
                    }

                }
                for (int j = 0; j < potentialShape.Count; ++j)
                {
                    if (dirIntersect[i])
                        break;
                    foreach (Linedef ld in General.Map.Map.Linedefs)
                    {
                        float u;
                        if (ld.Line.GetIntersection(potentialShape[j], out u))
                        {
                            if (!float.IsNaN(u) && (u > 0.0f) && (u < 1.0f))
                            {
                                dirIntersect[i] = true;
                                break;
                            }
                        }
                    }
                }                
            }

            foreach (bool d in dirIntersect)
                if (!d)
                    oneDirOk = true;

            if (!oneDirOk)
            {
                Console.WriteLine("No direction available, cannot use this room.");
                //Step back into the previous room and try create a path from there.

                //Add some stuff so the player doesn't feel bad about going down the wrong path. We're nice guys.
                {
                    Vector2D roomMid = new Vector2D(0, 0);
                    int count = 0;
                    foreach (Line2D ld in pathRooms.Last<List<Line2D>>())
                    {
                        roomMid.x += ld.v1.x;
                        roomMid.y += ld.v1.y;
                        roomMid.x += ld.v2.x;
                        roomMid.y += ld.v2.y;
                        count += 2;
                    }
                    roomMid /= count;

                    addThing(new Vector2D(roomMid.x + ((float)r.Next(0, 64)), roomMid.y + ((float)r.Next(0, 64))), "weapons");
                    addThing(new Vector2D(roomMid.x + ((float)r.Next(0, 64)), roomMid.y + ((float)r.Next(0, 64))), "ammunition");
                    addThing(new Vector2D(roomMid.x + ((float)r.Next(0, 64)), roomMid.y + ((float)r.Next(0, 64))), "health");

                }

                pathRooms.RemoveAt(pathRooms.Count-1);
                CheckDirection(shape);
            }
            else
            {
                int index = r.Next() % dirIntersect.Length;
                while (dirIntersect[index])
                    index = r.Next() % dirIntersect.Length;
                directionFace = pathRoomDirectionFaces[index];
            }
        }

        private void showCategories()
        {
            lbCategories.Items.Clear();
            IList<ThingCategory> cats = General.Map.Data.ThingCategories;
            foreach (ThingCategory cat in cats)
            {
                if (!lbCategories.Items.Contains(cat.Name))
                    lbCategories.Items.Add(cat.Name);
            }
        }

        private void GenerateFirstRoom()
        {
            pathRoomDirectionFaces.Clear(); //Necessary if we delete the map and regenerate. 
            points.Clear();

            minWidth = 256;
            minHeight = 256;
            maxWidth = 768;
            maxHeight = 768;
            ceil = (int)(r.NextDouble() * 128 + 128);
            floor = 0;

            //Size of the next sector
            width = (float)(r.Next(minWidth, maxWidth));
            height = (float)(r.Next(minHeight, maxHeight));

            Vector2D startingPos = new Vector2D(0, 0);

            Vector2D[] cornerArr = new Vector2D[4];
            cornerArr[0] = startingPos;
            cornerArr[1] = startingPos + (new Vector2D(0, 1) * height);
            cornerArr[2] = startingPos + (new Vector2D(1, 0) * width) + (new Vector2D(0, 1) * height);
            cornerArr[3] = startingPos + (new Vector2D(1, 0) * width);


            Vector2D roomMid = new Vector2D(0, 0) ;
            foreach (Vector2D c in cornerArr)
            {
                roomMid += c;
                v.pos = c;
                points.Add(v);
            }
            roomMid /= cornerArr.Length;

            newSector(points, illumination, ceil, floor);

            for (int i = 0; i < cornerArr.Length - 1; ++i)
                pathRoomDirectionFaces.Add(new Line2D(cornerArr[i], cornerArr[i + 1]));
            pathRoomDirectionFaces.Add(new Line2D(cornerArr[cornerArr.Length - 1], cornerArr[0]));

            pathRooms.Add(pathRoomDirectionFaces);
            
            prevCeil = ceil;
            prevFloor = floor;

            //Place player in the center of the room
            {
                Thing t = addThing(roomMid);
                t.Type = TYPE_PLAYER_START;
                t.Rotate(0);
            }
        }

        #region Standard Shaped Rooms

        private void GenerateRectangularRoom()
        {
            points.Clear();

            minWidth = 256;
            minHeight = 256;
            maxWidth = 768;
            maxHeight = 768;

            //Size of the next sector
            width = (float)(r.Next(minWidth, maxWidth));
            height = (float)(r.Next(minHeight, maxHeight));

            connectionPos = (float)r.NextDouble();

            //The corners of the room before translation and rotation. 
            Vector2D[] cornerArr = new Vector2D[4];
            cornerArr[0] = new Vector2D(-width/2, 0);
            cornerArr[1] = new Vector2D(-width/2, height);
            cornerArr[2] = new Vector2D(width/2, height);
            cornerArr[3] = new Vector2D(width/2, 0);

            //Lines defining the shape of the room at (0,0)
            List<Line2D> shape = new List<Line2D>();
            //Note: we pusposefully do not close the shape because we don't want to check for a collision on the connecting line.
            for (int i = 0; i < cornerArr.Length - 1; ++i)
            {
                shape.Add(new Line2D(cornerArr[i], cornerArr[i + 1]));
            }

            CheckDirection(shape);

            faceWidthRatio = width / directionFace.GetLength();

            List<Vector2D> cornerList = new List<Vector2D>();

            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2) + (directionFace.GetPerpendicular().GetNormal() * height));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2) + (directionFace.GetPerpendicular().GetNormal() * height));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2));

            int dontAddToFaceList = 1;
            if (connectionPos + faceWidthRatio / 2 > 1.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(1.0f));
                dontAddToFaceList++;
            }
            if (connectionPos - faceWidthRatio / 2 < 0.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(0.0f));
                dontAddToFaceList++;
            }
            if (connectionPos - faceWidthRatio / 2 > 0.0f && connectionPos + faceWidthRatio / 2 < 1.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(connectionPos));
                dontAddToFaceList++;
            }

            for (int i = 0; i < cornerList.Count; ++i)
            {
                v.pos = cornerList[i];
                points.Add(v);
            }

            List<Line2D> currentRoomFacesList = new List<Line2D>();
            for (int i = 0; i < cornerList.Count - dontAddToFaceList; ++i)
            {
                currentRoomFacesList.Add(new Line2D(cornerList[i], cornerList[i + 1]));
            }
            pathRooms.Add(currentRoomFacesList);

           // floor = Math.Min(256, Math.Max(0, prevFloor + (r.NextDouble() > 0.5 ? 1 : -1) * 16));
           // ceil = Math.Min(256, Math.Max(0, prevCeil + (r.NextDouble() > 0.5 ? 1 : -1) * 16));

            if (ceil - floor < 100)
            {
                floor = prevFloor;
                ceil = floor + 180 + (r.NextDouble() > 0.5 ? 1 : -1) * 16;
            }

            newSector(points, illumination, ceil, floor);

            prevCeil = ceil;
            prevFloor = floor;
        }

        private void GeneratePentagonRoom()
        {
       
            points.Clear();

            minWidth = 512;
            minHeight = 512;
            maxWidth = 1024;
            maxHeight = 1024;

            //Size of the next sector
            width = (float)(r.Next(minWidth, maxWidth));
            height = (float)(r.Next(minHeight, maxHeight));

            connectionPos = (float)r.NextDouble();


            //The corners of the room before translation and rotation. 
            Vector2D[] cornerArr = new Vector2D[5];
            cornerArr[0] = new Vector2D(-width / 4, 0);
            cornerArr[1] = new Vector2D(-width / 2, height / 2);
            cornerArr[2] = new Vector2D(0, height);
            cornerArr[3] = new Vector2D(width / 2, height / 2);
            cornerArr[4] = new Vector2D(width / 4, 0);

            //Lines defining the shape of the room at (0,0)
            List<Line2D> shape = new List<Line2D>();
            //Note: we pusposefully do not close the shape because we don't want to check for a collision on the connecting line.
            for (int i = 0; i < cornerArr.Length - 1; ++i)
            {
                shape.Add(new Line2D(cornerArr[i], cornerArr[i + 1]));
            }

            CheckDirection(shape);

            this.faceWidthRatio = width / this.directionFace.GetLength();

            List<Vector2D> cornerList = new List<Vector2D>();

            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 2);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 2);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4));

            int dontAddToFaceList = 1;
            if (connectionPos + this.faceWidthRatio / 4 > 1.0f)
            {
                cornerList.Add(this.directionFace.GetCoordinatesAt(1.0f));
                dontAddToFaceList++;

            }
            if (connectionPos - this.faceWidthRatio / 4 < 0.0f)
            {
                cornerList.Add(this.directionFace.GetCoordinatesAt(0.0f));
                dontAddToFaceList++;
            }
            if (connectionPos - this.faceWidthRatio / 4 > 0.0f && connectionPos + this.faceWidthRatio / 2 < 1.0f)
            {
                cornerList.Add(this.directionFace.GetCoordinatesAt(connectionPos));
                dontAddToFaceList++;
            }

            for (int i = 0; i < cornerList.Count; ++i)
            {
                v.pos = cornerList[i];
                points.Add(v);
            }

            List<Line2D> currentRoomFacesList = new List<Line2D>();
            for (int i = 0; i < cornerList.Count - dontAddToFaceList; ++i)
            {
                currentRoomFacesList.Add(new Line2D(cornerList[i], cornerList[i + 1]));
            }
            pathRooms.Add(currentRoomFacesList);

            //floor = Math.Min(256, Math.Max(0, prevFloor + (r.NextDouble() > 0.5 ? 1 : -1) * 16));
            //ceil = Math.Min(256, Math.Max(0, prevCeil + (r.NextDouble() > 0.5 ? 1 : -1) * 16));

            if (ceil - floor < 100)
            {
                floor = prevFloor;
                ceil = floor + 180 + (r.NextDouble() > 0.5 ? 1 : -1) * 16;
            }

            newSector(points, illumination, ceil, floor);

            prevCeil = ceil;
            prevFloor = floor;
        }

        private void GenerateHexagonRoom()
        {
            points.Clear();

            minWidth = 512;
            minHeight = 512;
            maxWidth = 1024;
            maxHeight = 1024;

            //Size of the next sector
            width = (float)(r.Next(minWidth, maxWidth));
            height = (float)(r.Next(minHeight, maxHeight));

            connectionPos = (float)r.NextDouble();
           
            //The corners of the room before translation and rotation. 
            Vector2D[] cornerArr = new Vector2D[6];
            cornerArr[0] = new Vector2D(-width / 4, 0);
            cornerArr[1] = new Vector2D(-width / 2, height / 2);
            cornerArr[2] = new Vector2D(-width / 4, height);
            cornerArr[3] = new Vector2D(width / 4, height);
            cornerArr[4] = new Vector2D(width / 2, height/2);
            cornerArr[5] = new Vector2D(width / 4, 0);

            //Lines defining the shape of the room at (0,0)
            List<Line2D> shape = new List<Line2D>();
            //Note: we pusposefully do not close the shape because we don't want to check for a collision on the connecting line.
            for (int i = 0; i < cornerArr.Length - 1; ++i)
            {
                shape.Add(new Line2D(cornerArr[i], cornerArr[i + 1]));
            }

            CheckDirection(shape);

            faceWidthRatio = width / this.directionFace.GetLength(); ;

            List<Vector2D> cornerList = new List<Vector2D>();

            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 2);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 2);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4));

            int dontAddToFaceList = 1;

            //We add a vertex on the limit of the previous face to make sure there are at least 2 points connecting to the previous room. This fixes a rounding error. 
            if (connectionPos + faceWidthRatio / 4 > 1.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(1.0f));
                dontAddToFaceList++;
            }
            else if (connectionPos - faceWidthRatio / 4 < 0.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(0.0f));
                dontAddToFaceList++;
            }
            else
            {
                cornerList.Add(directionFace.GetCoordinatesAt(connectionPos));
                dontAddToFaceList++;
            }

            for (int i = 0; i < cornerList.Count; ++i)
            {
                v.pos = cornerList[i];
                points.Add(v);
            }

            //We add the list of generated faces to our faces vector which will use each one to check for an intersection. We don't need to add the last two faces since that's the face we used.
            List<Line2D> currentRoomFacesList = new List<Line2D>();
            for (int i = 0; i < cornerList.Count - dontAddToFaceList; ++i)
            {
                currentRoomFacesList.Add(new Line2D(cornerList[i], cornerList[i + 1]));
            }
            pathRooms.Add(currentRoomFacesList);

            //floor = Math.Min(256, Math.Max(0, prevFloor + (r.NextDouble() > 0.5 ? 1 : -1) * 16));
            //ceil = Math.Min(256, Math.Max(0, prevCeil + (r.NextDouble() > 0.5 ? 1 : -1) * 16));

            if (ceil - floor < 100)
            {
                floor = prevFloor;
                ceil = floor + 180 + (r.NextDouble() > 0.5 ? 1 : -1) * 16;
            }

            newSector(points, illumination, ceil, floor);

            prevCeil = ceil;
            prevFloor = floor;
        }
        private void GenerateFixedHexagonRoom()
        {
            points.Clear();

            minWidth = 512;
            minHeight = 512;
            maxWidth = 1024;
            maxHeight = 1024;

            //Size of the next sector
            width = 512;// (float)(r.Next(minWidth, maxWidth));
            height = 512;// (float)(r.Next(minHeight, maxHeight));

            connectionPos = 0.5f; //(float)r.NextDouble();

            //The corners of the room before translation and rotation. 
            Vector2D[] cornerArr = new Vector2D[6];
            cornerArr[0] = new Vector2D(-width / 4, 0);
            cornerArr[1] = new Vector2D(-width / 2, height / 2);
            cornerArr[2] = new Vector2D(-width / 4, height);
            cornerArr[3] = new Vector2D(width / 4, height);
            cornerArr[4] = new Vector2D(width / 2, height / 2);
            cornerArr[5] = new Vector2D(width / 4, 0);

            //Lines defining the shape of the room at (0,0)
            List<Line2D> shape = new List<Line2D>();
            //Note: we pusposefully do not close the shape because we don't want to check for a collision on the connecting line.
            for (int i = 0; i < cornerArr.Length - 1; ++i)
            {
                shape.Add(new Line2D(cornerArr[i], cornerArr[i + 1]));
            }

            CheckDirection(shape);

            faceWidthRatio = width / this.directionFace.GetLength(); ;

            List<Vector2D> cornerList = new List<Vector2D>();

            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 2);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 2);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4));

            int dontAddToFaceList = 1;

            //We add a vertex on the limit of the previous face to make sure there are at least 2 points connecting to the previous room. This fixes a rounding error. 
            if (connectionPos + faceWidthRatio / 4 > 1.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(1.0f));
                dontAddToFaceList++;
            }
            else if (connectionPos - faceWidthRatio / 4 < 0.0f)
            {
                cornerList.Add(directionFace.GetCoordinatesAt(0.0f));
                dontAddToFaceList++;
            }
            else
            {
                cornerList.Add(directionFace.GetCoordinatesAt(connectionPos));
                dontAddToFaceList++;
            }

            for (int i = 0; i < cornerList.Count; ++i)
            {
                v.pos = cornerList[i];
                points.Add(v);
            }

            //We add the list of generated faces to our faces vector which will use each one to check for an intersection. We don't need to add the last two faces since that's the face we used.
            List<Line2D> currentRoomFacesList = new List<Line2D>();
            for (int i = 0; i < cornerList.Count - dontAddToFaceList; ++i)
            {
                currentRoomFacesList.Add(new Line2D(cornerList[i], cornerList[i + 1]));
            }
            pathRooms.Add(currentRoomFacesList);

            //floor = Math.Min(256, Math.Max(0, prevFloor + (r.NextDouble() > 0.5 ? 1 : -1) * 16));
            //ceil = Math.Min(256, Math.Max(0, prevCeil + (r.NextDouble() > 0.5 ? 1 : -1) * 16));

            if (ceil - floor < 100)
            {
                floor = prevFloor;
                ceil = floor + 180 + (r.NextDouble() > 0.5 ? 1 : -1) * 16;
            }

            newSector(points, illumination, ceil, floor);

            prevCeil = ceil;
            prevFloor = floor;
        }
        private void GenerateOctagonRoom()
        {
            points.Clear();

            minWidth = 512;
            minHeight = 512;
            maxWidth = 1028;
            maxHeight = 1028;

            //Size of the next sector
            width = (float)(r.Next(minWidth, maxWidth));
            height = (float)(r.Next(minHeight, maxHeight));

            this.connectionPos = (float)r.NextDouble();

            //The corners of the room before translation and rotation. 
            Vector2D[] cornerArr = new Vector2D[8];
            cornerArr[0] = new Vector2D(-width / 4, 0);
            cornerArr[1] = new Vector2D(-width / 2, height / 4);
            cornerArr[2] = new Vector2D(-width / 2, height * 3 / 4);
            cornerArr[3] = new Vector2D(-width / 4, height);
            cornerArr[4] = new Vector2D(width / 4, height);
            cornerArr[5] = new Vector2D(width / 2, height * 3 / 4);
            cornerArr[6] = new Vector2D(width / 2, height / 4);
            cornerArr[7] = new Vector2D(width / 4, 0);

            //Lines defining the shape of the room at (0,0)
            List<Line2D> shape = new List<Line2D>();
            //Note: we pusposefully do not close the shape because we don't want to check for a collision on the connecting line.
            for (int i = 0; i < cornerArr.Length - 1; ++i)
            {
                shape.Add(new Line2D(cornerArr[i], cornerArr[i + 1]));
            }

            CheckDirection(shape);

            this.faceWidthRatio = width / this.directionFace.GetLength();

            List<Vector2D> cornerList = new List<Vector2D>();

            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4));
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 4);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height * 3 / 4);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos - faceWidthRatio / 4) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4) + directionFace.GetPerpendicular().GetNormal() * height);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height * 3 / 4);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 2) + directionFace.GetPerpendicular().GetNormal() * height / 4);
            cornerList.Add(directionFace.GetCoordinatesAt(connectionPos + faceWidthRatio / 4));


            int dontAddToFaceList = 1;
            if (this.connectionPos + this.faceWidthRatio / 4 > 1.0f)
            {
                cornerList.Add(this.directionFace.GetCoordinatesAt(1.0f));
                dontAddToFaceList++;
            }
            if (this.connectionPos - this.faceWidthRatio / 4 < 0.0f)
            {
                cornerList.Add(this.directionFace.GetCoordinatesAt(0.0f));
                dontAddToFaceList++;
            }
            if (this.connectionPos - this.faceWidthRatio / 4 > 0.0f && this.connectionPos + this.faceWidthRatio / 4 < 1.0f)
            {
                cornerList.Add(this.directionFace.GetCoordinatesAt(this.connectionPos));
                dontAddToFaceList++;
            }

            for (int i = 0; i < cornerList.Count; ++i)
            {
                v.pos = cornerList[i];
                points.Add(v);
            }

            List<Line2D> currentRoomFacesList = new List<Line2D>();
            for (int i = 0; i < cornerList.Count - dontAddToFaceList; ++i)
            {
                currentRoomFacesList.Add(new Line2D(cornerList[i], cornerList[i + 1]));
            }
            pathRooms.Add(currentRoomFacesList);

            //floor = Math.Min(256, Math.Max(0, prevFloor + (r.NextDouble() > 0.5 ? 1 : -1) * 16));
            //ceil = Math.Min(256, Math.Max(0, prevCeil + (r.NextDouble() > 0.5 ? 1 : -1) * 16));

            if (ceil - floor < 100)
            {
                floor = prevFloor;
                ceil = floor + 180 + (r.NextDouble() > 0.5 ? 1 : -1) * 16;
            }

            newSector(points, illumination, ceil, floor);

            prevCeil = ceil;
            prevFloor = floor;
        }

        #endregion

        private void MakeMagicPath(bool playerOneStart) //My method. 
        {
            illumination = 200;
            ceil = (int)(r.NextDouble() * 128 + 128);
            floor = (int)(r.NextDouble() * 128 + 128);

            int numSectors = 30; //Originally 100 

            for (int i = 0; i < numSectors; i++)
            {
                Console.WriteLine("---------------------- Sector " + i);

                double randomRoom = r.NextDouble();
                if (i == 0)
                    GenerateFirstRoom();
                else if (i == numSectors - 1)
                    GenerateOctagonRoom();
                else if (randomRoom < 0.3f)
                    GenerateRectangularRoom();
                else if (0.3f <= randomRoom && randomRoom <= 0.65f)
                    GeneratePentagonRoom();
                else if (0.55f <= randomRoom && randomRoom <= 0.8f)
                    GenerateHexagonRoom();
                else
                    GenerateOctagonRoom();

                generateThings(i);

                // Handle thread interruption
                try { Thread.Sleep(0); }
                catch (ThreadInterruptedException) { Console.WriteLine(">>>> thread generation interrupted at sector " + i); break; }
            }
        }

        private void MakeSidMeiersCivilizationVI()
        {
            illumination = 200;
            ceil = (int)(r.NextDouble() * 128 + 128);
            floor = (int)(r.NextDouble() * 128 + 128);

            int numSectors = 100;

            for (int i = 0; i < numSectors; i++)
            {
                Console.WriteLine("---------------------- Sector " + i);

                if (i == 0)
                    GenerateFirstRoom();
                else
                    GenerateFixedHexagonRoom();

                generateThings(i);

                // Handle thread interruption
                try { Thread.Sleep(0); }
                catch (ThreadInterruptedException) { Console.WriteLine(">>>> thread generation interrupted at sector " + i); break; }
            }
        }

        private void btnDoMagic_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Trying to do some magic !!");

            MakeMagicPath(true);

            correctMissingTex();

            Console.WriteLine("Did I ? Magic ?");
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            foreach (ThingTypeInfo ti in General.Map.Data.ThingTypes)
                Console.WriteLine(ti.Category.Name);
            showCategories();

        }

        //Horribly written method; 
        private void generateThings(int roomNumber)
        {
            float left;
            float right;
            float top;
            float bottom;

            Random r = new Random();

            left = Math.Min(Math.Min(points[0].pos.x, points[1].pos.x), Math.Min(points[2].pos.x, points[3].pos.x));
            right = Math.Max(Math.Max(points[0].pos.x, points[1].pos.x), Math.Max(points[2].pos.x, points[3].pos.x));
            bottom = Math.Min(Math.Min(points[0].pos.y, points[1].pos.y), Math.Min(points[2].pos.y, points[3].pos.y));
            top = Math.Max(Math.Max(points[0].pos.y, points[1].pos.y), Math.Max(points[2].pos.y, points[3].pos.y));

            Vector2D roomMid = new Vector2D((right + left) / 2, (top + bottom) / 2);
            if (roomNumber == 0) //Adds player in the centre of the first room
            {
            }
            else
            {
                if (roomNumber == 1)//Adds weapons immediately
                {
                    addThing(roomMid, "weapons");
                }

                if (roomNumber % 2 == 0)
                {
                    //Easy room
                    addThing(new Vector2D(roomMid.x + ((float)r.NextDouble() * (right - left) / 2),
                        roomMid.y + ((float)r.NextDouble() * (top - bottom) / 2) - (top - bottom) / 4), "weapons");
                    addThing(new Vector2D(roomMid.x + ((float)r.NextDouble() * (right - left) / 2),
                        roomMid.y + ((float)r.NextDouble() * (top - bottom) / 2) - (top - bottom) / 4), "ammunition");
                    addThing(new Vector2D(roomMid.x + ((float)r.NextDouble() * (right - left) / 2),
                        roomMid.y + ((float)r.NextDouble() * (top - bottom) / 2) - (top - bottom) / 4), "health");
                    spawnEnemies(SUBCATEGORIES.COMMON_MONSTERS, points, r.Next(3));
                }
                if (roomNumber % 3 == 0)
                {
                    spawnEnemies(SUBCATEGORIES.COMMON_MONSTERS, points, r.Next(4));
                    spawnEnemies(SUBCATEGORIES.UNCOMMON_MONSTERS, points, r.Next(2));
                }
                if (roomNumber % 5 == 0)
                {
                    spawnEnemies(SUBCATEGORIES.COMMON_MONSTERS, points, r.Next(2));
                    spawnEnemies(SUBCATEGORIES.UNCOMMON_MONSTERS, points, r.Next(4));
                    spawnEnemies(SUBCATEGORIES.RARE_MONSTERS, points, r.Next(3));
                }
                if (roomNumber % 7 == 0)
                {
                    spawnEnemies(SUBCATEGORIES.COMMON_MONSTERS, points, r.Next(2));
                    spawnEnemies(SUBCATEGORIES.UNCOMMON_MONSTERS, points, r.Next(2));
                    spawnEnemies(SUBCATEGORIES.RARE_MONSTERS, points, r.Next(2));
           //         spawnEnemies(SUBCATEGORIES.EPIC_MONSTERS, points, r.Next(2));
                }
                if (roomNumber % 11 == 0)
                {
               //     spawnEnemies(SUBCATEGORIES.COMMON_MONSTERS, points, r.Next(9));
               //     spawnEnemies(SUBCATEGORIES.UNCOMMON_MONSTERS, points, r.Next(4));
                    spawnEnemies(SUBCATEGORIES.EPIC_MONSTERS, points, r.Next(2));
                    //Generate health
                    addThing(new Vector2D(roomMid.x + ((float)r.NextDouble() * (right - left) / 2),
                        roomMid.y + ((float)r.NextDouble() * (top - bottom) / 2) - (top - bottom) / 4), "health"); //TODO always positive. allow negative.
                }
                if (roomNumber == 29) //numSectors -1!!TODO
                {
                    magicAddThing(roomMid, SUBCATEGORIES.LEGEND_MONSTERS);
                }

                while (r.NextDouble() > 0.7f) //30 percent chance of a decoration 
                    addThing(new Vector2D(roomMid.x + ((float)r.NextDouble() * (right - left) / 2),
                        roomMid.y + ((float)r.NextDouble() * (top - bottom) / 2) - (top - bottom) / 4), "decoration");
            }

        }

        private void spawnEnemies(SUBCATEGORIES monsterDifficulty, IList<DrawnVertex> roomBoundaries, int numMonsters = 0)
        {
            float left;
            float right;
            float top;
            float bottom;

            left = roomBoundaries[0].pos.x;
            right = roomBoundaries[0].pos.x;
            top = roomBoundaries[0].pos.y;
            bottom = roomBoundaries[0].pos.y;

            Random r = new Random();

            //TODO. don't populate this list every time we want to add new enemies. Perhaps a populate Lists method is necessary. 
            IList<ThingCategory> cats = General.Map.Data.ThingCategories;
            //Has thing already been placed?
            List<List<ThingTypeInfo>> SubCategories = new List<List<ThingTypeInfo>>();
            //THIS WILL ONLY WORK WITH DOOM2 FORMAT.
            SubCategories.Add(new List<ThingTypeInfo>()); //Minion monsters
            SubCategories[0].Add(cats[3].Things[8]); //Former Human
            SubCategories[0].Add(cats[3].Things[12]); //Imp
            SubCategories[0].Add(cats[3].Things[13]); // Lost Soul
                                                      //SubCategories[0].Add(cats[3].Things[21]); //Wolfenstein SS // I don't want these guys

            //Minion Leaders
            SubCategories.Add(new List<ThingTypeInfo>());
            SubCategories[1].Add(cats[3].Things[9]); //Former Sergeant
            SubCategories[1].Add(cats[3].Things[4]); //Chain gunner
            SubCategories[1].Add(cats[3].Things[7]); //Demon

            SubCategories.Add(new List<ThingTypeInfo>()); //Medium monsters
            SubCategories[2].Add(cats[3].Things[18]); //Revenant
            SubCategories[2].Add(cats[3].Things[10]); //Hell knight
            SubCategories[2].Add(cats[3].Things[19]); //Spectre //Invisible demons. I think I can do without
            SubCategories[2].Add(cats[3].Things[3]); //Cacodemon

            SubCategories.Add(new List<ThingTypeInfo>()); //Hard monsters
            SubCategories[3].Add(cats[3].Things[2]); //Archville
            SubCategories[3].Add(cats[3].Things[2]); //Baron of hell
            SubCategories[3].Add(cats[3].Things[17]); //Pain Elemental
            SubCategories[3].Add(cats[3].Things[14]); // Mancubus
            SubCategories[3].Add(cats[3].Things[0]); //Archanotron


            SubCategories.Add(new List<ThingTypeInfo>()); //Boss monsters
            SubCategories[4].Add(cats[3].Things[6]); //CyberDemon
            SubCategories[4].Add(cats[3].Things[20]); //Spider Mastermind

            foreach (DrawnVertex dv in points) //TODO. I did this better (more neatly at least in GenerateThings. 
            {
                if (dv.pos.x < left)
                    left = dv.pos.x;
                if (dv.pos.x > right)
                    right = dv.pos.x;
                if (dv.pos.y < bottom)
                    bottom = dv.pos.y;
                if (dv.pos.y > top)
                    top = dv.pos.y;
            }

            Thing[] monsters = new Thing[numMonsters];

            int index = SubCategories[(int)monsterDifficulty][r.Next(0, SubCategories[(int)monsterDifficulty].Count)].Index;

            for (int i = 0; i < numMonsters; i++)
            {
                int roomBorderBuffer = 50; //TODO experiment with this. Maybe I should change it for different monsters.
                monsters[i] = addThing(new Vector2D(r.Next((int)left + roomBorderBuffer, (int)right) - roomBorderBuffer, r.Next((int)bottom + roomBorderBuffer, (int)top - roomBorderBuffer)));

                if (monsters[i] != null)
                {
                    monsters[i].Type = index;
                }
            }
        }

        // We're going to use this to show the form
        public void ShowWindow(Form owner)
        {
            // Position this window in the left-top corner of owner
            this.Location = new Point(owner.Location.X + 20, owner.Location.Y + 90);

            // Show it
            base.Show(owner);
        }

        // Form is closing event
        private void GDForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // When the user is closing the window we want to cancel this, because it
            // would also unload (dispose) the form. We only want to hide the window
            // so that it can be re-used next time when this editing mode is activated.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Just cancel the editing mode. This will automatically call
                // OnCancel() which will switch to the previous mode and in turn
                // calls OnDisengage() which hides this window.
                General.Editing.CancelMode();
                e.Cancel = true;
            }
        }

        private void GDForm_Load(object sender, EventArgs e)
        {
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            General.Editing.CancelMode();
        }

        private Thing addThing(Vector2D pos, String category, float proba = 0.5f)
        {
            Thing t = addThing(pos);
            if (t != null)
            {

                IList<ThingCategory> cats = General.Map.Data.ThingCategories;
                Random r = new Random();

                bool found = false;
                foreach (ThingTypeInfo ti in General.Map.Data.ThingTypes)
                {
                    if (ti.Category.Name == category)
                    {
                        t.Type = ti.Index;
                        Console.WriteLine("Add thing cat " + category + " for thing at pos " + pos);
                        found = true;
                        if (r.NextDouble() > proba)
                            break;
                    }
                }
                if (!found)
                {
                    Console.WriteLine("###### Could not find category " + category + " for thing at pos " + pos);
                }
                else
                    t.Rotate(0);
            }
            else
            {
                Console.WriteLine("###### Could not add thing for cat " + category + " at pos " + pos);
            }

            return t;
        }

        private Thing addThing(Vector2D pos)
        {
            if (pos.x < General.Map.Config.LeftBoundary || pos.x > General.Map.Config.RightBoundary ||
                pos.y > General.Map.Config.TopBoundary || pos.y < General.Map.Config.BottomBoundary)
            {
                Console.WriteLine("Error Generaetive Doom: Failed to insert thing: outside of map boundaries.");
                return null;
            }

            // Create thing
            Thing t = General.Map.Map.CreateThing();
            if (t != null)
            {
                General.Settings.ApplyDefaultThingSettings(t);

                t.Move(pos);

                t.UpdateConfiguration();

                // Update things filter so that it includes this thing
                General.Map.ThingsFilter.Update();

                // Snap to map format accuracy
                t.SnapToAccuracy();
            }

            return t;
        }

        private Thing magicAddThing(Vector2D pos, SUBCATEGORIES subcategory)// originally had -> , float proba = 0.5f)
        {
            Thing t = addThing(pos);

            if (t != null)
            {
                IList<ThingCategory> cats = General.Map.Data.ThingCategories;
                //Has thing already been placed?
                List<List<ThingTypeInfo>> SubCategories = new List<List<ThingTypeInfo>>();
                //THIS WILL ONLY WORK WITH DOOM2 FORMAT. 
                SubCategories.Add(new List<ThingTypeInfo>()); //Minion monsters
                SubCategories[0].Add(cats[3].Things[8]); //Former Human
                SubCategories[0].Add(cats[3].Things[12]); //Imp
                SubCategories[0].Add(cats[3].Things[13]); // Lost Soul
                //SubCategories[0].Add(cats[3].Things[21]); //Wolfenstein SS // I don't want these guys

                //Minion Leaders
                SubCategories.Add(new List<ThingTypeInfo>());
                SubCategories[1].Add(cats[3].Things[9]); //Former Sergeant
                SubCategories[1].Add(cats[3].Things[4]); //Chain gunner
                SubCategories[1].Add(cats[3].Things[7]); //Demon

                SubCategories.Add(new List<ThingTypeInfo>()); //Medium monsters
                SubCategories[2].Add(cats[3].Things[18]); //Revenant
                SubCategories[2].Add(cats[3].Things[10]); //Hell knight
                SubCategories[2].Add(cats[3].Things[19]); //Spectre //Invisible demons. I think I can do without
                SubCategories[2].Add(cats[3].Things[3]); //Cacodemon

                SubCategories.Add(new List<ThingTypeInfo>()); //Hard monsters
                SubCategories[3].Add(cats[3].Things[2]); //Archville
                SubCategories[3].Add(cats[3].Things[2]); //Baron of hell
                SubCategories[3].Add(cats[3].Things[17]); //Pain Elemental
                SubCategories[3].Add(cats[3].Things[14]); // Mancubus
                SubCategories[3].Add(cats[3].Things[0]); //Archanotron


                SubCategories.Add(new List<ThingTypeInfo>()); //Boss monsters
                SubCategories[4].Add(cats[3].Things[6]); //CyberDemon
                SubCategories[4].Add(cats[3].Things[20]); //Spider Mastermind

                Random r = new Random();

                bool found = false;

                switch (subcategory)
                {
                    case SUBCATEGORIES.COMMON_MONSTERS:
                        int i = r.Next(0, SubCategories[(int)SUBCATEGORIES.COMMON_MONSTERS].Count);
                        t.Type = SubCategories[(int)SUBCATEGORIES.COMMON_MONSTERS][i].Index;
                        //roll probablitiy of the monster type 
                        break;
                    case SUBCATEGORIES.UNCOMMON_MONSTERS:
                        i = r.Next(0, SubCategories[(int)SUBCATEGORIES.UNCOMMON_MONSTERS].Count);
                        t.Type = SubCategories[(int)SUBCATEGORIES.UNCOMMON_MONSTERS][i].Index;
                        //roll probability of the monster type
                        break;
                    case SUBCATEGORIES.RARE_MONSTERS:
                        i = r.Next(0, SubCategories[(int)SUBCATEGORIES.RARE_MONSTERS].Count);
                        t.Type = SubCategories[(int)SUBCATEGORIES.RARE_MONSTERS][i].Index;
                        break;
                    case SUBCATEGORIES.EPIC_MONSTERS:
                        i = r.Next(0, SubCategories[(int)SUBCATEGORIES.EPIC_MONSTERS].Count);
                        t.Type = SubCategories[(int)SUBCATEGORIES.EPIC_MONSTERS][i].Index;
                        break;
                    case SUBCATEGORIES.LEGEND_MONSTERS:
                        i = r.Next(0, SubCategories[(int)SUBCATEGORIES.LEGEND_MONSTERS].Count);
                        t.Type = SubCategories[(int)SUBCATEGORIES.LEGEND_MONSTERS][i].Index;
                        break;
                }

                if (!found)
                {
                    Console.WriteLine("###### Could not find category " + subcategory + " for thing at pos " + pos);
                }
                else
                    t.Rotate(0);
            }
            else
            {
                Console.WriteLine("###### Could not add thing for cat " + subcategory + " at pos " + pos);
            }

            return t;
        }

        private void correctMissingTex()
        {

            String defaulttexture = "-";
            if (General.Map.Data.TextureNames.Count > 1)
                defaulttexture = General.Map.Data.TextureNames[1];

            // Go for all the sidedefs
            foreach (Sidedef sd in General.Map.Map.Sidedefs)
            {
                // Check upper texture. Also make sure not to return a false
                // positive if the sector on the other side has the ceiling
                // set to be sky
                if (sd.HighRequired() && sd.HighTexture[0] == '-')
                {
                    if (sd.Other != null && sd.Other.Sector.CeilTexture != General.Map.Config.SkyFlatName)
                    {
                        sd.SetTextureHigh(General.Settings.DefaultCeilingTexture);
                    }
                }

                // Check middle texture
                if (sd.MiddleRequired() && sd.MiddleTexture[0] == '-')
                {
                    sd.SetTextureMid(General.Settings.DefaultTexture);
                }

                // Check lower texture. Also make sure not to return a false
                // positive if the sector on the other side has the floor
                // set to be sky
                if (sd.LowRequired() && sd.LowTexture[0] == '-')
                {
                    if (sd.Other != null && sd.Other.Sector.FloorTexture != General.Map.Config.SkyFlatName)
                    {
                        sd.SetTextureLow(General.Settings.DefaultFloorTexture);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MakeSidMeiersCivilizationVI();
        }

        //Cut Content :'(
        /*
        private void GenerateCorridor()
        {
            prevV = v.pos;

            int corridorWidth = r.Next(128, 196);
            int corridorLength = r.Next(512, 1024);

            maxWidth += corridorLength;
            maxHeight += corridorLength;

            CheckDirection();
            floor = prevFloor;
            ceil = prevCeil;

            switch (nextDirection)
            {
                case DIRECTION.RIGHT: //right
                    Console.WriteLine("We are going right !");
                    v.pos.x += prevWidth;
                    v.pos.y -= Math.Min(height, prevHeight) / 2 - corridorWidth / 2;
                    //newSector(v, corridorLength, corridorWidth, illumination, ceil, floor);
                    prevWidth = corridorLength;
                    v.pos.y += Math.Min(height, prevHeight) / 2 - corridorWidth / 2;
                    break;
                case DIRECTION.LEFT: //left
                    Console.WriteLine("We are going left !");
                    v.pos.x -= corridorLength;
                    v.pos.y -= Math.Min(height, prevHeight) / 2 - corridorWidth / 2;
                    //  newSector(v, corridorLength, corridorWidth, illumination, ceil, floor);
                    prevWidth = corridorLength;
                    v.pos.y += Math.Min(height, prevHeight) / 2 - corridorWidth / 2;
                    break;
                case DIRECTION.UP:
                    Console.WriteLine("We are going up !");
                    v.pos.y += corridorLength;
                    v.pos.x += Math.Min(width, prevWidth) / 2 - corridorWidth / 2;
                    // newSector(v, corridorWidth, corridorLength, illumination, ceil, floor);
                    prevHeight = corridorLength;
                    v.pos.x -= Math.Min(width, prevWidth) / 2 - corridorWidth / 2;
                    break;
                case DIRECTION.DOWN:
                    Console.WriteLine("We are going down !");
                    v.pos.y -= prevHeight;
                    v.pos.x += Math.Min(width, prevWidth) / 2 - corridorWidth / 2;
                    //newSector(v, corridorWidth, corridorLength, illumination, ceil, floor);
                    prevHeight = corridorLength;
                    v.pos.x -= Math.Min(width, prevWidth) / 2 - corridorWidth / 2;
                    break;
                default:
                    break;
            }
            }*/
        //TODO Update doorways for new intersection
        private void GenerateDoorWay()
        {
            float doorWidth = 0;
            float minDoorWidth = 80;
            float maxDoorWidth = 400;//TODO size door for room//Math.Min(directionFace.GetLength(), nextFace.GetLength() )* 0.9f;
            float doorDepth = 0;
            float minDoorDepth = 10;
            float maxDoorDepth = 40;

            doorDepth = r.Next((int)minDoorDepth, (int)maxDoorDepth);
            doorWidth = r.Next((int)minDoorWidth, (int)maxDoorWidth);

            Vector2D[] cornerArr = new Vector2D[4];
            float wRatio;
            if (directionFace.GetLength() != 0)
                wRatio = doorWidth / directionFace.GetLength();
            else
                wRatio = 0;

            float connectPos = (float)r.NextDouble();

            //Unlike the rooms. If we go outide the bounds of a room we want to use that as our edge limit.
            if (connectPos - wRatio / 2 < 0.0f)
                cornerArr[0] = directionFace.GetCoordinatesAt(0.0f);
            else
                cornerArr[0] = directionFace.GetCoordinatesAt(connectPos - wRatio / 2);

            cornerArr[1] = cornerArr[0] + (directionFace.GetPerpendicular().GetNormal() * doorDepth);

            //Assigning cornerArr[3] before cornerArr[2] since we'll need to use it to determine cornerArr[2]
            if (connectPos + wRatio / 2 > 1.0f)
                cornerArr[3] = directionFace.GetCoordinatesAt(1.0f);
            else
                cornerArr[3] = directionFace.GetCoordinatesAt(connectPos + wRatio / 2);

            cornerArr[2] = cornerArr[3] + (directionFace.GetPerpendicular().GetNormal() * doorDepth);

            for (int i = 0; i < cornerArr.Length; ++i)
            {
                v.pos = cornerArr[i];
                points.Add(v);
            }

            newSector(points, illumination, ceil, floor);
            points.Clear();

            directionFace = new Line2D(cornerArr[1], cornerArr[2]);
        }

        private void GenerateDoor()
        {
            float doorWidth = 0;
            int minDoorWidth = 80;
            float doorDepth = 0;
            int minDoorDepth = 10;
            int maxDoorDepth = 40;

            doorDepth = r.Next(minDoorDepth, maxDoorDepth);

            /*switch (nextDirection)
            {
                case DIRECTION.RIGHT:
                    v.pos.x += prevWidth;
                    doorWidth = r.Next(minDoorWidth, (int)(Math.Min(height, prevHeight) * 0.9));
                    v.pos.y -= Math.Min(height, prevHeight) / 2 - doorWidth / 2; //Centres the door with the smaller room
                                                                                 //    newSector(v, doorDepth, doorWidth, illumination, ceil, floor);
                    prevWidth = doorDepth;
                    v.pos.y += Math.Min(height, prevHeight) / 2 - doorWidth / 2;
                    break;
                case DIRECTION.LEFT:
                    v.pos.x -= doorDepth;
                    doorWidth = r.Next(minDoorWidth, (int)(Math.Min(height, prevHeight) * 0.9));
                    v.pos.y -= Math.Min(height, prevHeight) / 2 - doorWidth / 2;
                    //  newSector(v, doorDepth, doorWidth, illumination, ceil, floor);
                    prevWidth = doorDepth;
                    v.pos.y += Math.Min(height, prevHeight) / 2 - doorWidth / 2;
                    break;
                case DIRECTION.UP:
                    v.pos.y += doorDepth;
                    doorWidth = r.Next(minDoorWidth, (int)(Math.Min(width, prevWidth) * 0.9));
                    v.pos.x += Math.Min(width, prevWidth) / 2 - doorWidth / 2;
                    //  newSector(v, doorWidth, doorDepth, illumination, ceil, floor);
                    prevHeight = doorDepth;
                    v.pos.x -= Math.Min(width, prevWidth) / 2 - doorWidth / 2;
                    break;
                case DIRECTION.DOWN:
                    v.pos.y -= prevHeight;
                    doorWidth = r.Next(minDoorWidth, (int)(Math.Min(width, prevWidth) * 0.9));
                    v.pos.x += Math.Min(width, prevWidth) / 2 - doorWidth / 2;
                    //newSector(v, doorWidth, doorDepth, illumination, ceil, floor);
                    prevHeight = doorDepth;
                    v.pos.x -= Math.Min(width, prevWidth) / 2 - doorWidth / 2;
                    break;
                default:
                    break;
            }


            if (r.NextDouble() > 0.25)//Chance to generate a door.
            {
                Sector doorSector = allSectors.Last<Sector>(); //TODO I think there's some redundnacy. I believe sectors are being stored in some other list and i can get the last one from that.

                doorSector.CeilHeight = doorSector.FloorHeight;

                float length = 0;
                foreach (Sidedef sdef in doorSector.Sidedefs)
                {
                    if (length < sdef.Line.Length)
                        length = sdef.Line.Length;
                }

                List<Linedef> longLines = new List<Linedef>();

                foreach (Sidedef sdef in doorSector.Sidedefs)
                {
                    if (length <= sdef.Line.Length)
                    {
                        longLines.Add(sdef.Line);
                    }
                    else
                        sdef.Line.SetFlag(General.Map.Config.LowerUnpeggedFlag, true);
                }

                if (longLines.Count != 2)
                {
                    Console.WriteLine("Door error, long Lines");
                }
                else
                {
                    longLines[0].Action = 1;
                    longLines[1].Action = 1;
                    longLines[0].SetFlag(General.Map.Config.SoundLinedefFlag, true);//Blocks sound from travelling through doors.
                    longLines[1].SetFlag(General.Map.Config.SoundLinedefFlag, true);

                    switch (nextDirection)
                    {
                        case DIRECTION.RIGHT:
                            if (longLines[0].GetCenterPoint().x >= longLines[1].GetCenterPoint().x)
                            {
                                longLines[0].FlipVertices(); longLines[0].FlipSidedefs();
                            }
                            else
                            {
                                longLines[1].FlipVertices(); longLines[1].FlipSidedefs();
                            }
                            break;
                        case DIRECTION.LEFT:
                            if (longLines[0].GetCenterPoint().x <= longLines[1].GetCenterPoint().x)
                            {
                                longLines[0].FlipVertices(); longLines[0].FlipSidedefs();
                            }
                            else
                            {
                                longLines[1].FlipVertices(); longLines[1].FlipSidedefs();
                            }
                            break;
                        case DIRECTION.UP:
                            if (longLines[0].GetCenterPoint().y >= longLines[1].GetCenterPoint().y)
                            { longLines[0].FlipVertices(); longLines[0].FlipSidedefs(); }
                            else
                            {
                                longLines[1].FlipVertices(); longLines[1].FlipSidedefs();
                            }
                            break;
                        case DIRECTION.DOWN:
                            if (longLines[0].GetCenterPoint().y <= longLines[1].GetCenterPoint().y)
                            { longLines[0].FlipVertices(); longLines[0].FlipSidedefs(); }
                            else
                            {
                                longLines[1].FlipVertices(); longLines[1].FlipSidedefs();
                            }
                            break;
                    }
                }
            }*/
        }

        /*TODO
         * Stairs
         * Cover
         * Textutes & Decoration
         * Lava
         * Bridges
         * Redo Doors so they work with new direction check.
         * Locked doors and keys
         * Rooms/Paths with jagged edges. 
         */
    }
}