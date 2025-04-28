using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct Setting
{
    public TMP_Text valueDisplay;
    public Slider slider;
    public string title;
    public bool intType;

    public Setting(TMP_Text valueDisplay,  Slider slider, string title, bool intType)
    {
        this.valueDisplay = valueDisplay;
        this.slider = slider;
        this.title = title;
        this.intType = intType;
    }
}

public class SettingsDisplay : MonoBehaviour
{
    [SerializeField] TMP_InputField exportResInput;
    [SerializeField] TMP_Text exportResOutText;

    Generator generator;
    List<Setting> settings;

    private void Awake()
    {
        generator = FindAnyObjectByType<Generator>();

        settings = new List<Setting>();
        foreach (Transform child in transform)
        {
            //Automatically find all the sliders
            TMP_Text valueDisplay = child.GetChild(0).GetComponent<TMP_Text>();
            Slider slider = child.GetComponentInChildren<Slider>();
            if (slider != null)
            settings.Add(new Setting(valueDisplay, slider, child.name, slider.wholeNumbers));
        }
    }

    private void Update()
    {
        //Converts the res input to an int and sets it the the export resolution
        exportResOutText.text = "X " + exportResInput.text;
        if (int.TryParse(exportResInput.text, out int res))
        {
            generator.exportResolution = res;
        }

        foreach (Setting setting in settings)
        {
            setting.valueDisplay.text = (Mathf.Round(setting.slider.value * 10f)/ 10f).ToString();

            //Matches the slider to its relevant setting

            switch (setting.title)
            {
                case "Noise Scale":
                    generator.noiseScale = setting.slider.value;
                    break;
                case "Octaves":
                    generator.octaves = Mathf.RoundToInt(setting.slider.value);
                    break;
                case "Persistance":
                    generator.persistance = setting.slider.value;
                    break;
                case "Lacunarity":
                    generator.lacunarity = setting.slider.value;
                    break;
                case "Preview Resolution":
                    generator.previewResolution = Mathf.RoundToInt(setting.slider.value);
                    break;
                case "Falloff":
                    generator.falloff = setting.slider.value;
                    break;
            }
        }
    }
}
