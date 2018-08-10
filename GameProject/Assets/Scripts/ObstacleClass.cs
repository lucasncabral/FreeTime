using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleClass : MonoBehaviour {
    Vector3 position;
    Vector3 rotation;
    Transform prefab;

    public ObstacleClass(Vector3 _position, Vector3 _rotation, Transform _prefab)
    {
        this.position = _position;
        this.rotation = _rotation;
        this.prefab = _prefab;
    }
    
    public Vector3 getPosition()
    {
        return this.position;
    }

    public Vector3 getRotation()
    {
        return this.rotation;
    }

    public Transform getPrefab()
    {
        return this.prefab;
    }
}