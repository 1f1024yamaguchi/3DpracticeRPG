using UnityEngine;
using TMPro;
using UnityEngine.Playables;

public class DamageText : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _text;
    private Vector3 _worldPosition;
    private PlayableDirector _director;
    private Camera _mainCam;
    private Vector3 _randomOffset; // ダメージテキストの位置にランダムなオフセットを加えるための変数

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
   



    public void Setup(int damage, Vector3 startPos)
    {
        _mainCam = Camera.main; //カメラへの参照を取得
        
        _text = GetComponent<TextMeshProUGUI>();
        _director = GetComponent<PlayableDirector>();

        _parentCanvas = GetComponentInParent<Canvas>();
        //_mainCam = Camera.main;
       if (_text != null)
       {
            _text.text = damage.ToString();
       }
    
        _worldPosition = startPos;

        Vector2 circlePos = Random.insideUnitCircle * 0.6f; // 半径1.5の円の中からランダムな点を取得
        
        _randomOffset = new Vector3(circlePos.x, 1f, circlePos.y); // Y軸に1.0fのオフセットを加える（テキストがキャラクターの頭上に表示されるように）

        //Timelineを再生する
        if ( _director != null)
        {
            _director.Play();
            Destroy(gameObject, (float)_director.duration);
        }
        else
        {
            Destroy(gameObject, 1f); //Timelineがない場合の保険として1秒後に削除
        }

        //timelineの再生が終わったらDestroyする
        
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_mainCam == null || _parentCanvas == null) return;
        // Timelineのアニメーション（Local Position）を活かすため、ワールド座標をスクリーン座標に変換してUIの位置を更新する
        Vector2 screenPos = _mainCam.WorldToScreenPoint(_worldPosition + _randomOffset);

        // 2. スクリーン座標をCanvas内のローカル座標に変換
        // これにより、Render ModeがCameraでも正しい位置に配置される

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform, 
            screenPos, 
            _parentCanvas.worldCamera, 
            out Vector2 localPos
        ); //canvasに設定されているカメラ(UIカメラ)を使う

        // 3. ローカル座標をUIオブジェクトの位置に設定
        _rectTransform.anchoredPosition = localPos;



        
    }
}
