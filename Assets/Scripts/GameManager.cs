using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public List<Food> foods = new List<Food>();
    [SerializeField] Food food_prefab;
    [SerializeField] Line line_prefab;
    [SerializeField] SpiderObj spider_prefab;

    public static GameManager Instance;
    public List<SpiderObj> spiders;
    public List<Line> lines;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        AudioCtrl.Instance.PlayBGM(SFX_tag.bgm);

        for(int i = 0; i<16; i++)
        {
            AddFood();
        }

        SexTime();
    }

    private void AddFood()
    {
        Food food = Instantiate(food_prefab, gameObject.transform);
        Vector2 pos = new Vector2(Random.Range(-30f, 24f), Random.Range(-10f, 10f));
        food.transform.position = pos;

        foods.Add(food);
    }

    public void DesroyFood(Food food)
    {
        if(foods.Contains(food)) foods.Remove(food);
        Destroy(food.gameObject);

        AddFood();
    }

    public async Task HaveSex(SpiderObj A, SpiderObj B)
    {
        if (!A.readyToHaveSex || !B.readyToHaveSex)
            return;

        A.readyToHaveSex = false;
        B.readyToHaveSex = false;

        A.BeginSex();
        B.BeginSex();

        for (int i = 0; i < 3; i++) {
            switch (Random.Range(0, 6))
            {
                case 0:
                    AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.sex0);
                    break;
                case 1:
                    AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.sex1);
                    break;
                case 2:
                    AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.sex2);
                    break;
                case 3:
                    AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.sex3);
                    break;
                case 4:
                    AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.sex4);
                    break;
                case 5:
                    AudioCtrl.Instance.PlaySFXbyTag(SFX_tag.sex5);
                    break;
            }
        }

        foreach (Head posA in A.heads)
        {
            foreach(Head posB in B.heads)
            {
                await CreateLine(posA.gameObject.transform, posB.gameObject.transform);
            }

            foreach (Child posB in B.children)
            {
                await CreateLine(posA.gameObject.transform, posB.gameObject.transform);
            }
        }

        foreach(Child posA in A.children)
        {
            foreach (Head posB in B.heads)
            {
                await CreateLine(posA.gameObject.transform, posB.gameObject.transform);
            }

            foreach (Child posB in B.children)
            {
                await CreateLine(posA.gameObject.transform, posB.gameObject.transform);
            }
        }


        await Task.Delay(10500);

        //new spider
        SpiderObj newSpider = Instantiate(spider_prefab, gameObject.transform);
        newSpider.headCount = Random.Range(0, 2) == 0 ? 2 : 4;
        newSpider.Start();
        newSpider.heads[0].transform.position = Vector3.Lerp(A.heads[0].gameObject.transform.position, B.heads[0].gameObject.transform.position, 0.5f);
        newSpider.SetHeadPos();

        //add food
        for (int i = 0; i < 4; i++)
        {
            AddFood();
        }

        A.EndSex();
        B.EndSex();
    }

    async Task CreateLine(Transform A, Transform B)
    {
        Line line = Instantiate(line_prefab, gameObject.transform);
        line.Init(A, B, 10.5f, 0.25f);
        lines.Add(line);

        await Task.Delay(10);
    }

    async Task SexTime()
    {
        await Task.Delay(58000);

        spiders[2].readyToHaveSex = true;
        spiders[3].readyToHaveSex = true;

        await Task.Delay(16000);
        spiders[0].readyToHaveSex = true;
        spiders[1].readyToHaveSex = true;
    }
}
