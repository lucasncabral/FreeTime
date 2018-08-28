
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

class DragTransform : MonoBehaviour, IPointerClickHandler
{
    public enum Mode {NewObject, MoveObject, DropObject};
    public Mode currentMode;

    public GameObject objectPrefab;
    float height;

    public bool isColliding;

    BuildMapManager mapManager;

    public bool isBase;
    public bool isPortable;
    public DragTransform otherObject;
    private void Start()
    {
        mapManager = FindObjectOfType<BuildMapManager>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left) {
        switch (currentMode)
        {
        case Mode.NewObject:
            mapManager.NewObject(objectPrefab, isBase, isPortable);
            break;
        case Mode.MoveObject:
            mapManager.MoveObject(this.gameObject);
            break;
        case Mode.DropObject:
            mapManager.DropObject();
            break;
        }
        }
    }

    public void DestroyObject()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10) {
            isColliding = true;
        }
    }
    

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10) {
            if (other.bounds.size.y + other.GetComponent<Transform>().position.y > height) {
            otherObject = other.gameObject.GetComponent<DragTransform>();
            height = (other.bounds.size.y);
            isColliding = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10) {
            otherObject = null;
            height = 0;
            isColliding = false;
        }
    }

    public float getHeight()
    {
        if (isColliding)
        {
            if (isAvailable()) {
                return otherObject.GetComponent<Transform>().position.y +  height;
        }
        }
        return 0.01f;
    }

    public bool isAvailable()
    {
        if (otherObject != null)
        {
            return isPortable && otherObject.isBase;
        } else
            return false;
    }
}
