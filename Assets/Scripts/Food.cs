using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    List<SpiderObj> spiders = new List<SpiderObj>();

    public bool ShouldIGet(SpiderObj spider)
    {
        if (spiders.Count == 0) return true;


        float dist = Vector2.Distance(gameObject.transform.position, spider.heads[0].transform.position);

        foreach(SpiderObj spiderObj in spiders)
        {
            if (spiderObj == spider) continue;

            float dist_2 = Vector2.Distance(gameObject.transform.position, spiderObj.heads[0].transform.position);

            if (dist_2 < dist) return false;
        }

        return true;
    }

    public void RemoveMe(SpiderObj spider)
    {
        if (spiders.Contains(spider))
            spiders.Remove(spider);
    }

    public void AddMe(SpiderObj spider)
    {
        if (!spiders.Contains(spider))
        {
            spiders.Add(spider);
        }
    }
}
