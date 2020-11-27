using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndChange : MonoBehaviour {

    public CamReader CamReader;

    GameObject up;
    GameObject down;
  
    public Material[] _material;

    /*public float interval = 0.1f;

    private float interval;

    //プロパティー
    public float IntervalTime
    {
        get { return this.interval; }  //取得用
        private set { this.interval = value; } //値入力用
    }
    */

    // Use this for initialization
    void Start () {
        //i = 0;
        up = GameObject.Find("Up_indicator");
        down = GameObject.Find("Down_indicator");
    }

    // Update is called once per frame
    void Update() {
        
    }
    /*
    IEnumerator Blink(int n0, int n1, int n2, GameObject Up, GameObject Down)
    {
        while (true)
        {
            var renderComponent = Up.GetComponent<Renderer>();

            if (n0 > 100 || n1 > 100 || n2 > 100)
            {
                if (n0 > n1 && n0 > n2)
                {
                    var Up.GetComponent<Renderer>();
                }
                else if (n1 > n2)

                {
                    Up.GetComponent<Renderer>();
                }
                else
                {
                    Up.GetComponent<Renderer>();

                }
            }
            else
            {
                up.GetComponent<Renderer>().material = _material[0];
            }
            
            renderComponent.enabled = !renderComponent.enabled;
            yield return new WaitForSeconds(interval);
        }
    }
    */

    public void changeUIndicator(int n0, int n1, int n2)
    {
        if (n0 > 50 || n1 > 50 || n2 > 50)
        {
            if (n0 > n1 && n0 > n2)
            {
                up.GetComponent<Renderer>().material = _material[1];
            }
            else if (n1 > n2)
            {
                up.GetComponent<Renderer>().material = _material[2];
            }
            else
            {
                up.GetComponent<Renderer>().material = _material[3];

            }
        }
        else
        {
            up.GetComponent<Renderer>().material = _material[0];
        }

    }

    public void changeDIndicator(int n0, int n1, int n2)
    {
        if (n0 > 50 || n1 > 50 || n2 > 50)
        {
            if (n0 > n1 && n0 > n2)
            {
                down.GetComponent<Renderer>().material = _material[1];
            }
            else if (n1 > n2)
            {
                down.GetComponent<Renderer>().material = _material[2];
            }
            else
            {
                down.GetComponent<Renderer>().material = _material[3];
            }
        }
        else
        {
            down.GetComponent<Renderer>().material = _material[0];
        }
    }
}
