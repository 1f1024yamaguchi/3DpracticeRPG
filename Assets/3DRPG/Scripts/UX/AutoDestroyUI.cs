using UnityEngine;
using TMPro;

public class AutoDestroyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textComponent;
    [SerializeField] private float _destroyDelay = 3.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, _destroyDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
