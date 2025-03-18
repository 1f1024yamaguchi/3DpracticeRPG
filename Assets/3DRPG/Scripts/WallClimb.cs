using UnityEngine;

public class WallClimb : MonoBehaviour
{
    public float wallCheckOffset =1.0f;
    public float upperWallCheckOffset = 2.0f;
    public float wallCheckDistance = 1.0f;

    private bool isForwardWall;
    private bool isUpperWall;
    private bool isGrab;
    private bool isClimb;
    

    private Vector3 climbOldPos;
    private Vector3 climbPos;

    private Animator anim;
    private CharacterController controller;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray wallCheckRay = new Ray(transform.position + Vector3.up * wallCheckOffset, transform.forward);
        Ray upperCheckRay = new Ray(transform.position + Vector3.up * upperWallCheckOffset, transform.forward);

        Vector3 wallCheckStart = transform.position + Vector3.up * wallCheckOffset;
        Vector3 upperCheckStart = transform.position + Vector3.up * upperWallCheckOffset;

        //Rayの方向（前方）
        Vector3 direction = transform.forward; 

        RaycastHit hit;
        if (Physics.Raycast(wallCheckRay, out hit, wallCheckDistance))
        {
            if(hit.collider.CompareTag("Climbable")) // タグが "Climbable" なら登れる壁と判定
            {
                isForwardWall = true;
            }
            else
            {
                isForwardWall = false;
            }
        
        }
        else
        {
            isForwardWall = false;
        }

        if (Physics.Raycast(upperCheckRay, out hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Climbable"))
            {
                isUpperWall = true;
            }
            else
            {
                isUpperWall = false;
            }
        }
        else
        {
            isUpperWall = false;
        }

        isForwardWall = Physics.Raycast(wallCheckRay, wallCheckDistance);
        isUpperWall = Physics.Raycast(upperCheckRay, wallCheckDistance);

        Debug.DrawRay(wallCheckStart, direction * wallCheckDistance, Color.red);
        Debug.DrawRay(upperCheckStart, direction * wallCheckDistance, Color.blue);

        //崖つかまり処理
        if (isForwardWall && !isUpperWall && Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            isGrab = true;
            anim.SetTrigger("ClimbGrab"); //掴みアニメーション開始
        }

        //壁つかまり中の処理
        if (isGrab && !isForwardWall)
        {
            controller.enabled = false; //移動を無効化
            if (Input.GetKey(KeyCode.W))//前進入力でよじ登り開始
            {
                climbOldPos = transform.position;
                climbPos = transform.position + transform.forward * 2.0f + Vector3.up * 2.5f;
                isGrab = false;
                isClimb = true;
                anim.SetTrigger("ClimbUp");
            }
        }

        //よじ登り処理
        if (isClimb)
        {
            
            
            float f = anim.GetFloat("ClimbProgress"); //アニメーションの進行度
            Debug.Log("ClimbProgress: " + f); // デバッグログの追加
            float x = Mathf.Lerp(climbOldPos.x , climbPos.x , Ease(f));
            float z = Mathf.Lerp(climbOldPos.z, climbPos.z, Ease(f));
            float y = Mathf.Lerp(climbOldPos.y, climbPos.y, f);

            transform.position = new Vector3(x,y,z);

            if (f >= 0.8f)
            {
                FinishClimb(); //よじ登り終了
            }
            
        }
    }

    float Ease(float x)
    {
        return x * x * x;
    }

    public void FinishClimb()
    {
        isClimb = false;
        controller.enabled = true; //移動を開始
    }
}
