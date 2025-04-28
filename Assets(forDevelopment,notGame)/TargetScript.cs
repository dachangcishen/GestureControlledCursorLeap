using UnityEngine;

public class TargetScript : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject player;
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.detectCollisions = false;
        player = GameObject.FindWithTag("Player");
    }
    
    void Update()
    {
        if (Mathf.Abs(player.transform.position.x - transform.position.x) + Mathf.Abs(player.transform.position.y - transform.position.y) < 0.15f)
        {
            EventManager.EndTrial(1);
            Destroy(gameObject);
        }
        /*else if (distance > 2.0f)
        {
            EventManager.EndTrial(0);
            Destroy(gameObject);
        }*/
    }
    
    /*
    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.EndTrial(1);
            Destroy(gameObject);
        } 
        else
        {
            EventManager.EndTrial(0);
            Destroy(gameObject);
        }
    }

    private void OnCollisionStay(Collision other) {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.EndTrial(1);
            Destroy(gameObject);
        } 
        else
        {
            EventManager.EndTrial(0);
            Destroy(gameObject);
        }
    }
    */
}
