using UnityEngine;

public class ChargeJumpEffectManager : MonoBehaviour
{
    [SerializeField] private Re_PlayerController controller;
    [SerializeField] private ParticleSystem chargeParticle;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private AudioSource jumpAudioSource;
    private ParticleSystem.EmissionModule emissionModule;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emissionModule = chargeParticle.emission;
    }

    // Update is called once per frame
    void Update()
    {
        if(controller.IsChargingJump)
        {
            if(chargeParticle.isPlaying == false)
            {
                chargeParticle.Play();
                
            }
            
            float currentscale = controller.ChargeRatio * maxScale;

            chargeParticle.transform.localScale = new Vector3(currentscale, currentscale,currentscale);
            // 0.0 ~ 1.0 の ChargeRatio に maxEmissionRate を掛けることで、0 ~ 50 の間で動的に変化します
            emissionModule.rateOverTime = controller.ChargeRatio * 50f;
        }
        else if (chargeParticle.isPlaying ==true)
        {
            chargeParticle.Stop();
            jumpAudioSource.Play();
        }
    }
}
