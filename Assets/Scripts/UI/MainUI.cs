using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public static MainUI instance;

    [Header("----- Main -----")]
    public GameObject       BG;         // 무한 이동 배경
    public List<GameObject> buttonList; // 메인 버튼
    
    [Header("----- Fade -----")]
    public  Image fadeImage;
    public  float fadeSpeed;
    private bool  fadeState;

    [Header("----- PowerUp -----")]
    public GameObject detailsPanel;
    public Image      detailsIcon;
    public Button     buyButton;
    public List<Text> detailsText;

    [HideInInspector] public PowerUp currentPowerUp;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (var buttonLists in buttonList)
            buttonLists.gameObject.SetActive(false);
        buttonList[0].SetActive(true);
        FadeOut();
    }

    private void Update()
    {
        // 페이드 보이기
        if(fadeState)
        {
            if (fadeImage.color.a < 1)
                fadeImage.color += new Color(0f,0f,0f,fadeSpeed * Time.unscaledDeltaTime);
                
        }
        // 페이드 없애기
        else if(!fadeState)
        {
            if (fadeImage.color.a > 0)
                fadeImage.color -= new Color(0f,0f,0f,fadeSpeed * Time.unscaledDeltaTime);
            else if(fadeImage.color.a <= 0 && fadeImage.gameObject.activeSelf)
                fadeImage.gameObject.SetActive(false);
        }
    }
    
    public void FadeIn()
    {
        fadeState       = true;
        fadeImage.color = new Color(0f,0f,0f,0);
        fadeImage.gameObject.SetActive(true);
    }
    
    public void FadeOut()
    {
        fadeState       = false;
        fadeImage.color = new Color(0f,0f,0f,1);
        fadeImage.gameObject.SetActive(true);
    }

    public void AllMainUI_Invisible()
    {
        BG.SetActive(false);
    
        foreach (var buttonLists in buttonList)
            buttonLists.gameObject.SetActive(false);
    }

    public void ReturnMain()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        foreach (var buttonLists in buttonList)
            buttonLists.gameObject.SetActive(false);
        buttonList[0].SetActive(true);
    }

    public void ReturnMainButton()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        foreach (var buttonLists in buttonList)
            buttonLists.SetActive(false);
        buttonList[0].SetActive(true);
    }

    public void NormalGameButton()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        foreach (var buttonLists in buttonList)
            buttonLists.SetActive(false);
        buttonList[1].SetActive(true);

        GameManager.instance.isNormalGameMode = true;
    }
    public void AI_GameButton()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        foreach (var buttonLists in buttonList)
            buttonLists.SetActive(false);
        buttonList[2].SetActive(true);
        
        GameManager.instance.isNormalGameMode = false;
    }
    
    public void PowerUpButton()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        detailsPanel.SetActive(false);  // 디테일 패널 끄기
        buyButton.interactable = false; // 구매 버튼 비활성화
    
        foreach (var buttonLists in buttonList)
            buttonLists.SetActive(false);
        buttonList[3].SetActive(true);
    }
    

    // 활성화 되어 있을 때(= 돈이 충분할 때), 클릭 가능
    public void Buy()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        PlayerPrefs.SetInt("GoldData",PlayerPrefs.GetInt("GoldData") - currentPowerUp.needGold[currentPowerUp.level]);
        
        AchieveManager.instance.dataGoldText.text = PlayerPrefs.GetInt("GoldData").ToString();
        
        PlayerPrefs.SetInt("PowerUp" + currentPowerUp.id + "Level",currentPowerUp.level + 1);
        currentPowerUp.level = PlayerPrefs.GetInt("PowerUp" + currentPowerUp.id + "Level");
        for (int i = 0; i < currentPowerUp.level; i++)
            currentPowerUp.ranks[i].sprite = currentPowerUp.rankOnImage;
        
        currentPowerUp.OnClick();
    }
    
    public void Quit()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    
        Application.Quit();
    }   
}
