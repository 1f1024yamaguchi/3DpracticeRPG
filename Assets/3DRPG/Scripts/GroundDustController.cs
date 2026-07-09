using UnityEngine;

public class GroundDustController : MonoBehaviour
{
    [SerializeField] private ParticleSystem DustParticles;

    private CharacterController characterController;

    private Re_PlayerController _playerController;

    private ParticleSystem.EmissionModule emissionModule;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponentInParent<CharacterController>();

        _playerController = GetComponent<Re_PlayerController>();
        if (DustParticles != null)
        {
            emissionModule = DustParticles.emission;

            emissionModule.enabled = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (characterController == null)
        {
            return;
        }

        bool isGrounded = characterController.isGrounded;
        bool isRunning = _playerController.isRunning;

        if (isGrounded && isRunning)
        {
            //地面にいる＋走っているときemmisionを有効に。
            emissionModule.enabled = true;
        }
        else
        {
            emissionModule.enabled = false;
            //空中にいるときはパーティクル発生終了
        }
    }
}
