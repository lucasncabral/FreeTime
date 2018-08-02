using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleClass : MonoBehaviour {
    List<MapGenerator.Coord> coord;
    Transform prefab;

    public ObstacleClass(int _x, int _y, Transform _prefab)
    {
        coord = new List< MapGenerator.Coord > ();
        coord.Add(new MapGenerator.Coord(_x, _y));
        this.prefab = _prefab;
    }

    public List<MapGenerator.Coord> getCoord()
    {
        return this.coord;
    }

    public MapGenerator.Coord centerCoord()
    {
        return coord.First();
    }

    public Transform getPrefab()
    {
        return this.prefab;
    }
}