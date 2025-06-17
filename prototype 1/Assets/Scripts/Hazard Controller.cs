using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HazardController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public List<GameObject> hazards;
    public float spawnInterval = 2f;
    private float timer;
    public float speed = 5f;
    public Vector2 removePosition;
   
    public float destroyDistance = 1f;
    // parameters invullen per hazard
    public Vector2 destroyLocationCar = new Vector2(15f, 0f);
    public Vector2 spawnPositionCar;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            GameObject Hazard = Instantiate(hazards[0], spawnPositionCar, Quaternion.identity, transform);
            Rigidbody2D rb = Hazard.GetComponent<Rigidbody2D>();
            rb.linearVelocity = new Vector2(-speed, 0f); 
            timer = 0f;
        }
        CheckChildrenForDestruction(); 
    }

    void CheckChildrenForDestruction()
    {
        // Loop through all children (backwards to avoid issues when destroying)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            // Check distance to destroy location
            float distance = Vector2.Distance(child.position, destroyLocationCar);

            if (distance <= destroyDistance)
            {
                Debug.Log($"Destroying {child.name} at distance {distance} from destroy point");
                Destroy(child.gameObject);
            }
        }
    }
}
