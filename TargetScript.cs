using UnityEngine;

public class TargetScript : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }
    
    void Update()
    {
        //rb.AddForce(0f, 0f, -0.005f);
    }

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
}
