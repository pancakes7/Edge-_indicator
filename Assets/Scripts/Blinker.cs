using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour
{

    public float interval = 0.1f;

    //public IndChange indChange;

    void Start()
    {
        //float interval;
        //interval = indChange.IntervalTime;
        StartCoroutine("Blink");
    }

    IEnumerator Blink()
    {
        while (true)
        {
            var renderComponent = GetComponent<Renderer>();
            renderComponent.enabled = !renderComponent.enabled;
            Debug.Log("interval");
            yield return new WaitForSeconds(interval);
        }
    }

}

