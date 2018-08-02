
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

class DragTransform : MonoBehaviour, IPointerClickHandler
{
    public enum Mode {NewObject, MoveObject, DropObject};
    public Mode currentMode;
    public MapGenerator.Coord coord;
    public Transform objectPrefab;

    BuildMapManager mapManager;
    private void Start()
    {
        mapManager = FindObjectOfType<BuildMapManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (currentMode)
        {
            case Mode.NewObject:
                mapManager.NewObject(objectPrefab);
                break;
            case Mode.MoveObject:
                mapManager.MoveObject(this.gameObject, coord);
                break;
            case Mode.DropObject:
                mapManager.DropObject(coord);
                break;
        }
    }
}