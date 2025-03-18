using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;  

public class PlayerController : MonoBehaviour
{
    //movement variables 
    public Rigidbody2D rb;
    public float speed = 5.0f;
    [SerializeField] float jumpTime = 0.5f;
    [SerializeField] Vector2 aniDirection;
    private float horizontal;
    private float vertical;
    private Vector2 move;
    private Vector2 pos;    
    private float movecooldown = 0.3f; 
    private float inputtreshold = 0.5f;
    public bool ismoving = false;
    private float movecooldowntimer = 0f;
    private Animator animator;


    // powerbar variables
    private Image powerBar;
    private GameObject powerbarGOB;
    [SerializeField] float barChangeSpeed = 10f;
    private float maxBarValue = 100f;
    public float currentBarValue = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        powerBar = GameObject.Find("PowerBarMask").GetComponent<Image>();
        powerbarGOB = GameObject.Find("Powerbar");
        powerbarGOB.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        movecooldowntimer -= Time.deltaTime;
        horizontal = Input.GetAxisRaw("Horizontal");
        pos = transform.position;
        vertical = Input.GetAxisRaw("Vertical");
        move = new Vector2(horizontal, vertical);

        if (((Mathf.Abs(horizontal) > inputtreshold)) && !ismoving && movecooldowntimer < 0f)
        {
            Vector2 direction = horizontal > 0 ? Vector2.right : Vector2.left;
            StartCoroutine(Move(direction));
            movecooldowntimer = movecooldown;
            aniDirection = direction;
        }



        else if (((Mathf.Abs(vertical) > inputtreshold)) && !ismoving && movecooldowntimer < 0f)
        {
            Vector2 direction = vertical > 0 ? Vector2.up : Vector2.down;
            StartCoroutine(Move(direction));
            movecooldowntimer = movecooldown;
            aniDirection = direction;
        }

        //animatie variables
        float right = aniDirection.x;
        float up = aniDirection.y;

        animator.SetFloat("right", right);
        animator.SetFloat("up", up);
        if (right < 0f)
        {
            GetComponent<SpriteRenderer>().flipX = true;


        }
        else if (right > 0f)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        //jump test animatie waarde

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("charge", true);
            animator.GetCurrentAnimatorStateInfo(0).IsName("Charge");
            powerbarGOB.SetActive(true);    





        }
        


    }


        IEnumerator Move(Vector2 Direction)
        {
            ismoving = true;
            float t = 0f;
            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + Direction;
            while (t < 1f)
            {
                rb.freezeRotation = true;
                t += Time.deltaTime * speed;
                rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
                yield return null;
            }

            rb.position = targetPos;

            rb.freezeRotation = false;
            ismoving = false;



        }
    //powerbar function

      
    void FixedUpdate()
        {



        }

    }