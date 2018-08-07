using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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

    // Use this for initialization
    void Start () {
        GenerateBaseMap();
    }

    void GenerateBaseMap()
    {
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        // ADICIONA NA LISTA TODAS AS COORDENADAS POSSÍVEIS, COM BASE NO TAMANHO DO MAPA
        allTilesCoords = new List<MapGenerator.Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
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
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(currentMap.tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                Destroy(newTile.GetComponent<NetworkTransform>());
                Destroy(newTile.GetComponent<Resize>());
                Destroy(newTile.GetComponent<NetworkIdentity>());
                newTile.gameObject.AddComponent<DragTransform>();
                newTile.gameObject.AddComponent<BoxCollider>();
                newTile.GetComponent<DragTransform>().currentMode = DragTransform.Mode.DropObject;
                newTile.GetComponent<DragTransform>().coord = new MapGenerator.Coord(x, y);
                newTile.localScale = Vector3.one * (1 - outlinePercent) * currentMap.tileSize;
                newTile.parent = mapHolder;
                newTile.name = "Tile " + x + y;
                tileMap[x, y] = newTile;

                newTile.GetComponent<Resize>().localScaleVec = newTile.transform.localScale;
            }
        }


        obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int obstacleCount = currentMap.ObstacleTileCoords.Count;
        currentObstacleCount = 0;
        
        foreach (ObstacleClass obstacle in currentMap.ObstacleTileCoords)
        {
            //ObstacleClass obstacle = GetObstacleCoord();

            MapGenerator.Coord randomCoord = obstacle.centerCoord();

            foreach (MapGenerator.Coord coord in obstacle.getCoord())
            {
                obstacleMap[coord.x, coord.y] = true;
                currentObstacleCount++;
            }

            if ((randomCoord != currentMap.mapCenter) && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstacle.getPrefab(), obstaclePosition + (Vector3.up * 2f / 2), Quaternion.identity) as Transform;
                Destroy(newObstacle.GetComponent<NetworkTransform>());
                Destroy(newObstacle.GetComponent<Resize>());
                Destroy(newObstacle.GetComponent<NetworkIdentity>());
                newObstacle.gameObject.AddComponent<DragTransform>();
                newObstacle.GetComponent<DragTransform>().currentMode = DragTransform.Mode.MoveObject;

                newObstacle.parent = mapHolder;

                // COR DOS OBSTACULOS
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleRenderer.sharedMaterial = obstacleMaterial;
                newObstacle.GetComponent<Resize>().localScaleVec = newObstacle.transform.localScale;
                
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        Transform mapFloor = Instantiate(mapFloorPrefab, mapFloorPrefab.transform.position, mapFloorPrefab.transform.rotation) as Transform;
        
        mapFloor.parent = mapHolder;
        
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * currentMap.tileSize, currentMap.mapSize.y * currentMap.tileSize);
        
        mapFloor.GetComponent<Resize>().localScaleVec = mapFloor.transform.localScale;
    }

    public void UpdatePosition(Transform transform)
    {
        position = transform.position;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
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

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * currentMap.tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / 1.4f + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / 1.4f + (currentMap.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    // Update is called once per frame
    void Update () {
        if (currentObject != null)
        {
            Vector3 temp = Input.mousePosition;
            temp.z = 10f;
            
            //currentObject.transform.position = position;
            currentObject.transform.position = new Vector3(position.x, 1.5f, position.z);
             //   currentObject.transform.position = Camera.main.ScreenToWorldPoint(temp);
            //currentObject.transform.position = new Vector3(currentObject.transform.position.x, currentObject.transform.position.y * 0.5f, currentObject.transform.position.z);
            //Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Cursor.visible = true;
                Destroy(currentObject);
            }
        }
    }

    public void NewObject(Transform ObjectPrefab)
    {
        if(currentObject != null)
        {
            DestroyObject(currentObject);
        }

        currentObject = Instantiate(ObjectPrefab.gameObject, this.transform);
        Destroy(currentObject.GetComponent<BoxCollider>());
        currentObject.transform.position = new Vector3(0,0,0);
    }

    public void MoveObject(GameObject objectMove, MapGenerator.Coord coord)
    {
        if (currentObject == null)
        {
            currentObject = objectMove;
            //objectMove.GetComponent<DragTransform>().currentMode = DragTransform.Mode.NewObject; 
            currentObject.transform.parent = this.transform;
            Destroy(currentObject.GetComponent<BoxCollider>());

            obstacleMap[coord.x, coord.y] = false;
            currentObstacleCount--;
        }
    }

    public void DropObject(MapGenerator.Coord coord)
    {
        if (currentObject != null && !obstacleMap[coord.x, coord.y])
        {
            obstacleMap[coord.x, coord.y] = true;
            currentObstacleCount++;

            if ((coord != currentMap.mapCenter) && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(coord.x, coord.y);
                Transform newObstacle = Instantiate(currentObject.transform, obstaclePosition + (Vector3.up * 2f / 2), Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                Destroy(newObstacle.GetComponent<NetworkTransform>());
                Destroy(newObstacle.GetComponent<Resize>());
                Destroy(newObstacle.GetComponent<NetworkIdentity>());

                if(newObstacle.GetComponent<DragTransform>() == null)
                    newObstacle.gameObject.AddComponent<DragTransform>();

                if (newObstacle.GetComponent<BoxCollider>() == null)
                    newObstacle.gameObject.AddComponent<BoxCollider>();

                newObstacle.GetComponent<DragTransform>().coord = new MapGenerator.Coord(coord.x, coord.y);
                newObstacle.GetComponent<DragTransform>().currentMode = DragTransform.Mode.MoveObject;

                // COR DOS OBSTACULOS
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = coord.y / (float)currentMap.mapSize.y;
                obstacleRenderer.sharedMaterial = obstacleMaterial;
                //newObstacle.GetComponent<Resize>().localScaleVec = newObstacle.transform.localScale;

                currentMap.ObstacleTileCoords.Add(new ObstacleClass(coord.x, coord.y, currentObject.transform));

                Destroy(currentObject);
                currentObject = null;
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

}
