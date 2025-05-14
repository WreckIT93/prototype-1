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
    [SerializeField] private float jumpTime = 2f;
    [SerializeField] private Vector2 aniDirection;
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
    [SerializeField] private float barChangeSpeed = 50f;
    private float maxBarValue = 100f;
    private float currentBarValue = 0f;
    private bool isCharging = false;
    private bool isChargingUp = true;

    // Player manager variables
    private Vector2 Startinglocation;
    private bool ResetCheckpoint = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Startinglocation = transform.position;
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
        else
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
        else if (Jumpdistance <= 30)
        {
            JumpDistanceMove = 2;
        }
        else if (Jumpdistance <= 70)
        {
            JumpDistanceMove = 3;
        }
        else if (Jumpdistance <= 100)
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
        if (Mathf.Abs(horizontal) > inputtreshold && !ismoving && movecooldowntimer < 0f)
        {
            Vector2 direction = horizontal > 0 ? Vector2.right : Vector2.left;
            StartCoroutine(Move(direction));
            movecooldowntimer = movecooldown;
            aniDirection = direction;
        }
        else if (Mathf.Abs(vertical) > inputtreshold && !ismoving && movecooldowntimer < 0f)
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
                aniDirection = Vector2.up;
            }
            else if (vertical < 0)
            {
                jumpdirection = Vector2.down;
                aniDirection = Vector2.down;
            }
            else if (horizontal > 0)
            {
                jumpdirection = Vector2.right;
                aniDirection = Vector2.right;
            }
            else if (horizontal < 0)
            {
                jumpdirection = Vector2.left;
                aniDirection = Vector2.left;
            }
        }

        if (isCharging && !Input.GetButton("Jump") && canJump)
        {
            canJump = false;
            animator.SetBool("charge", false);
            animator.GetCurrentAnimatorStateInfo(0).IsName("jumping");
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
        LayerMask layersToExclude = ~LayerMask.GetMask("Ground Hazard");
        RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.zero, 0.1f, layersToExclude);
        Debug.DrawRay(transform.position, jumpdirection * JumpDistanceMove, Color.red, 10f);

        if (hit.collider != null)
        {
            Debug.Log("Hit object: " + hit.collider.name);
            ResetJumpState();
            yield break;
        }

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
        ResetJumpState();
    }

    // Movement function
    IEnumerator Move(Vector2 direction)
    {
        ismoving = true;
        float t = 0f;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (direction / 1f);

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
        ResetJumpState();
    }

    private void SnapPositionToGrid()
    {
        Vector2 currentPos = transform.position;
        Vector2 snappedPos = new Vector2(
            Mathf.Round(currentPos.x),
            Mathf.Round(currentPos.y)
        );

        rb.position = snappedPos;
        transform.position = snappedPos;
        CollisionOccured = false;
    }

    private void ResetJumpState()
    {
        Jumpdistance = 0;
        currentBarValue = 0f;
        canJump = true;
        isCharging = false;
        ismoving = false;
        movecooldowntimer = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionOccured = true;
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag ("Hazard"))
        {
          ResetCheckpoint = true;

        }
            
        
    }

    void FixedUpdate()
    {
        if (!canJump)
        {
            transform.Find("MoveGroundCollider").gameObject.SetActive(false);
            transform.Find("MoveJumpCollider").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("MoveGroundCollider").gameObject.SetActive(true);
            transform.Find("MoveJumpCollider").gameObject.SetActive(false);
        }
        if(ResetCheckpoint == true)
        {
            canJump = false;
            ismoving = true;
            transform.position = Startinglocation;
            ResetCheckpoint = false;
            ResetJumpState();

        }
    }
    
}
