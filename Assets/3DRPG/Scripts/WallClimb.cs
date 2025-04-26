using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WallClimb : MonoBehaviour
{
    [Header("壁検出設定")]
    public float wallCheckOffset = 1.0f;
    public float upperCheckOffset = 2.0f;
    public float checkDistance = 1.0f;
    [Header("よじ登り後の移動量")]
    public float climbForwardDistance = 2.0f;
    public float climbUpwardDistance = 2.5f;
    public float climbDuration = 0.5f; // 何秒でよじ登るか

    //Animator anim;
    CharacterController controller;

    //内部ステート
    //bool isGrabbed;
    bool isClimbing = false;
    Vector3 climbStartPos;
   
    Vector3 climbEndPos;
    float climbTimer = 0f;

    void Awake()
    {
        //anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    } 

    //壁の前方・上部の有無チェック
    void CheckWalls(out bool wallInFront, out bool wallAbove)
    {
        Vector3 pos = transform.position;
        wallInFront = Physics.Raycast(pos + Vector3.up * wallCheckOffset, transform.forward, checkDistance);
        wallAbove = Physics.Raycast(pos + Vector3.up * upperCheckOffset, transform.forward, checkDistance);

        Debug.DrawRay(pos + Vector3.up * wallCheckOffset, transform.forward * checkDistance, Color.red);
        Debug.DrawRay(pos + Vector3.up * upperCheckOffset, transform.forward * checkDistance, Color.blue);
    }

    //つかめるか判定（地面にいて、前方に壁、かつ上部に余裕あり）

    public bool CanGrab()
    {
        if(!controller.isGrounded) return false;
        CheckWalls(out bool fwd, out bool up);
        return fwd && !up;
    }

    //掴み開始
    public void StartClimb()
    {
        isClimbing = true;
        climbStartPos = transform.position;
        climbEndPos = climbStartPos + transform.forward * climbForwardDistance + Vector3.up * climbUpwardDistance;
        climbTimer = 0f;
    }

    //前入力でよじ登り開始を試みる
    //moveInput.y > 0.1fなどで前入力を検出

    public void HandleClimb()
    {
        if (!isClimbing) return;

        climbTimer += Time.deltaTime;
        float t = Mathf.Clamp01(climbTimer / climbDuration);
        transform.position = Vector3.Lerp(climbStartPos, climbEndPos, t);
    }

    public void HandClimb()
    {
        if (!isClimbing) return;

        climbTimer += Time.deltaTime;
        float t = Mathf.Clamp01(climbTimer / clumbDuration);
        transform.position = Vector3.Lerp(climbStartPos, climbEndPos, t);

        if (t >= 1f)
        {
            isClimbing = false;
        }
    }

    public bool IsClimbing => isClimbing;
}