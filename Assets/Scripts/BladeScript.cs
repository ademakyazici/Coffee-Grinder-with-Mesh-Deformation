using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeScript : MonoBehaviour
{
    [SerializeField]
    private int speed = 100;

    [SerializeField]
    private GameObject coffee;

    [SerializeField]
    ParticleSystem particle;

    void Update()
    {        
        transform.Rotate(new Vector3(0, Time.deltaTime * speed, 0));             
    }

    private void OnCollisionEnter(Collision collision)
    {       
        if (collision.gameObject.tag.Equals("coffee"))
        {
            StartCoroutine(DestroyCube(collision.gameObject));
        }
    }

    public IEnumerator DestroyCube(GameObject obj)
    {
        particle.Play();
        yield return new WaitForSeconds(1.5f);        
        obj.SetActive(false);
    }


}
