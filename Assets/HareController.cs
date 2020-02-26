using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HareController : MonoBehaviour
{
    public string trackName;
    public GameObject next;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<AudioManager>().Play(trackName);
        if(next) next.SetActive(true);
        GameObject.Destroy(this.gameObject);
    }
}
