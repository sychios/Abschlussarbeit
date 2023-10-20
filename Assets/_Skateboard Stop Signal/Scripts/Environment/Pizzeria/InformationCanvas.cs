using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationCanvas : MonoBehaviour
{
    private Camera _cam;

    [SerializeField] private GameObject lTouchImageGameObject;
    private Image _lTouchImage;
    [SerializeField] private GameObject rTouchImageGameObject;
    private Image _rTouchImage;
    
    [SerializeField] private Sprite lTouchHighRes;
    [SerializeField] private Sprite lTouchMediumRes;
    [SerializeField] private Sprite lTouchLowRes;
    [SerializeField] private Sprite rTouchHighRes;
    [SerializeField] private Sprite rTouchMediumRes;
    [SerializeField] private Sprite rTouchLowRes;

    [SerializeField] private GameObject infoTextLeft;
    [SerializeField] private GameObject infoTextRight;
    
    [SerializeField] private float fadingDistanceZ = 80f; // Distance from which to start fading the canvas
    [SerializeField] private float fadingDistanceX = 80f; // Distance from which to start fading the canvas

    private static TMP_Text infoLeft;
    private static TMP_Text infoRight;
    
    private static string leftInteractionGerman = "1. Stadtteil rotieren\n2. Stadtteil löschen\n3. Interagieren";
    private static string leftMovementGerman = "1. Spieler drehen\n2. Spieler respawn\n3. Teleportieren";
    private static string rightInteractionGerman = "4. Stadtteil rotieren\n5. Stadtteil löschen\n6. Interagieren";
    private static string rightMovementGerman = "4. Spieler drehen\n5. Spieler respawn\n6. Teleportieren";
    
    private static string leftInteractionEnglish = "1. Rotate piece\n2. Delete piece\n3. Interact";
    private static string leftMovementEnglish = "1. Turn player\n2. Respawn player\n3. Teleport";
    private static string rightInteractionEnglish = "4. Rotate piece\n5. Delete peice\n6. Interact";
    private static string rightMovementEnglish = "4. Turn player\n5. Respawn player\n6. Teleport";


    private void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            this.enabled = false;
            return;
        }
        _cam = Camera.main;

        _lTouchImage = lTouchImageGameObject.GetComponent<Image>();
        _rTouchImage = rTouchImageGameObject.GetComponent<Image>();
        
        
        infoLeft = GameObject.Find("InfoLinks").GetComponent<TMP_Text>();
        infoRight = GameObject.Find("InfoRechts").GetComponent<TMP_Text>();
    }

    private void SetInfoToLowRes()
    {
        _lTouchImage.sprite = lTouchLowRes;
        _rTouchImage.sprite = rTouchLowRes;
        infoTextLeft.gameObject.SetActive(false);
        infoTextRight.gameObject.SetActive(false);
        
    }

    private void SetInfoToMedRes()
    {
        _lTouchImage.sprite = lTouchMediumRes;
        _rTouchImage.sprite = rTouchMediumRes;
        infoTextLeft.gameObject.SetActive(false);
        infoTextRight.gameObject.SetActive(false);
        
    }
    private void SetInfoToHighRes(){
        _lTouchImage.sprite = lTouchHighRes;
        _rTouchImage.sprite = rTouchHighRes;
        infoTextLeft.gameObject.SetActive(true);
        infoTextRight.gameObject.SetActive(true);
        
    }
    
    private void Update()
    {
        if (!_cam) return;
        Vector3 viewPos = _cam.WorldToViewportPoint(transform.position);
        if (viewPos.z < fadingDistanceZ || viewPos.x < fadingDistanceZ)
        {
            float fadeOutZ = (viewPos.z - fadingDistanceZ * 1.5f) * -1 / (fadingDistanceZ / 2);
            float fadeOutX = (viewPos.z - fadingDistanceX * 1.5f) * -1 / (fadingDistanceX / 2);
            if (fadeOutZ < 0 || fadeOutX < 0)
            {
                SetInfoToLowRes();
                //gameObject.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            }
            else
            {
                float fade;
                float dif;
                if (fadeOutX > fadeOutZ)
                {
                    dif = fadeOutX - fadeOutZ;
                    dif /= 2;
                    fade = fadeOutZ + dif;
                } else if (fadeOutZ > fadeOutX)
                {
                    dif = fadeOutZ - fadeOutX;
                    dif /= 2;
                    fade = fadeOutX + dif;
                }
                else
                {
                    fade = fadeOutZ;
                }

                if (fade < 0.5f) // low res
                {
                    SetInfoToLowRes();
                } 
                else if(fade < 0.75f) // medium res
                {
                    SetInfoToMedRes();
                } 
                else // high res
                {
                    SetInfoToHighRes();
                }
                
                //gameObject.GetComponent<CanvasRenderer>().SetAlpha(fade);
            }
        }
        else
        {
            SetInfoToHighRes();
            //gameObject.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        }
    }


    public static void SetPointerInfo(bool switched)
    {
        var iSGerman = GameObject.Find("GameManager").GetComponent<GameManager>().Language == GameManager.Languages.Deutsch;
        if (iSGerman)
        {
            infoLeft.text = switched ? leftInteractionGerman : leftMovementGerman;
            infoRight.text = switched ? rightMovementGerman : rightInteractionGerman;
        }
        else
        {
            infoLeft.text = switched ? leftInteractionEnglish : leftMovementEnglish;
            infoRight.text = switched ? rightMovementEnglish : rightInteractionEnglish;
        }
    }
}
