using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpringJoint2D))]
public class Child : MonoBehaviour
{
    private SpiderObj main = null;
    [SerializeField] List<SpriteRenderer> bg_imgs;

    public void Init(SpiderObj _main)
    {
        main = _main;
        Rigidbody2D rigidBody = gameObject.GetComponent<Rigidbody2D>();
        rigidBody.mass = main.childMass;
        rigidBody.drag = main.childDrag;
        gameObject.transform.localScale = Vector3.one * main.childSize * Random.Range(0.8f, 1.2f);
        gameObject.SetActive(true);

        gameObject.GetComponent<SpriteRenderer>().DOFade(Random.Range(0.1f, 0.8f), Random.Range(0.3f, 2f))
            .OnComplete(() => {
                gameObject.GetComponent<SpriteRenderer>().DOFade(Random.Range(0.1f, 0.8f), Random.Range(0.3f, 2f));
            });

        foreach (SpriteRenderer bg in bg_imgs)
        {
            bg.DOFade(Random.Range(0.1f, 0.5f), Random.Range(0.3f, 2f))
            .OnComplete(() => {
                bg.DOFade(Random.Range(0f, 0.4f), Random.Range(0.3f, 2f));
            });
        }
    }

    public void AddJoint(GameObject jointTarget)
    {
        SpringJoint2D springJoint = gameObject.GetComponent<SpringJoint2D>();
        springJoint.connectedBody = jointTarget.GetComponent<Rigidbody2D>();
        springJoint.autoConfigureDistance = false;
        springJoint.distance = main.jointDistance;
        springJoint.dampingRatio = main.jointDamping;
        springJoint.frequency = main.jointUpdateFrequency;
    }
}
