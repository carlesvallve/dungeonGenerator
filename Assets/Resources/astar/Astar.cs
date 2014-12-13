using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.IO;
//using System.Text;

/*
;===================================================================
;A* Pathfinder (Version 1.71a) by Patrick Lester. Used by permission.
;===================================================================
;Last updated 06/16/03 -- Visual C++ version
;Edited by Ramen Sama for use in Torque 5.13.06about to
;Edited by Carles Vallve, 
 * changed to c# for use in Unity3d
 * many issues where removed/simplified for more transparent-easy use.
*/

public class Astar {
    //Declare constants
    bool cutAcrossCorners = false; //activates-deactivates cut acroos corners functionallity
    int mapWidth, mapHeight;
    int onClosedList = 10;
    const int notfinished = 0, notStarted = 0, found = 1, nonexistent = 2; // path-related constants
    const int walkable = 0, unwalkable = 1; // walkability array constants
    //Create needed arrays
    public int[,] walkability;
    int[] openList;     //1 dimensional array holding ID# of open list items
    int[,] whichList;   //2 dimensional array used to record 
    //whether a cell is on the open list or on the closed list.
    int[] openX, openY;    //1d arrays stores the x and y locations of an item on the open list
    int[,] parentX, parentY; //2d arrays to store parent of each cell (x and y)
    //Cell costs
    int[] Fcost;	//1d array to store F cost of a cell on the open list
    int[,] Gcost; 	//2d array to store G cost for each cell.
    int[] Hcost;	//1d array to store H cost of a cell on the open list
    //Path in int-array format
    int[] mypath;
    int pathLength = notStarted; //stores length of the found path for critter	

    //-----------------------------------------------------------------------------
    // Name: AstarLibrary 
    // Desc: Initializes/Redimensionates all required vars and generates walkability map.
    //-----------------------------------------------------------------------------

    public Astar(Tile[,] arr) {
        //get map dimensions
        mapWidth = arr.GetLength(0);
        mapHeight = arr.Length / arr.GetLength(0);
        //Redimensionate needed arrays
        openList = new int[mapWidth * mapHeight + 2]; //1 dimensional array holding ID# of open list items
        whichList = new int[mapWidth + 1, mapHeight + 1];  //2 dimensional array used to record 
        //whether a cell is on the open list or on the closed list.
        openX = new int[mapWidth * mapHeight + 2]; //1d array stores the x location of an item on the open list
        openY = new int[mapWidth * mapHeight + 2]; //1d array stores the y location of an item on the open list
        parentX = new int[mapWidth + 1, mapHeight + 1]; //2d array to store parent of each cell (x)
        parentY = new int[mapWidth + 1, mapHeight + 1]; //2d array to store parent of each cell (y)
        Fcost = new int[mapWidth * mapHeight + 2];	//1d array to store F cost of a cell on the open list
        Gcost = new int[mapWidth + 1, mapHeight + 1]; 	//2d array to store G cost for each cell.
        Hcost = new int[mapWidth * mapHeight + 2];	//1d array to store H cost of a cell on the open list	
        //walkability array: is a bidimensional int-array of ones and zeros that we generate from cells walkable states.
        walkability = new int[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                if (arr[x, y].getWalkable()) { walkability[x, y] = walkable; } else { walkability[x, y] = unwalkable; }
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Name: SearchPath
    // Desc: Called by javascript. Calls FindPath and returns the path to javascript.
    //-----------------------------------------------------------------------------

    public List<Vector2> SearchPath(Vector3 start, Vector3 end) {
        int startingX = (int)start.x;
        int startingY = (int)start.z;
        int targetX = (int)end.x;
        int targetY = (int)end.z;

        //get the path in int-array format
        int[] mypath = FindPath(startingX, startingY, targetX, targetY);
        //convert path to Vector2-list
        List<Vector2> pathlist = ConvertPath(mypath);
        //return path
        return pathlist;
    }


    public void PrintPath (List<Vector2> path) {
        string str = "Path: ";
        for (int i = 0; i < path.Count; i++) {
            str += path[i] + " ";
        }
        Debug.Log(str);
    }

    /*public List<Vector2> SearchPath(int startingX, int startingY, int targetX, int targetY) {
        //get the path in int-array format
        int[] mypath = FindPath(startingX, startingY, targetX, targetY);
        //convert path to Vector2-list
        List<Vector2> pathlist = ConvertPath(mypath);
        //return path
        return pathlist;
    }*/

    //-----------------------------------------------------------------------------
    // Name: ConvertPath
    // Desc: converts path given in format [x0,y0,x1,y1...] to a Vector2-List.
    //-----------------------------------------------------------------------------

    List<Vector2> ConvertPath(int[] lista) {
        List<Vector2> pathlist = new List<Vector2>();
        for (int n = 0; n < lista.Length; n += 2) {
            pathlist.Add(new Vector3(lista[n], lista[n + 1]));
        }
        pathlist.Reverse();
        return pathlist;
    }

    //-----------------------------------------------------------------------------
    // Name: FindPath
    // Desc: Finds a path using A*
    //-----------------------------------------------------------------------------

    int[] FindPath(int startX, int startY, int targetX, int targetY) {

        int onOpenList = 0, parentXval = 0, parentYval = 0,
        a = 0, b = 0, m = 0, u = 0, v = 0, temp = 0, corner = 0, numberOfOpenListItems = 0,
        addedGCost = 0, tempGcost = 0, path = 0,
        tempx, pathX, pathY,
        newOpenListItemID = 0;

        //2.Quick Path Checks: Under the some circumstances no path needs to be generated ...
        //	If target square is unwalkable, return that it's a nonexistent path.
        if (walkability[targetX, targetY] == unwalkable) { goto noPath; }

        //3.Reset some variables that need to be cleared
        if (onClosedList > 1000000) { //reset whichList occasionally
            for (int x = 0; x < mapWidth; x++) {
                for (int y = 0; y < mapHeight; y++)
                    whichList[x, y] = 0;
            }
            onClosedList = 10;
        }

        //changing the values of onOpenList and onClosed list is faster than redimming whichList() array
        onClosedList = onClosedList + 2;
        onOpenList = onClosedList - 1;

        pathLength = notStarted;
        Gcost[startX, startY] = 0; //reset starting square's G value to 0

        //4.Add the starting location to the open list of squares to be checked.
        numberOfOpenListItems = 1;
        //assign it as the top (and currently only) item in the open list, which is maintained as a binary heap (see below)
        openList[1] = 1;
        openX[1] = startX; openY[1] = startY;

        //5.Do the following until a path is found or deemed nonexistent.
        bool e = true;
        do {
            //6.If the open list is not empty, take the first cell off of the list.
            //	This is the lowest F cost cell on the open list.
            if (numberOfOpenListItems != 0) {

                //7. Pop the first item off the open list.
                parentXval = openX[openList[1]];
                parentYval = openY[openList[1]]; //record cell coordinates of the item
                whichList[parentXval, parentYval] = onClosedList;//add the item to the closed list

                //	Open List = Binary Heap: Delete this item from the open list, which
                //  is maintained as a binary heap. For more information on binary heaps, see:
                //	http://www.policyalmanac.org/games/binaryHeaps.htm

                numberOfOpenListItems = numberOfOpenListItems - 1;//reduce number of open list items by 1	

                //	Delete the top item in binary heap and reorder the heap, with the lowest F cost item rising to the top.
                openList[1] = openList[numberOfOpenListItems + 1];//move the last item in the heap up to slot #1
                v = 1;

                //	Repeat the following until the new item in slot #1 sinks to its proper spot in the heap.
                do {
                    u = v;
                    if (2 * u + 1 <= numberOfOpenListItems) { //if both children exist
                        //Check if the F cost of the parent is greater than each child.
                        //Select the lowest of the two children.
                        if (Fcost[openList[u]] >= Fcost[openList[2 * u]]) {
                            v = 2 * u;
                            if (Fcost[openList[v]] >= Fcost[openList[2 * u + 1]]) { v = 2 * u + 1; }
                        }
                    }
                    else {
                        if (2 * u <= numberOfOpenListItems) { //if only child #1 exists
                            //Check if the F cost of the parent is greater than child #1	
                            if (Fcost[openList[u]] >= Fcost[openList[2 * u]]) { v = 2 * u; }
                        }
                    }

                    if (u != v) { //if parent's F is > one of its children, swap them
                        temp = openList[u];
                        openList[u] = openList[v];
                        openList[v] = temp;
                    }
                    else { break; } //otherwise, exit loop

                } while (e); //changed to prevent stopping of process. reorder the binary heap


                //7.Check the adjacent squares. (Its "children" -- these path children
                //	are similar, conceptually, to the binary heap children mentioned
                //	above, but don't confuse them. They are different. Path children
                //	are portrayed in Demo 1 with grey pointers pointing toward
                //	their parents.) Add these adjacent child squares to the open list
                //	for later consideration if appropriate (see various if statements below).
                for (b = parentYval - 1; b <= parentYval + 1; b++) {
                    for (a = parentXval - 1; a <= parentXval + 1; a++) {

                        //	If not off the map (do this first to avoid array out-of-bounds errors)
                        if (a != -1 && b != -1 && a != mapWidth && b != mapHeight) {

                            //If not already on the closed list (items on the closed list have
                            //already been considered and can now be ignored).			
                            if (whichList[a, b] != onClosedList) {

                                //If not a wall/obstacle square. //yield return WaitForSeconds(0.1);
                                if (walkability[a, b] != unwalkable) {

                                    //	Don't cut across corners if flag is active
                                    corner = walkable;
                                    if (cutAcrossCorners == true) {
                                        if (a == parentXval - 1) {
                                            if (b == parentYval - 1) {
                                                if (walkability[parentXval - 1, parentYval] == unwalkable
                                                    || walkability[parentXval, parentYval - 1] == unwalkable) {
                                                    corner = unwalkable;
                                                }
                                            }
                                            else if (b == parentYval + 1) {
                                                if (walkability[parentXval, parentYval + 1] == unwalkable
                                                    || walkability[parentXval - 1, parentYval] == unwalkable)
                                                    corner = unwalkable;
                                            }
                                        }
                                        else if (a == parentXval + 1) {
                                            if (b == parentYval - 1) {
                                                if (walkability[parentXval, parentYval - 1] == unwalkable
                                                    || walkability[parentXval + 1, parentYval] == unwalkable) {
                                                    corner = unwalkable;
                                                }
                                            }
                                            else if (b == parentYval + 1) {
                                                if (walkability[parentXval + 1, parentYval] == unwalkable
                                                    || walkability[parentXval, parentYval + 1] == unwalkable)
                                                    corner = unwalkable;
                                            }
                                        }
                                    }



                                    if (corner == walkable) {

                                        //If not already on the open list, add it to the open list.			
                                        if (whichList[a, b] != onOpenList) {

                                            //Create a new open list item in the binary heap.
                                            newOpenListItemID = newOpenListItemID + 1; //each new item has a unique ID #
                                            m = numberOfOpenListItems + 1;
                                            openList[m] = newOpenListItemID;//place the new open list item (actually, its ID#) at the bottom of the heap
                                            openX[newOpenListItemID] = a;
                                            openY[newOpenListItemID] = b;//record the x and y coordinates of the new item

                                            //Figure out its G cost
                                            if (Mathf.Abs(a - parentXval) == 1 && Mathf.Abs(b - parentYval) == 1)
                                                addedGCost = 14;//cost of going to diagonal squares	
                                            else
                                                addedGCost = 10;//cost of going to non-diagonal squares				
                                            Gcost[a, b] = Gcost[parentXval, parentYval] + addedGCost;

                                            //Determine the direction of travel from the previous square to this one
                                            //Add a penalty for direction change
                                            //int xDif = (a-parentXVal);
                                            //int yDif = (b-parntYVal);

                                            //Figure out its H and F costs and parent
                                            Hcost[openList[m]] = 10 * (Mathf.Abs(a - targetX) + Mathf.Abs(b - targetY));
                                            Fcost[openList[m]] = Gcost[a, b] + Hcost[openList[m]];
                                            parentX[a, b] = parentXval; parentY[a, b] = parentYval;

                                            //Move the new open list item to the proper place in the binary heap.
                                            //Starting at the bottom, successively compare to parent items,
                                            //swapping as needed until the item finds its place in the heap
                                            //or bubbles all the way to the top (if it has the lowest F cost).
                                            while (m != 1) //While item hasn't bubbled to the top (m=1)	
		{
                                                //Check if child's F cost is < parent's F cost. If so, swap them.	
                                                if (Fcost[openList[m]] <= Fcost[openList[m / 2]]) {
                                                    temp = openList[m / 2];
                                                    openList[m / 2] = openList[m];
                                                    openList[m] = temp;
                                                    m = m / 2;
                                                }
                                                else
                                                    break;
                                            }
                                            numberOfOpenListItems = numberOfOpenListItems + 1;//add one to the number of items in the heap

                                            //Change whichList to show that the new item is on the open list.
                                            whichList[a, b] = onOpenList;
                                        }



                                        //8.If adjacent cell is already on the open list, check to see if this 
                                        //	path to that cell from the starting location is a better one. 
                                        //	If so, change the parent of the cell and its G and F costs.	
                                        else //If whichList(a,b) = onOpenList
	{

                                            //Figure out the G cost of this possible new path
                                            if (Mathf.Abs(a - parentXval) == 1 && Mathf.Abs(b - parentYval) == 1)
                                                addedGCost = 14;//cost of going to diagonal tiles	
                                            else
                                                addedGCost = 10;//cost of going to non-diagonal tiles				
                                            tempGcost = Gcost[parentXval, parentYval] + addedGCost;

                                            //If this path is shorter (G cost is lower) then change
                                            //the parent cell, G cost and F cost. 		
                                            if (tempGcost < Gcost[a, b]) //if G cost is less,
		{
                                                parentX[a, b] = parentXval; //change the square's parent
                                                parentY[a, b] = parentYval;
                                                Gcost[a, b] = tempGcost;//change the G cost			

                                                //Because changing the G cost also changes the F cost, if
                                                //the item is on the open list we need to change the item's
                                                //recorded F cost and its position on the open list to make
                                                //sure that we maintain a properly ordered open list.
                                                for (int x = 1; x <= numberOfOpenListItems; x++) //look for the item in the heap
			{
                                                    if (openX[openList[x]] == a && openY[openList[x]] == b) //item found
			{
                                                        Fcost[openList[x]] = Gcost[a, b] + Hcost[openList[x]];//change the F cost

                                                        //See if changing the F score bubbles the item up from it's current location in the heap
                                                        m = x;
                                                        while (m != 1) //While item hasn't bubbled to the top (m=1)	
				{
                                                            //Check if child is < parent. If so, swap them.	
                                                            if (Fcost[openList[m]] < Fcost[openList[m / 2]]) {
                                                                temp = openList[m / 2];
                                                                openList[m / 2] = openList[m];
                                                                openList[m] = temp;
                                                                m = m / 2;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        break; //exit for x = loop
                                                    } //If openX(openList(x)) = a
                                                } //For x = 1 To numberOfOpenListItems
                                            }//If tempGcost < Gcost(a,b)

                                        }//else If whichList(a,b) = onOpenList	
                                    }//If not cutting a corner
                                }//If not a wall/obstacle square.
                            }//If not already on the closed list 
                        }//If not off the map
                    }//for (a = parentXval-1; a <= parentXval+1; a++){
                }//for (b = parentYval-1; b <= parentYval+1; b++){

            }//if (numberOfOpenListItems != 0)


    //9.If open list is empty then there is no path.	
            else {
                path = nonexistent; break;
            }

            //If target is added to open list then path has been found.
            if (whichList[targetX, targetY] == onOpenList) {
                path = found; break;
            }

        }
        while (e);//Do until path is found or deemed nonexistent


        //-------------------------------------------------------------------	
        //10.Return the path if it exists.
        if (path == found) {

            //a.Working backwards from the target to the starting location by checking
            //each cell's parent, figure out the length of the path.
            pathX = targetX; pathY = targetY;
            do {
                //Look up the parent of the current cell.	
                tempx = parentX[pathX, pathY];
                pathY = parentY[pathX, pathY];
                pathX = tempx;
                //Figure out the path length
                pathLength = pathLength + 1;
            }
            while (pathX != startX || pathY != startY);

            //c. Now copy the path information over to the databank. Since we are
            //	working backwards from the target to the start location, we copy
            //	the information to the data bank in reverse order. The result is
            //	a properly ordered set of path data, from the first step to the
            //	last.
            pathX = targetX; pathY = targetY;

            //initialize mypath (charlie!)
            int n = 0;
            mypath = new int[(pathLength + 1) * 2];
            mypath[0] = pathX;
            mypath[1] = pathY;

            do {

                //d.Look up the parent of the current cell.	
                tempx = parentX[pathX, pathY];
                pathY = parentY[pathX, pathY];
                pathX = tempx;

                //fill my path with new node x,y values (charlie!)
                n += 2;
                mypath[n] = pathX;
                mypath[n + 1] = pathY;
            }
            while (pathX != startX || pathY != startY);	//e.If we have reached the starting square, exit the loop.	

            return mypath; //a path was generated: return path (charlie!)
        }
    //-------------------------------------------------------------------

    //11.If there is no path to the selected target, 
    //return a 0 length path array. (charlie!)
    noPath:
        mypath = new int[0];
        return mypath;
    }
    //-------------------------------------------------------------------


}
