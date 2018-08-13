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
                newTile.GetComponent<Tile>().coord = new Coord(x, y);
                tileMap[x, y] = newTile;
                newTile.name = "Tile " + x + y;

                newTile.GetComponent<Resize>().localScaleVec = newTile.transform.localScale;
            }
        }

        // OBSTACLES

        // List<Coord> allOpenCoords = new List<Coord>(allTilesCoords);
        foreach (ObstacleClass obstacle in currentMap.ObstacleTileCoords)
        {            
            Transform newObstacle = Instantiate(obstacle.getPrefab(), obstacle.getPosition(), Quaternion.identity) as Transform;
            newObstacle.parent = mapHolder;
            //newObstacle.transform.rotation = obstacle.getRotation();

            // COR DOS OBSTACULOS

            Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
            Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
            //float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
            //obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
            obstacleRenderer.sharedMaterial = obstacleMaterial;
            newObstacle.GetComponent<Resize>().localScaleVec = newObstacle.transform.localScale;
            
            // --> AQUI NÃO SEI OQ FAZER
            // allOpenCoords.Remove(randomCoord);
        }

        updateObstaclesPosition();
        
        //shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), prng.Next(1, 20)));

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
    
    void updateObstaclesPosition()
    {
        List<Coord> allOpenCoords = new List<Coord>(allTilesCoords);
        int obstacleCount = currentMap.ObstacleTileCoords.Count;
        System.Random prng = new System.Random();

        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Tile tile = tileMap[x, y].GetComponent<Tile>();
                if (tile.isOccuped)
                {
                    allOpenCoords.Remove(tile.coord);
                }
            }
        }
                shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), prng.Next(1, 20)));
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = new Coord();
        updateObstaclesPosition();

        int randomNumber = UnityEngine.Random.Range(1, 10);
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
        public Transform obstaclePrefab3;
        public Transform obstaclePrefab4;
        public Transform obstaclePrefab5;

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
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(-2.81f, 0f, -3.79f), new Vector3(-89.98f,0,0), obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(4.23f, 0f, -3.79f), new Vector3(-89.98f, 0, 0), obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(4.23f, 0f, 4.63f), new Vector3(-89.98f, 0, 0), obstaclePrefab));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(-2.81f, 0f, 4.63f), new Vector3(-89.98f, 0, 0), obstaclePrefab));


            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(-6.53f, 0f, 0.5392f), new Vector3(0, 0, 0), obstaclePrefab2));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(-6.33f, 0f,-0.86f), new Vector3(0, 0, 0), obstaclePrefab2));

            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(-1.37f, 0f, -1.42f), new Vector3(0, 0, 0), obstaclePrefab3));

            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(-6.27f, 0f, -6.32f), new Vector3(0, 0, 0), obstaclePrefab4));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(6.22f, 0f, 0.49f), new Vector3(0, 0, 0), obstaclePrefab4));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(6.22f, 0f, -0.82f), new Vector3(0, 0, 0), obstaclePrefab4));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(6.22f, 0.67f,-0.1f), new Vector3(0, 0, 0), obstaclePrefab4));
            
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(0.67f, 0f, 6.35f), new Vector3(0, 0, 0), obstaclePrefab5));
            ObstacleTileCoords.Add(new ObstacleClass(new Vector3(0.67f, 0f, -6.35f), new Vector3(0, 0, 0), obstaclePrefab5));
        }


    }
}
