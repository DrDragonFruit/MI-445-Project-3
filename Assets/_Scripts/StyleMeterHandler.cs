using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StyleMeterHandler : MonoBehaviour
{
    //visual stuff (first time commenting my code #theycallme John Coder)
    [SerializeField] private Sprite Dithered, Clipped, Brushed, Artful, Painterly, WetMario;
    [SerializeField] private Image StyleBar;
    [SerializeField] private float ditheredMinRange, clippedMinRange, brushedMinRange, artfulMinRange, painterlyMinRange, wetmarioMinRange;
    [SerializeField] private float MaxStyle;
    private Image StyleMeterText;

    //numbers and math and timers and score calc stuff
    private float score;
    private float scoreGain = .1f;


    void Start()
    {
        StyleMeterText = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        score += scoreGain * Time.deltaTime;
        Debug.Log(score);
        score = Mathf.Clamp(score, 0f, MaxStyle);
        StyleBar.fillAmount = score;
        UpdateStyleMeter();
    }

    private void UpdateStyleMeter()
    {
        if (score >= ditheredMinRange && score < clippedMinRange)
        {
            StyleMeterText.sprite = Dithered;
        }
        else if (score >= clippedMinRange && score < brushedMinRange)
        {
            StyleMeterText.sprite = Clipped;
        }
        else if (score >= brushedMinRange && score < artfulMinRange)
        {
            StyleMeterText.sprite = Brushed;
        }
        else if (score >= artfulMinRange && score < painterlyMinRange)
        {
            StyleMeterText.sprite = Artful;
        }
        else if (score >= painterlyMinRange && score < wetmarioMinRange)
        {
            StyleMeterText.sprite = Painterly;
        }
        else if (score >= wetmarioMinRange)
        {
            StyleMeterText.sprite = WetMario;
        }
    }
}
