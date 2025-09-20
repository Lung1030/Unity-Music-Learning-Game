using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Vector2 moveDir = Vector2.zero;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // 抓取 Animator
    }

    public void Move(string dir)
    {
        switch (dir)
        {
            case "Up": moveDir = Vector2.up; break;
            case "Down": moveDir = Vector2.down; break;
            case "Left": moveDir = Vector2.left; break;
            case "Right": moveDir = Vector2.right; break;
        }

        // 設定 Animator 的方向與移動狀態
        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetBool("isMoving", true);
    }

    public void StopMove()
    {
        moveDir = Vector2.zero;
        animator.SetBool("isMoving", false);
    }

    void FixedUpdate()
    {
        rb.velocity = moveDir * moveSpeed;
    }
}
