using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;

public enum SpiderState { idle, findingFood, findingMate, havingSex }

public class SpiderObj : MonoBehaviour
{
    [SerializeField] Head head_prefab;
    [SerializeField] Child child_prefab;
    [SerializeField] public bool readyToHaveSex;

    [SerializeField] float moveSpeedInSec;
    [SerializeField] float moveVelocity;
    [SerializeField, ReadOnly] private float moveVelocityRandom;

    [SerializeField, BoxGroup("Heads")] public int headCount;
    [BoxGroup("Head")] public float headSize, headMass, headDrag;
    [ReadOnly, BoxGroup("Head")] public List<Head> heads;

    [SerializeField, BoxGroup("Child")] int childCount;
    [SerializeField, BoxGroup("Child")] public float childSize, childMass, childDrag;
    [ReadOnly, BoxGroup("Childs")] public List<Child> children;

    [SerializeField, BoxGroup("Joints")] public float jointDistance, jointDamping;
    [SerializeField, BoxGroup("Joints")] public int jointUpdateFrequency;

    private GameObject targetFood, targetMate;
    private Vector2 vec = Vector2.zero;

    private SpiderState state;

    bool initiated = false;

    [Button]
    public void Init(Vector3 pos)
    {
        if (initiated) return;
        DestroySpider();

        gameObject.transform.position = pos;

        if (headCount <= 0) headCount = 1;
        Head head = Instantiate(head_prefab, gameObject.transform);
        head.transform.localScale = Vector3.one * headSize;
        head.Init(this);
        head.gameObject.SetActive(true);
        head.GetComponent<SpringJoint2D>().enabled = false;
        heads.Add(head);

        for (int i = 0; i < headCount; i++)
        {
            AddHead();
        }

        for (int i = 0; i < childCount; i++)
        {
            AddChild();
        }

        moveVelocityRandom = 0.5f;
        if (!GameManager.Instance.spiders.Contains(this))
        {
            GameManager.Instance.spiders.Add(this);
        }

        SetHeadPos();
        MoveHead(0);
        initiated = true;
    }

    private void Start()
    {
        Init(gameObject.transform.position);
    }

    public void DestroySpider()
    {
        for (int i = heads.Count - 1; i >= 0; i--)
        {
            if (heads[i] != null) Destroy(heads[i].gameObject);
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
            .OnComplete(() =>
            {
                MoveHead(nextIdx);
            });

    }

    [Button]
    private void AddChild()
    {
        PlaySFX();
        moveVelocityRandom = Random.Range(0.5f, 1f);
        UpdateWeights();
        AddChildToTheHead(GetHeadWithLeastChildren());
        SetHeadPos();

        if(children.Count > 6)
        {
            if(Random.Range(0f, 1f) < 0.1f)
            {
                readyToHaveSex = true;
            }
        }
    }

    private void PlaySFX()
    {
        int rnd = Random.Range(0, 3);

        switch (rnd)
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

        for (int i = 1; i < heads.Count; i++)
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
        foreach (Head head in heads)
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
    public void SetHeadPos()
    {
        float dist;
        Vector2 pos;
        if (heads.Count == 3)
        {
            dist = 0.5f + children.Count * 2f / 10f;

            pos = heads[0].gameObject.transform.position;
            pos.y += dist;
            heads[1].transform.position = pos;

            pos = heads[0].gameObject.transform.position;
            pos.y -= dist;
            heads[2].transform.position = pos;
            return;
        }

        dist = 0.5f + children.Count * 2f / 18f;

        pos = heads[0].gameObject.transform.position;
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

    private void FindNearestFood()
    {
        if (!initiated) Init(gameObject.transform.position);

        List<Food> foods = GameManager.Instance.foods;

        float minDist = float.MaxValue;
        GameObject nearFood = null;

        foreach (Food food in foods)
        {
            if (!food.ShouldIGet(this)) continue;
            float dist = Vector2.Distance(heads[0].gameObject.transform.position, food.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearFood = food.gameObject;
            }
        }

        if(targetFood != nearFood)
        {
            if(targetFood!=null) targetFood.GetComponent<Food>().RemoveMe(this);
            nearFood.GetComponent<Food>().AddMe(this);

            targetFood = nearFood;
        }
    }

    private GameObject FindMate()
    {
        List<SpiderObj> spiders = GameManager.Instance.spiders;

        foreach (SpiderObj spider in spiders)
        {
            if (spider == this) continue;
            if (spider.readyToHaveSex)
            {
                return spider.gameObject;
            }
        }

        return null;
    }

    private void Update()
    {
        if (Time.frameCount % 40 == 0)
        {
            switch (state)
            {
                case SpiderState.idle:
                    OnIdleState();
                    break;
                case SpiderState.findingFood:
                    OnFindingFood();
                    break;
                case SpiderState.findingMate:
                    OnFindingMate();
                    break;
                case SpiderState.havingSex:
                    vec = Vector2.zero;
                    break;
            }

            OnAnyState();
        }
    }

    private void ChangeState(SpiderState _state)
    {
        if (state == _state) return;

        state = _state;

        switch (state)
        {
            case SpiderState.idle:
                vec = Vector2.zero;
                break;
            case SpiderState.findingFood:
                break;
            case SpiderState.findingMate:
                moveVelocityRandom = 1.75f;
                break;
            case SpiderState.havingSex:
                GameManager.Instance.HaveSex(this, targetMate.GetComponent<SpiderObj>());
                vec = Vector2.zero;
                break;
        }
    }

    private void OnAnyState()
    {
        if (!readyToHaveSex) return;

        GameObject mate = FindMate();
        if (mate != null) ChangeState(SpiderState.findingMate);
    }

    private void OnIdleState()
    {
        FindNearestFood();
        if (targetFood != null) ChangeState(SpiderState.findingFood);
    }

    private void OnFindingFood()
    {
        FindNearestFood();
        if (targetFood == null)
        {
            ChangeState(SpiderState.idle);
        }

        vec = targetFood.transform.position - heads[0].gameObject.transform.position;
        vec.Normalize();

        if (Vector2.Distance(heads[0].transform.position, targetFood.transform.position) < 0.6f)
        {
            AddChild();
            GameManager.Instance.DesroyFood(targetFood.GetComponent<Food>());
            return;
        }
    }

    private void OnFindingMate()
    {
        targetMate = FindMate();
        if (targetMate == null || !readyToHaveSex)
        {
            ChangeState(SpiderState.idle);
            moveVelocityRandom = 1f;
            return;
        }

        vec = targetMate.GetComponent<SpiderObj>().heads[0].transform.position - heads[0].gameObject.transform.position;
        vec.Normalize();

        if (Vector2.Distance(heads[0].transform.position, targetMate.GetComponent<SpiderObj>().heads[0].transform.position) < 1f)
        {
            ChangeState(SpiderState.havingSex);
            return;
        }
    }

    public void BeginSex(float duration = 0.25f)
    {
        ChangeState(SpiderState.havingSex);
        foreach (Child child in children)
        {
            child.GetComponent<Rigidbody2D>().isKinematic = true;
            child.transform.DOShakePosition(0.5f, 0.05f * Random.Range(0.5f, 1.5f), 20, 90, false, false)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void EndSex()
    {
        foreach (Child child in children)
        {
            DOTween.Kill(child.transform);
            child.GetComponent<Rigidbody2D>().isKinematic = false;
        }

        moveVelocityRandom = 0.8f;
        ChangeState(SpiderState.idle);
    }
}