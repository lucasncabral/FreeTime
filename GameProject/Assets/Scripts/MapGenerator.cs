using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;
    public Map currentMap;

    public Transform mapFloorPrefab;
    public Transform navmeshMaskPrefab;
    public Transform navMeshFloorPrefab;
    [Range(0, 1)]
    public float outlinePercent;

    List<Coord> allTilesCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> ObstacleTileCoords;


    // enemy random
    Transform[,] tileMap;
    Queue<Coord> shuffledOpenTileCoords;

    // ?
    public Vector2 maxMapSize;
    public float tileSize;
    

    Transform tilePrefab;
    void Start()
    {
        mapIndex = PlayerPrefs.GetInt("missionChoose", 0);
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        // AQUI
        currentMap.UpdateObstacleTileCoords();
        tileSize = currentMap.tileSize;
        tilePrefab = currentMap.tilePrefab;

        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        // ADICIONA NA LISTA TODAS AS COORDENADAS POSSÍVEIS, COM BASE NO TAMANHO DO MAPA
        allTilesCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTilesCoords.Add(new Coord(x, y));
            }
        }
        

        System.Random prng = new System.Random();
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTilesCoords.ToArray(), prng.Next(1, 20)));

        // DELETANDO E CRIANDO GAMEOBJECT
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // GERANDO O TILE
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * currentMap.tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;

                newTile.GetComponent<Resize>().localScaleVec = newTile.transform.localScale;
            }
        }

        // OBSTACLES
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int obstacleCount = currentMap.ObstacleTileCoords.Count;
        int currentObstacleCount = 0;

        List<Coord> allOpenCoords = new List<Coord>(allTilesCoords);
        foreach (ObstacleClass obstacle in currentMap.ObstacleTileCoords)
        {
            //ObstacleClass obstacle = GetObstacleCoord();
            
            Coord randomCoord = obstacle.centerCoord();
            
            foreach(Coord coord in obstacle.getCoord()) {
                obstacleMap[coord.x, coord.y] = true;
                currentObstacleCount++;
            }

            if ((randomCoord != currentMap.mapCenter) && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstacle.getPrefab(), obstaclePosition + (Vector3.up * 2f / 2), Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                //newObstacle.localScale = new Vector3(currentMap.tileSize, 2f, currentMap.tileSize);

                // COR DOS OBSTACULOS

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                //float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                //obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;
                newObstacle.GetComponent<Resize>().localScaleVec = newObstacle.transform.localScale;

                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        // ??
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), prng.Next(1, 20)));

        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * currentMap.tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * currentMap.tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * currentMap.tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * currentMap.tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * currentMap.tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * currentMap.tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * currentMap.tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * currentMap.tileSize;
        
        Transform navMeshFloor = Instantiate(navMeshFloorPrefab, navMeshFloorPrefab.transform.position, navMeshFloorPrefab.transform.rotation) as Transform;
        Transform mapFloor = Instantiate(mapFloorPrefab, mapFloorPrefab.transform.position, mapFloorPrefab.transform.rotation) as Transform;

        navMeshFloor.parent = mapHolder;
        mapFloor.parent = mapHolder;

        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * currentMap.tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * currentMap.tileSize, currentMap.mapSize.y * currentMap.tileSize);

        navMeshFloor.GetComponent<Resize>().localScaleVec = navMeshFloor.transform.localScale;
        mapFloor.GetComponent<Resize>().localScaleVec = mapFloor.transform.localScale;
        

        maskLeft.GetComponent<Resize>().localScaleVec = maskLeft.transform.localScale;
        maskRight.GetComponent<Resize>().localScaleVec = maskRight.transform.localScale;
        maskTop.GetComponent<Resize>().localScaleVec = maskTop.transform.localScale;
        maskBottom.GetComponent<Resize>().localScaleVec = maskBottom.transform.localScale;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    void GetObstacleCoord()
    {
        /**
        ObstacleClass obstacle = currentMap.ObstacleTileCoords.Dequeue();
        //Coord randomCoord = obstacle.getCoord();
        currentMap.ObstacleTileCoords.Enqueue(obstacle);
        return obstacle;
        **/
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = new Coord();
        int randomNumber = UnityEngine.Random.Range(0, 5);
        for (int i = 0; i < randomNumber; i++)
        {
            randomCoord = shuffledOpenTileCoords.Dequeue();
            shuffledOpenTileCoords.Enqueue(randomCoord);
        }
        return tileMap[randomCoord.x, randomCoord.y];
    }

    // OBSTACLES
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * currentMap.tileSize;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
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
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
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

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            this.x = _x;
            this.y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        public Transform tilePrefab;
        public Transform obstaclePrefab;
        public Transform obstaclePrefab2;

        public float tileSize;
        public List<ObstacleClass> ObstacleTileCoords;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }

        public void UpdateObstacleTileCoords()
        {
            ObstacleTileCoords = new List<ObstacleClass>();
            ObstacleTileCoords.Add(new ObstacleClass(0, 3, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(1, 3, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(1, 5, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(2, 8, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(3, 4, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(4, 0, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(4, 2, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(4, 3, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(4, 9, obstaclePrefab2));
            ObstacleTileCoords.Add(new ObstacleClass(5, 9, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(6, 1, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(6, 2, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(7, 2, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(7, 7, obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(9, 6, obstaclePrefab2));
        }


    }
}
