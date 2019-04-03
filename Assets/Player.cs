using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isAlive;

    public float speed = 15f;

    public float jumpSpeed = 15f;
    public float springSpeed = 30f;
    public Transform springPrefab;

    public float gravity = 1f;

    public float distance = 0.15f;

    public bool isGround = false;
    private Vector2 moveSpeed = Vector2.zero;

    private int layerMask;

    private Vector2 boxSize;

    private readonly float detectDistance = 5.0f;

    private void Awake()
    {
        this.isAlive = true;
        this.layerMask = ~LayerMask.GetMask("Player", "Trigger");
        this.boxSize = new Vector2(this.transform.localScale.x, this.transform.localScale.y);
        this.respawnPosition = this.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*if (other.gameObject.tag.CompareTo("Trap") == 0)
        {
            Debug.Log("Trigger enter trap");
            Die();
        }
        else if (other.gameObject.tag.CompareTo("Spring") == 0)
        {
            Debug.Log("Trigger enter spring");
            this.moveSpeed.y = springSpeed;
        }*/
        if (other.gameObject.tag.CompareTo("Win") == 0)
        {
            if (winText != null)
                winText.SetActive(true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.CompareTo("Trap") == 0)
        {
            Debug.Log("Collision enter trap");
            Die();
        }
        if (collision.gameObject.tag.CompareTo("Spring") == 0)
        {
            Vector2 contact_normal = collision.contacts[0].normal;
            if (contact_normal.y==1)
            {
                Debug.Log("collision enter spring up"); 
            }
            else if (contact_normal.y == -1)
            {
                Debug.Log("collision enter spring down");
            }
            else if (contact_normal.x == 1)
            {
                Debug.Log("collision enter spring right");
            }
            else if (contact_normal.x == -1)
            {
                Debug.Log("collision enter spring left");
            }
            Debug.Log("collision enter spring");
            this.moveSpeed.x = springSpeed*contact_normal.x;
            this.moveSpeed.y = springSpeed*contact_normal.y;
        }
    }

    private void HorizontalUpdate()
    {
        this.moveSpeed.x = Input.GetAxis("Horizontal") * speed;
        if (this.moveSpeed.x != 0)
        {
            float nextX = this.transform.position.x + moveSpeed.x * Time.deltaTime;

            Vector2 direction = this.moveSpeed.x > 0 ? Vector2.right : Vector2.left;
            RaycastHit2D hit2d = Physics2D.BoxCast(this.transform.position, this.boxSize, 0, direction, detectDistance, this.layerMask);
            if (hit2d.collider != null)
            {
                float nextDis = Mathf.Abs(nextX - hit2d.point.x);
                float minDis = distance + this.boxSize.x * 0.5f;

                // if touch the wall
                if (nextDis <= minDis + 0.02f) // +0.02 防止鬼畜
                {
                    nextX = this.moveSpeed.x > 0 ? (hit2d.point.x - minDis) : (hit2d.point.x + minDis);
                    this.moveSpeed.y = 0;
                    gravity = 0.33f;
                }
                else
                {
                    gravity = 1.0f;
                }
            }
            this.transform.position = new Vector2(nextX, this.transform.position.y);

        }
    }

    private void CheckIsGround()
    {
        RaycastHit2D hit2d = Physics2D.BoxCast(this.transform.position, this.boxSize, 0f, Vector2.down, distance, this.layerMask);
        this.isGround = hit2d.collider != null;
    }

    private void VerticalUpdate()
    {
        if (isGround && Input.GetKeyDown(KeyCode.Space))
        {
                moveSpeed.y = jumpSpeed;
        }
        else
            moveSpeed.y = Mathf.Max(-jumpSpeed, moveSpeed.y - gravity);
           
        if (this.moveSpeed.y != 0)
        {
            float nextY = this.transform.position.y + moveSpeed.y * Time.deltaTime;

            Vector2 direction = this.moveSpeed.y > 0 ? Vector2.up : Vector2.down;
            RaycastHit2D hit2d = Physics2D.BoxCast(this.transform.position, this.boxSize, 0, direction, detectDistance, this.layerMask);
            if (hit2d.collider != null)
            {
                // float curDis = Vector2.Distance(this.transform.position, hit2d.point);
                float nextDis = Mathf.Abs(this.transform.position.y - hit2d.point.y);
                nextDis -= Mathf.Abs(moveSpeed.y) * Time.deltaTime;
                // Debug.Log(nextDis);
                float minDis = distance + this.boxSize.y * 0.5f;
                if (nextDis <= minDis + 0.02f) // +0.02 防止鬼畜
                {
                    nextY = this.moveSpeed.y > 0 ? (hit2d.point.y - minDis) : (hit2d.point.y + minDis);
                    this.moveSpeed.y = 0;

                    Debug.Log(hit2d.point.y);
                    Debug.Log(this.moveSpeed.y);
                    Debug.Log(nextY);

                }
            }        
 
            this.transform.position = new Vector2(this.transform.position.x, nextY);
        }

    }


    // Update is called once per frame
    void Update()
    {
        if (!isAlive) return;
        CheckIsGround();
        HorizontalUpdate();
        VerticalUpdate();
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearSprings();
            Respawn();
        }
    }

    private void Die()
    {
        this.isAlive = false;
        CreateSpring();
        Respawn();
    }

    List<GameObject> springs = new List<GameObject>();
    public Vector2 respawnPosition = new Vector2(-5, 0);

    private void ClearSprings()
    {
        foreach (GameObject gameObject in springs)
            Destroy(gameObject, 0f);
        springs.Clear();
    }
    private void CreateSpring()
    {
        if (springs.Count >= 3)
        {
            ClearSprings();
        }
        else
        {
            Transform spring = GameObject.Instantiate(springPrefab, this.transform.position, this.transform.rotation);
            springs.Add(spring.gameObject);
        }

    }
    private void Respawn()
    {
        if (winText != null)
            winText.SetActive(false);
        Debug.Log("respawn");
        this.transform.position = respawnPosition;
        this.moveSpeed = Vector2.zero;
        this.isAlive = true;
    }

    public GameObject winText;


}
