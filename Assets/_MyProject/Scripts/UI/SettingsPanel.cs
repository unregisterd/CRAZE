using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : BasePanel
{
    private Slider volumeSlider;
    private Button quitBtn;

    private void Awake()
    {
        Transform volumeSliderObj = transform.Find("Volume/ProgressBar/VolumeSlider");
        Transform quitBtnObj = transform.Find("QuitButton");

        if (volumeSliderObj != null)
        {
            volumeSlider = volumeSliderObj.GetComponent<Slider>();
            if (volumeSlider != null)
            {
                volumeSlider.interactable = true;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            }
        }

        if (quitBtnObj != null)
        {
            quitBtn = quitBtnObj.GetComponent<Button>();
            if (quitBtn != null)
            {
                quitBtn.onClick.AddListener(OnQuitClick);
            }
        }

        if (volumeSlider == null)
        {
            Debug.LogWarning("SettingsPanel: VolumeSlider not found.");
        }

        if (quitBtn == null)
        {
            Debug.LogWarning("SettingsPanel: QuitButton not found.");
        }
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    private void OnQuitClick()
    {
        UIManager.Instance.ClosePanel(UIConst.SettingsPanel);
    }
}