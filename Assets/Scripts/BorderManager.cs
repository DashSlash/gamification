using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;

public class ObjectToShow // store GameObject and change renderer whenever adjacent light is on or off
{
    internal GameObject game;
    GameObject[] light; // store the gameobject of light class
    public ObjectToShow(GameObject a)
    {
        game = a;
        light = new GameObject[0];
    }

    public ObjectToShow(GameObject a, GameObject[] l)
    {
        //Debug.Log("init");
        game = a;
        light = l;
        for(int i = 0; i < l.Length; i++)
        {
            l[i].GetComponent<Light>().PropertyChanged += new PropertyChangedEventHandler(SetRenderer);
        }
    }


    public void SetRenderer(object sender, PropertyChangedEventArgs e)
    {
        for(int i = 0; i < light.Length; i++)
        {
            if (light[i].GetComponent<Light>().IsOn)
            {
                game.GetComponent<Renderer>().enabled = true;

                //Debug.Log("received change");
                if (game.GetComponent<Animator>() != null)
                {
                    game.GetComponent<Animator>().SetTrigger("showUp");
                }
                return;
            }
        }
        game.GetComponent<Renderer>().enabled = false;
        if (game.GetComponent<Animator>() != null)
            game.GetComponent<Animator>().enabled = false;
    }
}

public class GraphNode
{
    public Vector2_int pos;
    //public Vector2_int prev;
    public bool visited;
    public int cost;
    public List<GraphNode> adjacent;
    public GraphNode previous;
    public int mapping_index = 0;

    public GraphNode(Vector2_int a)
    {
        pos = a;
        visited = false;
        cost = int.MaxValue; // int.MaxValue, because the initial value for dijkstra's algorithm should be infinity
        adjacent = new List<GraphNode>();
        previous = null;
    }
}

public class Heap
{
    internal int currentSize;
    internal GraphNode[] heap_arr;

    public Heap()
    {
        currentSize = 0;
    }

    public void Insert(GraphNode x)
    {
        if(heap_arr.Length == currentSize)
        {
            GraphNode[] tmp = new GraphNode[heap_arr.Length * 2];
            heap_arr.CopyTo(tmp, 0);
            heap_arr = tmp;
        }
        Move(x, 0); // heap_arr[0] = x; heap_arr[0].mapping_index = 0;
        int hole = ++currentSize;
        Move(x, hole);
        PercolateUp(hole);
    }

    public void Move(GraphNode node, int index)
    {
        heap_arr[index] = node;
        //FileStream fs = new FileStream("Running.log", FileMode.Append);
        //string s = node.pos.x + " " + node.pos.y + " " + node.cost + " " + node.visited + " MapIndex " + node.mapping_index + "\n";
        //byte[] buffer = Encoding.ASCII.GetBytes(s);
        //fs.Write(buffer, 0, buffer.Length);
        //fs.Close();
        //Debug.Log(s);
        heap_arr[index].mapping_index = index;
    }

    public GraphNode Pop(int index)
    {
        heap_arr[index].mapping_index = -1;
        return heap_arr[index];
    }

    public GraphNode DeleteMin()
    {
        GraphNode result = Pop(1);// heap_arr[1]; heap_arr[1].mapping_index = -1;
        result.visited = true;
        Move(heap_arr[currentSize--],1); //heap_arr[1] = heap_arr[currentSize--]; heap_arr[1].mapping_index = 1;
        PercolateDown(1);
        return result;
    }

    public void PercolateUp(int index)
    {
        Move(heap_arr[index], 0);// heap_arr[0] = heap_arr[index]; heap_arr[0].mapping_index = 0;
        for (; heap_arr[0].cost < heap_arr[index/2].cost; index /= 2)
        {
            Move(heap_arr[index / 2], index); //heap_arr[index] = heap_arr[index / 2]; heap_arr[index].mapping_index = index;
        }
        Move(heap_arr[0], index); // heap_arr[index] = heap_arr[0]; heap_arr[index].mapping_index = index;
    }

    public void PercolateDown( int index)
    {
        Move(heap_arr[index], 0); // heap_arr[0] = heap_arr[index]; heap_arr[0].mapping_index = 0;
        int child = index << 1;
        while (child <= currentSize)
        {
            if (child + 1 <= currentSize && heap_arr[child + 1].cost < heap_arr[child].cost)
                child++;
            if (heap_arr[child].cost < heap_arr[0].cost)
            {
                Move(heap_arr[child], index); // heap_arr[index] = heap_arr[child]; heap_arr[index].mapping_index = index;
                index = child;
                child = child << 1;
            }
            else
                break;
        }
        Move(heap_arr[0], index); //heap_arr[index] = heap_arr[0]; heap_arr[index].mapping_index = index;
    }

    // we want to use HeapSort to get the min path node
    public void BuildHeap(GraphNode[][] graph)
    {
        heap_arr = new GraphNode[graph.Length * graph[0].Length + 10];
        currentSize = graph.Length * graph[0].Length;
        for (int i = 0; i < graph.Length; i++)
        {
            for (int j = 0; j < graph[0].Length; j++)
            {
                Move(graph[i][j], i * graph[0].Length + j + 1); 
                //heap_arr[i * graph[0].Length + j + 1] = graph[i][j]; heap_arr[i * graph[0].Length + 1].mapping_index = i * graph[0].Length + 1;
            }
        }
        for (int i = currentSize / 2; i > 0; i--)
            PercolateDown(i);
    }
}

public struct Vector2_int
{
    public int x;
    public int y;

    public Vector2_int( float a, float b)
    {
        x = (int)a;
        y = (int)b;
    }

    public Vector2_int(int a, int b)
    {
        x = a;
        y = b;
    }
}

[Serializable] // useful to store the class's state and helpful when you want to reconstruct a class with same content
public class Count
{
    public int min;
    public int max;

    public Count(int m1, int m2)
    {
        min = m1;
        max = m2;
    }
}

public class BorderManager : MonoBehaviour {
    
    
    
    public int adj = 3;
    public int col = 8;
    public int row = 8;
    public Count wallCount;// = new Count( 5, 9);
    public Count foodCount;// = new Count(5, 9);
    public Count lightCount;// = new Count(2, 5);
    public GameObject exit;
    public GameObject light_origin;
    public new GameObject[] light; // to store the prefab light gameobject
    public List<GameObject> lights = new List<GameObject>(); // to store the lights objects, which are instantiated components of light gameobject and store their locations
    public GameObject[] floorTiles;
    public GameObject[] inwallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerwallTiles;
    public GameObject[][] ground;
    //public int distance = 5;

    public Dictionary<Vector2_int, ObjectToShow> objtoshow_dict = new Dictionary<Vector2_int, ObjectToShow>();
    public Dictionary<Vector2_int, int> obstacle_level = new Dictionary<Vector2_int, int>(); // this is the cost from adjacent grid cell to this cell
    public List<Vector2_int> changed_everyupdate = new List<Vector2_int>();
    private List<ObjectToShow> objtoshow = new List<ObjectToShow>();
    private Transform boardHolder;
    private List<Vector3> gridPosition = new List<Vector3>(); // track objects' position

    void InitList()
    {
        objtoshow_dict.Clear();
        changed_everyupdate.Clear();
        lights.Clear();
        objtoshow.Clear();
        gridPosition.Clear();
        obstacle_level.Clear();
        ground = new GameObject[col + 2][];
        for(int i = 0; i < col + 2; i++)
        {
            ground[i] = new GameObject[row + 2];
        }

        for (int x = -1; x < col + 1; x++)
        {
            for (int y = -1; y < row + 1; y++)
            {
                GameObject toInit = floorTiles[Random.Range(0,floorTiles.Length)];
                if (x == -1 || x == col || y == -1 || y == row)
                {
                    toInit = outerwallTiles[Random.Range(0, outerwallTiles.Length)];
                }

                GameObject instance = Instantiate(toInit, new Vector3( x, y, 0f), Quaternion.identity) as GameObject;
                instance.GetComponent<Renderer>().enabled = false;
                instance.transform.SetParent(boardHolder);
                ground[x + 1][y + 1] = instance;
            }
        }
        GameObject light1 = Instantiate(light_origin, new Vector3(0, 0, 0f), Quaternion.identity) as GameObject;
        lights.Add(light1);
    }

    Vector3 RandomPosition()
    {
        int index_rnd = Random.Range(0, gridPosition.Count);
        Vector3 pos_rnd = gridPosition[index_rnd];
        gridPosition.RemoveAt(index_rnd);
        return pos_rnd;
    }

    void LayoutObjRandom(GameObject[] tileArr, int m1, int m2)
    {
        int objCount = Random.Range(m1, m2 + 1);

        
        for (int i = 0; i < objCount; i++)
        {
            Vector3 pos = RandomPosition();
            GameObject ins = tileArr[Random.Range(0, tileArr.Length)];
            GameObject instance = Instantiate(ins, pos, Quaternion.identity) as GameObject;
            //print(objCount.ToString());
            //Debug.logger.Log( i );
            if (System.Object.ReferenceEquals(tileArr, light)) // if it's light, then we want to take the adjacent 3by3 area into light
            {
                lights.Add(instance);
                instance.GetComponent<Light>().pos = pos;
                //Debug.logger.Log(i);
            }
            else // else, we want to display or not the object
            {
                if (instance.GetComponent<Animator>() != null)
                    instance.GetComponent<Animator>().enabled = false;
                instance.GetComponent<Renderer>().enabled = false;

                if (System.Object.ReferenceEquals(tileArr, enemyTiles)) {
                    continue;
                    // might need some strategies for dealing with movable enemies, here. we avoid registering lights to enmies because
                    // enemy is moving, registering is not efficient
                }
                List<GameObject> light_tmp = new List<GameObject>();
                for(int j = 0; j < lights.Count; j++)
                {
                    if(GameManager.Distance(instance, lights[j]) < adj)
                    {
                        light_tmp.Add(lights[j]);
                    }
                }
                    
                if (light_tmp.Count > 0)
                {
                    objtoshow.Add(new ObjectToShow(instance, light_tmp.ToArray()));
                }
                else
                {
                    objtoshow.Add(new ObjectToShow(instance));
                }

                // if we're layouting the inside walls, we need to update the cost to it to make it int max
                if (System.Object.ReferenceEquals( tileArr, inwallTiles))
                {
                    Vector2_int int_tmp = new Vector2_int(pos.x, pos.y);
                    //Debug.Log(int_tmp.x + " " + int_tmp.y);
                    if (obstacle_level.ContainsKey(int_tmp))
                        Debug.Log( "Repeat at " + int_tmp.x + " " + int_tmp.y);
                    else
                        obstacle_level.Add(int_tmp, int.MaxValue);
                }


            }
        }
    }

    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;

        for (int x = 1; x < col - 1; x++)
        {
            for (int y = 1; y < row - 1; y++)
            {
                gridPosition.Add(new Vector3(x, y, 0f));
            }
        }
        gridPosition.Remove(new Vector3(1, 1, 0f)); //since the position (1,1) must be owned by a light
    }

    GraphNode[][] GraphSetup()
    {
        GraphNode[][] graphNodes = new GraphNode[col][];
        for (int i = 0; i < col; i++)
        {
            graphNodes[i] = new GraphNode[row];
            for (int j = 0; j < row; j++)
            {
                graphNodes[i][j] = new GraphNode(new Vector2_int( i, j));
            }
        }
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                int[] adj_i = new int[] { i - 1, i, i, i + 1};
                int[] adj_j = new int[] { j, j - 1, j + 1, j};
                for(int k = 0; k < adj_i.Length; k++)
                {
                    if (adj_i[k] >= 0 && (adj_i[k] < col) && adj_j[k] >= 0 && (adj_j[k] < row))
                        graphNodes[i][j].adjacent.Add(graphNodes[adj_i[k]][adj_j[k]]);
                }
            }
        }
        return graphNodes;
    }

    private void RegisterGround()
    {
        for (int x = 0; x < col + 2; x++)
        {
            for (int y = 0; y < row + 2; y++)
            {
                List<GameObject> light_tmp = new List<GameObject>();
                for (int i = 0; i < lights.Count; i++)
                {
                    if (GameManager.Distance(lights[i], ground[x][y]) < adj)
                        light_tmp.Add(lights[i]);
                }
                if (light_tmp.Count > 0)
                    objtoshow.Add(new ObjectToShow(ground[x][y], light_tmp.ToArray()));
            }
        }
    }

    // this is not updating player, it is actually updating the object to show objects after player is updated
    public void UpdatePlayer(GameObject gameobj)
    {
        int x = (int) gameobj.transform.position.x;
        int y = (int) gameobj.transform.position.y;
        Player p = gameobj.GetComponent<Player>();
        if (p == null) return;
        List<Vector2_int> list_tmp = new List<Vector2_int>();
        for (int i = Math.Max(0, x - p.fooddistance); i <= Math.Min(col + 1,x + p.fooddistance); i++)
            for(int j = Math.Max(0, y - p.fooddistance); j <= Math.Min(row + 1, y + p.fooddistance); j++)
            {
                Vector2_int tmp = new Vector2_int(i, j);
                if (objtoshow_dict.ContainsKey(tmp))
                {
                    objtoshow_dict[tmp].game.GetComponent<Renderer>().enabled = true;
                    if(objtoshow_dict[tmp].game.GetComponent<Animator>() != null)
                        objtoshow_dict[tmp].game.GetComponent<Animator>().enabled = true;
                    list_tmp.Add(tmp);
                }
            }
        //Debug.Log("list count" + list_tmp.Count);
        //Debug.Log("objecttoshow count:" + objtoshow_dict.Count);
        foreach (Vector2_int vint in changed_everyupdate)
        {
            if (!list_tmp.Contains(vint))
            {
                objtoshow_dict[vint].SetRenderer(this, new PropertyChangedEventArgs("UpdatingAccordingtoLights"));
            }
        }

        changed_everyupdate = list_tmp;
    }

    private void AddtoDictionary()
    {
        for (int i = 0; i < objtoshow.Count; i++)
        {
            if(objtoshow[i].game.tag == "Soda" || objtoshow[i].game.tag == "Food" || objtoshow[i].game.tag == "Exit")
            {
                objtoshow_dict.Add(new Vector2_int(objtoshow[i].game.transform.position.x, objtoshow[i].game.transform.position.y ), objtoshow[i]);
            }
        }
        //Debug.Log("Food Soda count: " + objtoshow_dict.Count);
    }

    public void SetupScene( int level)
    {
        InitList();
        BoardSetup();
        LayoutObjRandom(light, lightCount.min, lightCount.max);
        RegisterGround();
        LayoutObjRandom(inwallTiles, wallCount.min, wallCount.max);
        LayoutObjRandom(foodTiles, foodCount.min, foodCount.max);
        GameObject[] gameobj = new GameObject[1];
        gameobj[0] = exit;
        LayoutObjRandom(gameobj, 1, 1);
        int enemyCount = level/2;//(int)Math.Log(level, 2f);
        LayoutObjRandom(enemyTiles, enemyCount, enemyCount);
        lights[0].GetComponent<Light>().IsOn = true;
        //Debug.Log("Enemy count: " + enemyCount);
        AddtoDictionary();
    }
    

    // this is the algortithm for returning the shortest path
    public List<GraphNode> Dijkstra(Vector2_int start, Vector2_int end) // return the shortest path
    {
        //Debug.Log("Marker 1");
        if (start.x < 0 || start.x >= col || end.x < 0 || end.x >= col
            || start.y < 0 || start.y >= row || end.y < 0 || end.y >= row) // the position is not inside board
        {
            return null;
        }
        GraphNode[][] graphNodes = GraphSetup();

        Heap graphheap = new Heap();
        graphheap.BuildHeap(graphNodes);

        List<GraphNode> result = null;

        //Debug.Log("Marker 3");
        graphNodes[start.x][start.y].previous = null;
        graphNodes[start.x][start.y].cost = 0;
        
        graphheap.PercolateUp(graphNodes[start.x][start.y].mapping_index);

        //Debug.Log("Marker 4");
        //FileStream fs = new FileStream("Running.log", FileMode.Append);
        //byte[] buffer = Encoding.ASCII.GetBytes("Marker 4 \n");
        //fs.Write(buffer, 0, buffer.Length);
        //fs.Close();

        GraphNode tmp = null;
        while (graphheap.currentSize > 0)
        {
            tmp = graphheap.DeleteMin();
            if (tmp.cost == int.MaxValue || System.Object.ReferenceEquals(tmp, graphNodes[end.x][end.y]) || tmp.cost >= graphNodes[end.x][end.y].cost) // all infinite path to the end or already hit the last 
                break;
            foreach(GraphNode node in tmp.adjacent)
            {
                if(node.mapping_index > 0 && !obstacle_level.ContainsKey(node.pos) && tmp.cost + 1 < node.cost) // it's in the unvisited set (graph heap), and not the wall tile, and really lowers the cost
                {
                    node.cost = tmp.cost + 1;
                    node.previous = tmp;
                    graphheap.PercolateUp(node.mapping_index);
                    //for(int j = 1; j < 5; j++)
                    //{
                    //    Debug.Log(" j th cost is " + graphheap.heap_arr[j].pos.x + " " + graphheap.heap_arr[j].pos.y + " " + graphheap.heap_arr[j].cost + " " + graphheap.heap_arr[j].visited);
                    //}
                }
            }
        }

        //Debug.Log("Marker 5");
        //fs = new FileStream("Running.log", FileMode.Append);
        //buffer = Encoding.ASCII.GetBytes("Marker 5 \n");
        //fs.Write(buffer, 0, buffer.Length);
        if (graphNodes[end.x][end.y].cost != int.MaxValue)
        {
            tmp = graphNodes[end.x][end.y];
            result = new List<GraphNode>();
            while (tmp != null)
            {
                result.Insert( 0, tmp);
                //Debug.Log("Path is " + tmp.cost + tmp.mapping_index + "\n");
                //fs.Write(buffer, 0, buffer.Length);
                tmp = tmp.previous;
            }
        }
        //Debug.Log("Cost is " + graphNodes[end.x][end.y].cost);
        //buffer = Encoding.ASCII.GetBytes("Cost is " + graphNodes[end.x][end.y].cost + "\n");
        //fs.Write(buffer, 0, buffer.Length);
        
        //fs.Close();
        //GraphNodeSettoDefault();
        return result;
    }

}
