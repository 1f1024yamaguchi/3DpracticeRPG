using UnityEngine;

public class jump_dust : MonoBehaviour
{
    [SerializeField] private ParticleSystem DustParticles;

    private CharacterController characterController;

    // 前のフレームで地面にいたかどうかを記憶する
    

    private PlayerController playerController;

    private ParticleSystem.EmissionModule emissionModule;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool wasGrounded;

    void Start()
    {
        characterController = GetComponentInParent<CharacterController>();

        
        if (characterController != null)
        {
            wasGrounded = characterController.isGrounded;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (characterController == null || DustParticles == null)
        {
            return;
        }

        bool isGrounded = characterController.isGrounded;
      

        // 【着地した瞬間の判定】
        // 1. 前のフレームは空中にいた (!wasGrounded)
        // 2. 今のフレームは地面にいる (isGrounded)
        if (!wasGrounded && isGrounded )
        {
            //ジャンプから地面についたときに有効に
            DustParticles.Play();
        }


         // 現フレームの接地状態を「前のフレームの状態」として保存する
        // (次のフレームのUpdateで使うため)
        wasGrounded = isGrounded;
            //空中にいるときはパーティクル発生終了
        
    }
}
