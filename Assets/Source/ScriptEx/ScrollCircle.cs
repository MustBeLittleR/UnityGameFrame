using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollCircle : ScrollRect
{
    public float mRadius = .0f;

    protected override void Start()
    {
        base.Start();
        mRadius = (transform as RectTransform).sizeDelta.x * 0.5f;
    }

    public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        movementType = MovementType.Unrestricted;
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnDrag(eventData);
        var contentPostion = this.content.anchoredPosition;
        if (contentPostion.magnitude > mRadius)
        {
            contentPostion = contentPostion.normalized * mRadius;
            SetContentAnchoredPosition(contentPostion);
        }
    }

    public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        movementType = MovementType.Elastic;
        base.OnEndDrag(eventData);
    }
}