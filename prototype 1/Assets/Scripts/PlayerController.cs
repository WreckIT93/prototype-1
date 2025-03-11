using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //variables 
    public Rigidbody2D rb;
    public float speed = 5.0f;
    public float jumpTime = 0.5f;
    private float horizontal;
    private float vertical;
    private Vector2 move;
    private Vector2 pos;    
    private float movecooldown = 0.5f; 
    private float inputtreshold = 0.5f;
    public bool ismoving = false;  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     rb = GetComponent<Rigidbody2D>();   


    }

    // Update is called once per frame
    void Update()
    {
        movecooldown -= Time.deltaTime;
        horizontal = Input.GetAxisRaw("Horizontal");
        pos = transform.position;
        vertical = Input.GetAxisRaw("Vertical");
        move = new Vector2(horizontal,vertical);
          if (((Mathf.Abs( horizontal) > inputtreshold)) && !ismoving && movecooldown< 0f )
        {
            
        }
        else if (vertical > inputtreshold && !ismoving && movecooldown < 0f)
        {
           
        }
        else
        {
            
        }

    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * speed * Time.deltaTime);
        // onderstaand gebruiken voor verplaatsen van de speler
        //rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, t));
    }
}
