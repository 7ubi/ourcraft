using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] public float dayLength;
    [SerializeField] private Gradient directionalColor;
    [SerializeField] private Light sunLight;

    public float CurrentTime { get; set; }

    private void Update()
    {
        CurrentTime += Time.deltaTime * Time.timeScale;
        CurrentTime %= dayLength * 60f;
        
        sunLight.color = directionalColor.Evaluate(CurrentTime / (dayLength * 60f));

        sunLight.transform.rotation = Quaternion.Euler(new Vector3(CurrentTime / (dayLength * 60f) * 360f - 90f, 170f, 0));
    }
}
