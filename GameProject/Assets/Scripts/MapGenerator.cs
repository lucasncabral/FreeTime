using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapGenerator : NetworkBehaviour
{

    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloorPrefab;
    public Transform navmeshMaskPrefab;
    public Transform navMeshFloorPrefab;

    public Vector2 maxMapSize;
    public float obstacleHeight;

    [Range(0,1)]
    public float outlinePercent;

    // mudar tamanho do mapa
    public float tileSize;

    List<Coord> allTilesCoords;
    Queue<Coord> shuffledTileCoords;

    public Map currentMap;

    // enemy random
    Transform[,] tileMap;
    Queue<Coord> shuffledOpenTileCoords;

    public override void OnStartServer()
    {
        mapIndex = PlayerPrefs.GetInt("missionChoose");
        GenerateMap();
    }

    // change map
    public void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap(){
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        
        System.Random prng = new System.Random(currentMap.seed);

        
        allTilesCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTilesCoords.Add(new Coord(x, y));
            }
        }

        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTilesCoords.ToArray(), currentMap.getSeed()));
        
        // GERANDO A BASE DO MAPA
        string holderName = "Generated Map";
        if(transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for(int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;

                newTile.GetComponent<Resize>().localScaleVec = newTile.transform.localScale;
                //AQUI
                NetworkServer.Spawn(newTile.gameObject);
            }
        }

        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        List<Coord> allOpenCoords = new List<Coord>(allTilesCoords);
        for (int i=0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if((randomCoord != currentMap.mapCenter) && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) {
                obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());

                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + (Vector3.up * obstacleHeight / 2), Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3(tileSize, obstacleHeight, tileSize);

                // COR DOS OBSTACULOS
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float) currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;
                newObstacle.GetComponent<Resize>().localScaleVec = newObstacle.transform.localScale;

                NetworkServer.Spawn(newObstacle.gameObject);

                allOpenCoords.Remove(randomCoord);
            } else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.getSeed()));

        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform navMeshFloor = Instantiate(navMeshFloorPrefab, navMeshFloorPrefab.transform.position, navMeshFloorPrefab.transform.rotation) as Transform;
        Transform mapFloor = Instantiate(mapFloorPrefab, mapFloorPrefab.transform.position, mapFloorPrefab.transform.rotation) as Transform;

        navMeshFloor.parent = this.transform;
        mapFloor.parent = this.transform;


        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);


        navMeshFloor.GetComponent<Resize>().localScaleVec = navMeshFloor.transform.localScale;
        mapFloor.GetComponent<Resize>().localScaleVec = mapFloor.transform.localScale;

        NetworkServer.Spawn(navMeshFloor.gameObject);
        NetworkServer.Spawn(mapFloor.gameObject);

        maskLeft.GetComponent<Resize>().localScaleVec = maskLeft.transform.localScale;
        maskRight.GetComponent<Resize>().localScaleVec = maskRight.transform.localScale;
        maskTop.GetComponent<Resize>().localScaleVec = maskTop.transform.localScale;
        maskBottom.GetComponent<Resize>().localScaleVec = maskBottom.transform.localScale;
        
        NetworkServer.Spawn(maskLeft.gameObject);
        NetworkServer.Spawn(maskRight.gameObject);
        NetworkServer.Spawn(maskTop.gameObject);
        NetworkServer.Spawn(maskBottom.gameObject);
    }
    
    // VERIFICA CONECTIVIDADE DO MAPA
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;
        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for(int x = -1; x <= 1; x++)
            {
                for (int y=-1; y <= 1; y++)
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

        int targetAccessibleTileCount = (int) (currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }


    // OBSTACLES
    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }
    
    // random spawner
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

    public Transform GetTileFromPosition(Vector3 position){
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) -1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) -1);
        return tileMap[x, y];

    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
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
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColour;
        public Color backgroundColour;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }

        public int getSeed()
        {
            return this.seed;
            //return UnityEngine.Random.Range(0, 100);
        }
    }

}
