using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<GameObject> foods = new List<GameObject>();
    [SerializeField] GameObject food_prefab;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        AudioCtrl.Instance.PlayBGM(SFX_tag.bgm);

        AddFood();
        AddFood();
        AddFood();
        AddFood();
        AddFood();
        AddFood();
        AddFood();
        AddFood();
        AddFood();
        AddFood();
    }

    private void AddFood()
    {
        GameObject food = Instantiate(food_prefab, gameObject.transform);
        Vector2 pos = new Vector2(Random.Range(-15f, 15f), Random.Range(-5f, 5f));
        food.transform.position = pos;

        foods.Add(food);
    }

    public void DesroyFood(GameObject food)
    {
        if(foods.Contains(food)) foods.Remove(food);
        Destroy(food);

        AddFood();
    }
}
