using NUnit.Framework.Internal;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;  

public class PlayerController : MonoBehaviour
{
    // Movement variables
    public Rigidbody2D rb;
    public float speed = 5.0f;
    [SerializeField] float jumpTime = 2f;
    [SerializeField] Vector2 aniDirection;
    private float horizontal;
    private float vertical;
    private Vector2 move;
    private Vector2 pos;
    private Vector2 jumpdirection;
    private int JumpDistanceMove;
    private float Jumpdistance;
    private float movecooldown = 0.3f;
    private float inputtreshold = 0.5f;
    private bool ismoving = false;
    private float movecooldowntimer = 0f;
    private Animator animator;
    private bool canJump = true;
    private bool CollisionOccured = false;

    // Powerbar variables
    private Image powerBar;
    private GameObject powerbarGOB;
    [SerializeField] float barChangeSpeed = 50f;
    private float maxBarValue = 100f;
    private float currentBarValue = 0f;
    private bool isCharging = false;
    private bool isChargingUp = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        powerBar = GameObject.Find("PowerBarMask").GetComponent<Image>();
        powerbarGOB = GameObject.Find("Powerbar");
        powerbarGOB.SetActive(false);
    }

    // Powerbar function
    IEnumerator UpdatePowerbar()
    {
        powerbarGOB.SetActive(true);
        if (isChargingUp)
        {
            currentBarValue += barChangeSpeed * Time.deltaTime;
            if (currentBarValue >= maxBarValue)
            {
                isChargingUp = false;
            }
        }
        else if (!isChargingUp)
        {
            currentBarValue -= barChangeSpeed * Time.deltaTime;
            if (currentBarValue <= 0)
            {
                isChargingUp = true;
            }
        }
        float fill = currentBarValue / maxBarValue;
        powerBar.fillAmount = fill;
        isCharging = false;
        Jumpdistance = currentBarValue;
        if (Jumpdistance <= 10)
        {
            JumpDistanceMove = 0;
        }
        else if (Jumpdistance <= 30 && Jumpdistance >= 10)
        {
            JumpDistanceMove = 2;
        }
        else if (Jumpdistance <= 70 && Jumpdistance >= 30)
        {
            JumpDistanceMove = 3;
        }
        if (Jumpdistance <= 100 && Jumpdistance >= 70)
        {
            JumpDistanceMove = 4;
        }

        yield return new WaitForSeconds(0.2f);

        if (!Input.GetButton("Jump"))
        {
            StartCoroutine(TurnPowerbarOff());
        }
        yield return null;
    }

    IEnumerator TurnPowerbarOff()
    {
        StopCoroutine(UpdatePowerbar());
        yield return new WaitForSeconds(0.1f);
        yield return Jumpdistance;
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

        // Movement input
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

        // Animation variables
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

        // Jump input
        if (canJump && Input.GetButton("Jump") && movecooldowntimer <= 0f)
        {
            jumpdirection = aniDirection;
            StartCoroutine(UpdatePowerbar());
            animator.SetBool("charge", true);
            animator.GetCurrentAnimatorStateInfo(0).IsName("Charge");
            isCharging = true;
            ismoving = true;
            if (vertical > 0)
            {
                jumpdirection = Vector2.up;
                aniDirection.y = 1;
                aniDirection.x = 0;
            }
            if (vertical < 0)
            {
                jumpdirection = Vector2.down;
                aniDirection.y = -1;
                aniDirection.x = 0;
            }
            if (horizontal > 0)
            {
                jumpdirection = Vector2.right;
                aniDirection.x = 1;
                aniDirection.y = 0;
            }
            if (horizontal < 0)
            {
                jumpdirection = Vector2.left;
                aniDirection.x = -1;
                aniDirection.y = 0;
            }
        }
        if (isCharging && !Input.GetButton("Jump") && canJump)
        {
            canJump = false;
            animator.SetBool("charge", false);
            animator.GetCurrentAnimatorStateInfo(0).IsName("jumping");
            // Move player
            StartCoroutine(Jump());
        }
    }

    // Jump function
    IEnumerator Jump()
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (jumpdirection * JumpDistanceMove);
        movecooldowntimer = 10f;
        float t = 0f;
        //nog te fixen en checken
        RaycastHit2D hit = Physics2D.Raycast (targetPos,Vector2.zero, 0.1f);
        Debug.DrawRay (transform.position , jumpdirection * JumpDistanceMove, Color.red, 10f);
        if (hit.collider != null)
        {
            Debug.Log("Hit object: " + hit);
            Jumpdistance = 0;
            currentBarValue = 0f;
            canJump = true;
            isCharging = false;
            ismoving = false;
            movecooldowntimer = 0f;

            yield break;
        }
        // tot hier
        while (t < 1f)
        {
            t += Time.deltaTime * jumpTime;
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
            yield return null;
            if (CollisionOccured)
            {
                break;
            }
        }

        SnapPositionToGrid();
        Jumpdistance = 0;
        currentBarValue = 0f;
        canJump = true;
        isCharging = false;
        ismoving = false;
        movecooldowntimer = 0f;
    }

    // Movement function
    IEnumerator Move(Vector2 Direction)
    {
        ismoving = true;
        float t = 0f;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (Direction / 1F);
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
            if (CollisionOccured)
            {
                break;
            }
            yield return null;

        }
        SnapPositionToGrid();
        Jumpdistance = 0;
        movecooldowntimer = 0f;
        canJump = true;
        isCharging = false;
        ismoving = false;
    }
    private void SnapPositionToGrid()
    {
        // Round the current position to the nearest integer
        Vector2 currentPos = transform.position;
        Vector2 snappedPos = new Vector2(
            Mathf.Round(currentPos.x),
            Mathf.Round(currentPos.y)
        );
        rb.position = snappedPos;
        transform.position = snappedPos;    
        CollisionOccured = false;
    }

    private void OnCollisionEnter2D (Collision2D collision)
    {
        CollisionOccured = true;
        //Debug.Log("Collision Occured");
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        // More detailed information
        Debug.Log("Hit object tag: " + collision.gameObject.tag);
        Debug.Log("Hit point: " + collision.contacts[0].point);
        Debug.Log("Hit normal: " + collision.contacts[0].normal);
    }
    void FixedUpdate()
    {
        if (!canJump)
        {
            transform.Find("MoveGroundCollider").gameObject.SetActive(false); 
            transform.Find("MoveJumpCollider").gameObject.SetActive(true);
        }
        else if (canJump)
        {
            transform.Find("MoveGroundCollider").gameObject.SetActive(true);
            transform.Find("MoveJumpCollider").gameObject.SetActive(false);
        }
        
    }
}
