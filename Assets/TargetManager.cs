using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
       
	}

    //TODO: docstring
    /// <summary>
    /// 
    /// </summary>
    /// <param name="numSeconds"></param>
    public void KillAndRespawnTarget(int numSeconds)
    {
        StartCoroutine(ActuallyKillAndRespawn(numSeconds));
    }

    private IEnumerator ActuallyKillAndRespawn(int numSeconds)
    {
        Debug.Log("Removing target");
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        yield return new WaitForSeconds(numSeconds);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<SphereCollider>().enabled = true;
    }

}

