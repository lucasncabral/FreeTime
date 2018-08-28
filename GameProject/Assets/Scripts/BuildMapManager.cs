using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildMapManager : MonoBehaviour {
    GameObject currentObject;

    public MapGenerator.Map currentMap;

    [Range(0, 1)]
    public float outlinePercent;

    List<MapGenerator.Coord> allTilesCoords;
    Queue<MapGenerator.Coord> shuffledTileCoords;
    Queue<MapGenerator.Coord> ObstacleTileCoords;
    Transform[,] tileMap;
    public Transform mapFloorPrefab;

    bool[,] obstacleMap;
    public int currentObstacleCount;
    Transform mapHolder;

    Vector3 position;

    public int mapSizex;
    public Vector2 maxMapSize;
    public int mapSizey;
    public float tileSize;

    public Transform navmeshMaskPrefab;
    Transform[] masks;

    Transform lastObjectDropped;

    // Use this for initialization
    void Start () {
        masks = new Transform[4];
        mapSizex = currentMap.mapSize.x * 1;
        mapSizey = currentMap.mapSize.y * 1;
        tileSize = currentMap.tileSize / 1f;

        GenerateBaseMap();
    }

    void GenerateBaseMap()
    {
        tileMap = new Transform[mapSizex, mapSizey];
        
        allTilesCoords = new List<MapGenerator.Coord>();
        for (int x = 0; x < mapSizex; x++)
        {
            for (int y = 0; y < mapSizey; y++)
            {
                allTilesCoords.Add(new MapGenerator.Coord(x, y));
            }
        }

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        

        for (int x = 0; x < mapSizex; x++)
        {
            for (int y = 0; y < mapSizey; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(currentMap.tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1) * tileSize * (0.94f);

                newTile.parent = mapHolder;
                newTile.name = "Tile " + x + y;
                newTile.GetComponent<Tile>().coord = new MapGenerator.Coord(x, y);
                tileMap[x, y] = newTile;
            }
        }

        obstacleMap = new bool[mapSizex, mapSizey];
        
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (mapSizex + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.gameObject.AddComponent<BoxCollider>();
        maskLeft.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        maskLeft.gameObject.AddComponent<Tile>();
        maskLeft.localScale = new Vector3((maxMapSize.x - mapSizex) / 2f, 1, mapSizey) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (mapSizex + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.gameObject.AddComponent<BoxCollider>();
        maskRight.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        maskRight.gameObject.AddComponent<Tile>();
        maskRight.localScale = new Vector3((maxMapSize.x - mapSizex) / 2f, 1, mapSizey) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (mapSizey + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.gameObject.AddComponent<BoxCollider>();
        maskTop.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        maskTop.gameObject.AddComponent<Tile>();
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSizey) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (mapSizey + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.gameObject.AddComponent<BoxCollider>();
        maskBottom.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        maskBottom.gameObject.AddComponent<Tile>();
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSizey) / 2f) * tileSize;
        
        Transform mapFloor = Instantiate(mapFloorPrefab, mapFloorPrefab.transform.position, mapFloorPrefab.transform.rotation) as Transform;
        mapFloor.parent = mapHolder;
        mapFloor.localScale = new Vector3(mapSizex * tileSize, mapSizey * tileSize);

        masks[0] = maskLeft;
        masks[1] = maskRight;
        masks[2] = maskTop;
        masks[3] = maskBottom;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap)
    {
        int currentObstacleCount = 0;
        for (int x = 0; x < mapSizex; x++)
        {
            for (int y = 0; y < mapSizey; y++)
            {
                Tile tile = tileMap[x, y].GetComponent<Tile>();
                if (tile.isOccuped)
                {
                    obstacleMap[x, y] = true;
                    currentObstacleCount++;
                }
                else
                    obstacleMap[x, y] = false;
            }
        }

        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<MapGenerator.Coord> queue = new Queue<MapGenerator.Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;
        while (queue.Count > 0)
        {
            MapGenerator.Coord tile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new MapGenerator.Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        
        int targetAccessibleTileCount = (int)(mapSizex * mapSizey - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSizex / 2f + 0.5f + x, 0, -mapSizey / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / 1.4f + (mapSizex - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / 1.4f + (mapSizey - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    DragTransform dragObject;
    // Update is called once per frame
    void Update () {
        if (currentObject != null)
        {
            Cursor.visible = false;
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Cursor.visible = true;
                Destroy(currentObject);
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            dragObject = currentObject.gameObject.GetComponent<DragTransform>();
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * dragObject.getHeight());

            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Debug.DrawLine(ray.origin, point, Color.red);
                position = point;
            }
            
            currentObject.transform.position = new Vector3(position.x, dragObject.getHeight(), position.z);
        } else
        {

            Cursor.visible = true;
        }
    }


    public void NewObject(GameObject ObjectPrefab, bool isBase, bool isPortable)
    {
        if(currentObject != null)
        {
            Destroy(currentObject);
        }

        currentObject = Instantiate(ObjectPrefab.gameObject, this.transform);
        currentObject.AddComponent<DragTransform>();
        currentObject.GetComponent<DragTransform>().currentMode = DragTransform.Mode.DropObject;
        currentObject.GetComponent<DragTransform>().isBase = isBase;
        currentObject.GetComponent<DragTransform>().isPortable = isPortable;

        currentObject.GetComponent<Rigidbody>().isKinematic = true;

        BoxCollider[] colliders;
        colliders = currentObject.GetComponents<BoxCollider>();

        foreach (BoxCollider box in colliders)
            box.isTrigger = true;

        currentObject.transform.position = new Vector3(0,0,0);
    }

    public void MoveObject(GameObject objectMove)
    {
        if (currentObject == null)
        {
            updateObstaclesOccupation();
            currentObject = objectMove;
            currentObject.GetComponent<DragTransform>().currentMode = DragTransform.Mode.DropObject;
            currentObject.transform.parent = this.transform;
        }
    }

    public void DropObject()
    {
        float startTime = Time.time;
        if (currentObject != null)
        {
                Vector3 obstaclePosition = new Vector3(position.x, dragObject.getHeight(), position.z);
                Transform newObstacle = Instantiate(currentObject.transform, obstaclePosition, Quaternion.identity) as Transform;
                newObstacle.gameObject.layer = 10;
            
                if (newObstacle.GetComponent<DragTransform>() == null)
                    newObstacle.gameObject.AddComponent<DragTransform>();

                newObstacle.parent = mapHolder;
                newObstacle.GetComponent<Rigidbody>().isKinematic = true;
            
                newObstacle.GetComponent<DragTransform>().currentMode = DragTransform.Mode.MoveObject;

                // PROBLEMA --> OS TILES DEMORAM MUITO A RECONHECER A COLISÃO COM O MEU NOVO OBJETO INSTANCIADO
                lastObjectDropped = newObstacle;
            
                if (MapIsFullyAccessible(obstacleMap) && isInsideMap() && isInsideOtherObject(currentObject.transform)) {
                    Destroy(currentObject.gameObject);
                    currentObject = null;
                } else
                {
                    newObstacle.GetComponent<DragTransform>().DestroyObject();
                    Debug.Log("Não pode: Bloqueia posição do mapa e/ou esta fora do mapa e/ou colidindo com outro objeto");
                }
            }
    }

    bool isInsideOtherObject(Transform newObstacle)
    {
        if (newObstacle.gameObject.GetComponent<DragTransform>().isColliding)
        {
            
            return newObstacle.gameObject.GetComponent<DragTransform>().isAvailable();
        }
        return true;
    }

    bool isInsideMap()
    {
        for (int x = 0; x < 4; x++)
        {
            if (masks[x].GetComponent<Tile>().isOccuped)
                return false;
        }
        return true;
    }
    

    public void BackMenu()
    {
        SceneManager.LoadScene("Network");
    }


    void updateObstaclesOccupation()
    {
        for (int x = 0; x < mapSizex; x++)
        {
            for (int y = 0; y < mapSizey; y++)
            {
                Tile tile = tileMap[x, y].GetComponent<Tile>();
                tile.isOccuped = false;
            }
        }

        for(int x = 0; x < 4; x++)
        {
            masks[x].GetComponent<Tile>().isOccuped = false;
        }
    }
}
