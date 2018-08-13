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
    public int mapSizey;
    public float tileSize;

    int lastCount = 0;
    // Use this for initialization
    void Start () {
        mapSizex = currentMap.mapSize.x * 1;
        mapSizey = currentMap.mapSize.y * 1;
        tileSize = currentMap.tileSize / 1f;
        GenerateBaseMap();
    }

    void GenerateBaseMap()
    {
        tileMap = new Transform[mapSizex, mapSizey];

        // ADICIONA NA LISTA TODAS AS COORDENADAS POSSÍVEIS, COM BASE NO TAMANHO DO MAPA
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

        // GERANDO O TILE
        for (int x = 0; x < mapSizex; x++)
        {
            for (int y = 0; y < mapSizey; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(currentMap.tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                Destroy(newTile.GetComponent<Resize>());
                newTile.gameObject.AddComponent<DragTransform>();
                newTile.GetComponent<BoxCollider>().size = new Vector3(1f, 1f, 0.2f);
                newTile.gameObject.AddComponent<BoxCollider>();
                newTile.GetComponent<DragTransform>().currentMode = DragTransform.Mode.DropObject;
                newTile.GetComponent<DragTransform>().coord = new MapGenerator.Coord(x, y);
                newTile.localScale = Vector3.one * (1) * tileSize * (0.94f);
                newTile.parent = mapHolder;
                newTile.name = "Tile " + x + y;
                newTile.GetComponent<Tile>().coord = new MapGenerator.Coord(x, y);
                tileMap[x, y] = newTile;

                newTile.GetComponent<Resize>().localScaleVec = newTile.transform.localScale;
            }
        }

        obstacleMap = new bool[mapSizex, mapSizey];

        Transform mapFloor = Instantiate(mapFloorPrefab, mapFloorPrefab.transform.position, mapFloorPrefab.transform.rotation) as Transform;
        
        mapFloor.parent = mapHolder;
        
        mapFloor.localScale = new Vector3(mapSizex * tileSize, mapSizey * tileSize);
        
        mapFloor.GetComponent<Resize>().localScaleVec = mapFloor.transform.localScale;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap)
    {
        int currentObstacleCount = 0;
        
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
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
        Debug.Log(targetAccessibleTileCount + " - " + accessibleTileCount);
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

    // Update is called once per frame
    void Update () {
        if (currentObject != null)
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Cursor.visible = true;
                Destroy(currentObject);
            }


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.up * 0.2f);
            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Debug.DrawLine(ray.origin, point, Color.red);
                position = point;
            }


            Vector3 temp = Input.mousePosition;
            temp.z = 10f;
            
            currentObject.transform.position = new Vector3(position.x, 0.2f, position.z);
        }
    }

    public void NewObject(Transform ObjectPrefab)
    {
        if(currentObject != null)
        {
            Destroy(currentObject);
        }

        currentObject = Instantiate(ObjectPrefab.gameObject, this.transform);

        BoxCollider[] colliders;
        colliders = currentObject.GetComponents<BoxCollider>();

        foreach (BoxCollider box in colliders)
            box.enabled = false;

        currentObject.transform.position = new Vector3(0,0,0);
    }

    public void MoveObject(GameObject objectMove, MapGenerator.Coord coord)
    {
        if (currentObject == null)
        {
            updateObstaclesOccupation();
            currentObject = objectMove;
            //objectMove.GetComponent<DragTransform>().currentMode = DragTransform.Mode.NewObject; 
            currentObject.transform.parent = this.transform;
            BoxCollider[] colliders;
            colliders = currentObject.GetComponents<BoxCollider>();

            foreach (BoxCollider box in colliders)
                box.enabled = false;
        }
    }

    public void DropObject(MapGenerator.Coord coord)
    {
        float startTime = Time.time;
        if (currentObject != null && !obstacleMap[coord.x, coord.y])
        {
            if ((coord != currentMap.mapCenter))
            {
                Vector3 obstaclePosition = new Vector3(position.x, 0, position.z);
                Transform newObstacle = Instantiate(currentObject.transform, obstaclePosition, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.GetComponent<Rigidbody>().isKinematic = true;
                Destroy(newObstacle.GetComponent<Resize>());

                BoxCollider[] colliders;
                colliders = newObstacle.GetComponents<BoxCollider>();

                foreach (BoxCollider box in colliders)
                    box.enabled = true;

                if (newObstacle.GetComponent<DragTransform>() == null)
                    newObstacle.gameObject.AddComponent<DragTransform>();

                newObstacle.GetComponent<DragTransform>().coord = new MapGenerator.Coord(coord.x, coord.y);
                newObstacle.GetComponent<DragTransform>().currentMode = DragTransform.Mode.MoveObject;
                
                // TODO AQUI...
                if (MapIsFullyAccessible(obstacleMap)) {
                    Destroy(currentObject);
                    currentObject = null;
                } else
                {
                    Debug.Log("Não pode");
                }
            }
            else
            {
                obstacleMap[coord.x, coord.y] = false;
                currentObstacleCount--;
            }
        }
    }

    // Back
    public void BackMenu()
    {
        SceneManager.LoadScene("Network");
    }


    void updateObstaclesOccupation()
    {
        System.Random prng = new System.Random();

        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Tile tile = tileMap[x, y].GetComponent<Tile>();
                tile.isOccuped = false;
            }
        }
        
    }
}
