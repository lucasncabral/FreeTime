using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDropHandler : MonoBehaviour, IDropHandler{
    int indexContainer;
    GunContainer gunContainer;

    private void Start()
    {
        this.gunContainer = GetComponent<GunContainer>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        RectTransform invPanel = transform as RectTransform;
        this.gunContainer = GetComponent<GunContainer>();

        if (RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition))
        {
            Debug.Log(gunContainer.index);
        }
    }
}
