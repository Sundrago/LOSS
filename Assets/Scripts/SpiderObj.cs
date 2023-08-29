using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;

public class SpiderObj : MonoBehaviour
{
    [SerializeField] Head head_prefab;
    [SerializeField] Child child_prefab;

    [SerializeField] float moveSpeedInSec;
    [SerializeField] float moveVelocity;
    [SerializeField, ReadOnly] private float moveVelocityRandom;

    [SerializeField, BoxGroup("Heads")]
    int headCount;
    [BoxGroup("Head")]
    public float headSize, headMass, headDrag;
    [ReadOnly, BoxGroup("Head")]
    public List<Head> heads;

    [SerializeField, BoxGroup("Child")]
    int childCount;
    [SerializeField, BoxGroup("Child")]
    public float childSize, childMass, childDrag;
    [ReadOnly, BoxGroup("Childs")]
    public List<Child> children;

    [SerializeField, BoxGroup("Joints")]
    public float jointDistance, jointDamping;
    [SerializeField, BoxGroup("Joints")]
    public int jointUpdateFrequency;


    private GameObject targetFood;
    private Vector2 vec = Vector2.zero;

    [Button]
    public void Init()
    {
        DestroySpider();

        if (headCount <= 0) headCount = 1;
        Head head = Instantiate(head_prefab, gameObject.transform);
        head.transform.localScale = Vector3.one * headSize;
        head.Init(this);
        head.gameObject.SetActive(true);
        head.GetComponent<SpringJoint2D>().enabled = false;
        heads.Add(head);

        for(int i = 0; i<headCount; i++)
        {
            AddHead();
        }

        SetHeadPos();
    }

    public void DestroySpider()
    {
        for(int i = heads.Count - 1; i >= 0; i--)
        {
            if(heads[i] != null) Destroy(heads[i].gameObject);
            heads.RemoveAt(i);
        }

        for (int i = children.Count - 1; i >= 0; i--)
        {
            if (children[i] != null) Destroy(children[i].gameObject);
            children.RemoveAt(i);
        }
    }

    private void AddHead()
    {
        Head head = Instantiate(head_prefab, gameObject.transform);
        Vector2 pos = heads[0].transform.position;
        pos.x += Random.Range(-0.1f, 0.1f);
        pos.y += Random.Range(-0.1f, 0.1f);
        head.transform.position = pos;
        head.transform.localScale = Vector3.one * headSize;

        head.Init(this);
        head.AddJoint(heads[0].gameObject);
        heads.Add(head);
        head.gameObject.SetActive(true);

    }

    private async Task MoveHead(int idx)
    {
        await Task.Delay(Mathf.RoundToInt(moveSpeedInSec * 1000));

        int nextIdx = idx + 1;
        if (nextIdx >= heads.Count)
        {
            nextIdx = 0;
        }

        heads[idx].gameObject.GetComponent<Rigidbody2D>().DOMove(vec * moveVelocity * moveVelocityRandom, moveSpeedInSec)
            .SetRelative(true)
            .OnComplete(() => {
                MoveHead(nextIdx);
            });

    }

    private void Start()
    {
        moveVelocityRandom = Random.Range(0.5f, 1f);
        
        Init();
        MoveHead(0);
    }

    private void Update()
    {
        if(Time.frameCount % 30 == 0)
        {
            targetFood = FindNearestFood();
            if (targetFood == null) return;

            if(Vector2.Distance(heads[0].transform.position, targetFood.transform.position) < 0.5f)
            {
                AddChild();
                GameManager.Instance.DesroyFood(targetFood);
                return;
            }

            Debug.DrawLine(heads[0].gameObject.transform.position, targetFood.transform.position, Color.red, 0.5f);

            vec = targetFood.transform.position - heads[0].gameObject.transform.position;
            vec.Normalize();
        }
    }

    [Button]
    private void AddChild()
    {
        PlaySFX();
        moveVelocityRandom = Random.Range(0.5f, 1f);
        UpdateWeights();
        AddChildToTheHead(GetHeadWithLeastChildren());
        SetHeadPos();
    }

    private void PlaySFX()
    {
        int rnd = Random.Range(0, 3);

        switch(rnd)
        {
            case 0:
                AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.darkFX1);
                break;
            case 1:
                AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.darkFX2);
                break;
            case 2:
                AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.darkFX3);
                break;
        }
    }

    private void AddChildToTheHead(Head head)
    {
        Child child = Instantiate(child_prefab, gameObject.transform);
        child.Init(this);
        head.AddChild(child);
        children.Add(child);
        child.gameObject.SetActive(true);
    }

    private Head GetHeadWithLeastChildren()
    {
        int min = int.MaxValue;
        Head headWithMinChildren = null;

        for(int i = 1; i<heads.Count; i++)
        {
            if (heads[i].children.Count < min)
            {
                headWithMinChildren = heads[i];
                min = heads[i].children.Count;
            }
        }
        return headWithMinChildren;
    }

    [Button]
    private void UpdateWeights()
    {
        foreach(Head head in heads)
        {
            head.gameObject.transform.localScale = Vector3.one * headSize;
            Rigidbody2D rigidBody = head.GetComponent<Rigidbody2D>();
            rigidBody.mass = headMass;
            rigidBody.drag = headDrag;

            SpringJoint2D springJoint = head.GetComponent<SpringJoint2D>();
            springJoint.autoConfigureDistance = false;
            springJoint.distance = jointDistance;
            springJoint.dampingRatio = jointDamping;
            springJoint.frequency = jointUpdateFrequency;
        }

        foreach (Child child in children)
        {
            child.gameObject.transform.localScale = Vector3.one * childSize;
            Rigidbody2D rigidBody = child.GetComponent<Rigidbody2D>();
            rigidBody.mass = childMass;
            rigidBody.drag = childDrag;

            SpringJoint2D springJoint = child.GetComponent<SpringJoint2D>();
            springJoint.autoConfigureDistance = false;
            springJoint.distance = jointDistance;
            springJoint.dampingRatio = jointDamping;
            springJoint.frequency = jointUpdateFrequency;
        }

        Rigidbody2D rigidBody2 = heads[0].GetComponent<Rigidbody2D>();
        rigidBody2.mass = childMass;
        rigidBody2.drag = childDrag;
    }

    [Button]
    private void SetHeadPos()
    {
        float dist = 0.5f + children.Count * 2f / 20f;

        Vector2 pos = heads[0].gameObject.transform.position;
        pos.x += dist;
        pos.y += dist;
        heads[1].transform.position = pos;

        pos = heads[0].gameObject.transform.position;
        pos.x += dist;
        pos.y -= dist;
        heads[2].transform.position = pos;

        pos = heads[0].gameObject.transform.position;
        pos.x -= dist;
        pos.y += dist;
        heads[3].transform.position = pos;

        pos = heads[0].gameObject.transform.position;
        pos.x -= dist;
        pos.y -= dist;
        heads[4].transform.position = pos;
    }

    private GameObject FindNearestFood()
    {
        List<GameObject> foods = GameManager.Instance.foods;

        float minDist = float.MaxValue;
        GameObject nearFood = null;

        foreach(GameObject food in foods)
        {
            float dist = Vector2.Distance(heads[0].gameObject.transform.position, food.transform.position);
            if(dist<minDist)
            {
                minDist = dist;
                nearFood = food;
            }
        }

        return nearFood;
    }
}
