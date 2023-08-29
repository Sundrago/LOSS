using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpringJoint2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Head : MonoBehaviour
{
    [SerializeField] List<SpriteRenderer> bg_imgs;
    private SpiderObj main = null;
    public List<Child> children = new List<Child>();

    public void Init(SpiderObj _main)
    {
        main = _main;
        Rigidbody2D rigidBody = gameObject.GetComponent<Rigidbody2D>();
        rigidBody.mass = main.childMass;
        rigidBody.drag = main.childDrag;
        gameObject.transform.localScale = Vector3.one * main.headSize;
        gameObject.SetActive(true);

        gameObject.GetComponent<SpriteRenderer>().DOFade(Random.Range(0.1f, 0.8f), Random.Range(0.5f, 3f))
            .SetLoops(-1, LoopType.Yoyo);

        foreach (SpriteRenderer bg in bg_imgs)
        {
            bg.DOFade(Random.Range(0.1f, 0.5f), Random.Range(1f, 3f))
            .SetLoops(-1, LoopType.Yoyo);
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

    public void AddChild(Child child)
    {
        if(children.Count == 0)
        {
            child.transform.position = Vector3.Lerp(main.heads[0].transform.position, gameObject.transform.position, 0.5f);
            child.AddJoint(main.heads[0].gameObject);
            AddJoint(child.gameObject);
            children.Add(child);
        } else
        {
            child.transform.position = Vector3.Lerp(children[children.Count-1].transform.position, gameObject.transform.position, 0.5f);
            child.AddJoint(children[children.Count - 1].gameObject);
            AddJoint(child.gameObject);
            children.Add(child);
        }
    }
}
