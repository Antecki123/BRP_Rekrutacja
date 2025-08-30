using System.Collections.Generic;
using UnityEngine;

public class AnchorController : MonoBehaviour
{
    public List<AnchorPosition> Anchors;
    private void Awake()
    {
        SetupObjectsPosition();
    }

    private void SetupObjectsPosition()
    {
        if (Anchors == null)
            return;

        for (int i = 0; i < Anchors.Count; i++)
        {
            Anchors[i].SetObjectPosition();
            Destroy(Anchors[i].Anchor.gameObject);
        }
    }
}

[System.Serializable]
public class AnchorPosition
{
    public RectTransform Anchor;
    public GameObject AnchoredObject;
    public bool IsActiveAtStart = false;

    public void SetObjectPosition()
    {
        AnchoredObject.SetActive(IsActiveAtStart);

        AnchoredObject.transform.position = Anchor.position;
    }
}