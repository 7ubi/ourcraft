using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField] private worldCreation worldCreation;
    
    [Header("Render Distance")]
    [SerializeField] private Slider renderDistSlider;
    [SerializeField] private TMP_Text renderDistText;
    
    private void Start()
    {
        if (PlayerPrefs.GetFloat("RenderDist") == 0)
        {
            PlayerPrefs.SetFloat("RenderDist", renderDistSlider.minValue);
        }

        renderDistSlider.value = PlayerPrefs.GetFloat("RenderDist");
        renderDistText.text = "" + renderDistSlider.value;
        
        worldCreation.RenderDistance = PlayerPrefs.GetFloat("RenderDist");
    }

    public void ChangeRenderDistance()
    {
        PlayerPrefs.SetFloat("RenderDist", renderDistSlider.value);
        worldCreation.RenderDistance = renderDistSlider.value;
        renderDistText.text = "" + renderDistSlider.value;
        worldCreation.GenerateChunck();
    } 
}
