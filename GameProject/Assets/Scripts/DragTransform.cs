
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
    DragTransform otherObject;
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
        if (other.gameObject.layer == 10)
            isColliding = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10) {
            otherObject = other.gameObject.GetComponent<DragTransform>();
            height = (other.bounds.size.y);
            isColliding = true;
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10) {
            otherObject = null;
            isColliding = false;
        }
    }

    public float getHeight()
    {
        if (isColliding && isAvailable())
        {
            return height;
        }
        return 0.01f;
    }

    public bool isAvailable()
    {
        return isPortable && otherObject.isBase;
    }
}
