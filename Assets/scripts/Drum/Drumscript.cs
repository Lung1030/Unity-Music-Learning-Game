using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drumscript : MonoBehaviour
{
    [Header("按鈕專區")]
    public Button crashbutton;
    public Button ridebutton;
    public Button T1button;
    public Button T2button;
    public Button SDbutton;
    public Button BDbutton;
    public Button FTbutton;
    public Button HHbutton;

    [Header("圖層專區")]
    public GameObject crashImage;
    public GameObject rideImage;
    public GameObject T1Image;
    public GameObject T2Image;
    public GameObject SDImage;
    public GameObject FTImage;
    public GameObject HHImage;

    
    // Start is called before the first frame update
    void Start()
    {
        crashbutton.onClick.AddListener(Crash);
        ridebutton.onClick.AddListener(Ride);
        T1button.onClick.AddListener(T1);
        T2button.onClick.AddListener(T2);
        SDbutton.onClick.AddListener(SD);
        BDbutton.onClick.AddListener(BD);
        FTbutton.onClick.AddListener(FT);
        HHbutton.onClick.AddListener(HH);
    }
    
    private void Crash()
    {
        crashImage.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void Ride()
    {
        rideImage.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void T1()
    {
        T1Image.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void T2()
    {
        T2Image.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void SD()
    {
        SDImage.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void FT()
    {
        FTImage.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void HH()
    {
        HHImage.SetActive(true);
        
        StartCoroutine(StartCountdown());
    }
    private void BD()
    {

        
        StartCoroutine(StartCountdown());
    }
    IEnumerator StartCountdown()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Reset();
    }
   
    private void Reset()
    {
        T1Image.SetActive(false);
        T2Image.SetActive(false);
        crashImage.SetActive(false);
        rideImage.SetActive(false);
        SDImage.SetActive(false);
        FTImage.SetActive(false);
        HHImage.SetActive(false);
    }
}
