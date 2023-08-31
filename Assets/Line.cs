using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    LineRenderer lineRenderer;
    Transform A, B;
    float endTime;

    bool initialized = false;

    public void Init(Transform _A, Transform _B, float duration, float colorDuration)
    {
        lineRenderer = GetComponent<LineRenderer>();
        A = _A;
        B = _B;
        endTime = Time.time + duration;

        Color colorA = new Color(0, 0.9f * Random.Range(0.85f, 1.15f), 0.5f * Random.Range(0.85f, 1.15f), 0.35f * Random.Range(0.85f, 1.15f));
        Color colorB = new Color(0, 0.9f * Random.Range(0.85f, 1.15f), 0.5f * Random.Range(0.85f, 1.15f), 0.05f * Random.Range(0.85f, 1.15f));

        Color2 color2A = new Color2(colorA, colorB);
        Color2 color2B = new Color2(colorB, colorA);

        lineRenderer.DOColor(color2A, color2B, colorDuration)
            .SetEase(Ease.InOutCubic)
            .SetLoops(-1, LoopType.Yoyo);

        gameObject.SetActive(true);
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;
        if (endTime < Time.time)
        {
            Destroy(gameObject);
            return;
        }

        lineRenderer.SetPosition(0, A.position);
        lineRenderer.SetPosition(1, B.position);
    }


}
