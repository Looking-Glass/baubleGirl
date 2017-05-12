using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SMVHeader : MonoBehaviour
{
    string configFileName = "config.json";
    public SMVArrayRender smv;

    [Header("- Numpad -")]
    public Text numpadText;
    //"✔"
    [Header("- Sub Menu -")]
    public Text subMenuNameText;
    public Text smvValueText;
    public Slider smvValueSlider;
    public Text infoText1;
    public Text infoText2;
    public Text resolutionText;
    public GameObject flipXButton;
    public GameObject testCButton;
    public GameObject testNumButton;
    delegate float GetSmvValue();
    delegate void SetSmvValue(float x);
    delegate void DefaultSmvValue();
    delegate void RevertSmvValue();
    GetSmvValue getSmvValue;
    SetSmvValue setSmvValue;
    DefaultSmvValue defaultSmvValue;
    RevertSmvValue revertSmvValue;

    //toggle colors to save
    Color flipXColor;
    Color testColor;

    void Start()
    {
        smv = FindObjectOfType<SMVArrayRender>();
        getSmvValue = () => 0;
        setSmvValue = x => { };
        defaultSmvValue = () => { };
        revertSmvValue = () => { };
        numpadText.text = "";
        LoadConfig();
        LoadMenu("Main Menu");
        flipXColor = flipXButton.GetComponent<Image>().color;
        testColor = testCButton.GetComponent<Image>().color;
        smv.config.colorTest = false;
        smv.config.numTest = false;
        SetToggleButtonColors();
        SetResolutionText(smv.config.rtResolution);        
    }

    void Update()
    {
        smvValueText.text = getSmvValue().ToString();
        smvValueSlider.value = getSmvValue();
        //for debuging
        if (Input.GetKeyDown(KeyCode.D))
        {
            string path = Path.Combine(Application.persistentDataPath, configFileName);
            if (File.Exists(path))
            {
                Debug.Log("deleted config file at " + path);
                File.Delete(path);
                LoadConfig();
            }
        }
    }

    public void SaveConfig()
    {
        if (smv == null)
        {
            smv = FindObjectOfType<SMVArrayRender>();
        }
        string path = Path.Combine(Application.persistentDataPath, configFileName);
        string json = JsonUtility.ToJson(smv.config);
        File.WriteAllText(path, json);
    }

    public void LoadConfig()
    {
        if (smv == null)
        {
            smv = FindObjectOfType<SMVArrayRender>();
        }
        string path = Path.Combine(Application.persistentDataPath, configFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("didn't find save file, default values used");
            if (smv.config == null) smv.config = new SMVConfig();
        }
        else
        {
            string json = File.ReadAllText(path);
            smv.config = JsonUtility.FromJson<SMVConfig>(json);
        }
    }

    public void LoadMenu(string menuName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        string parentMenu = menuName;
        int slashIndex = menuName.IndexOf("/");
        if (slashIndex != -1)
        {
            parentMenu = menuName.Substring(0, slashIndex);
        }
        var menu = transform.FindChild(parentMenu);
        if (menu == null)
        {
            Debug.LogWarning("invalid menu name");
        }
        else
        {
            //activate menu and change delegates, mins, maxes, and whole nums on slider.
            menu.gameObject.SetActive(true);
            if (slashIndex != -1)
            {
                string subMenuName = menuName.Substring(slashIndex + 1);
                subMenuNameText.text = subMenuName;
                smvValueSlider.onValueChanged.RemoveAllListeners();
                switch (subMenuName)
                {
                    case "Num Views":
                        SetupSubMenu("numViews");
                        break;
                    case "View Cone":
                        SetupSubMenu("viewCone");
                        break;
                    case "Tilt":
                        SetupSubMenu("tilt");
                        break;
                    case "Pitch":
                        SetupSubMenu("pitch");
                        break;
                    case "Offset X":
                        SetupSubMenu("offsetX");
                        break;
                    case "Offset Y":
                        SetupSubMenu("offsetY");
                        break;
                    case "Header Size":
                        SetupSubMenu("headerSize");
                        break;
                    case "Useless Pixel %":
                        SetupSubMenu("uselessP");
                        break;
                    case "Pitch Offset X":
                        SetupSubMenu("pitchOffsetX");
                        break;
                    case "Blending":
                        SetupSubMenu("blending");
                        break;
                    case "DPI":
                        SetupSubMenu("DPI");
                        break;
                }
            }
            else if (parentMenu == "Info Menu")
            {
                //update info before displaying
                infoText1.text = "";
                infoText2.text = "";
                Type configType = typeof(SMVConfig);
                int index = 0;
                foreach (var fieldInfo in configType.GetFields())
                {
                    string str = "";
                    if (fieldInfo.FieldType == typeof(SMVConfig.configValue))
                    {
                        str = fieldInfo.Name + ": " + ((SMVConfig.configValue)fieldInfo.GetValue(smv.config)).value + "\n";
                    }
                    else if (fieldInfo.FieldType == typeof(int))
                    {
                        str = fieldInfo.Name + ": " + (int)fieldInfo.GetValue(smv.config) + "\n";
                    }
                    if (index < 7)
                    {
                        infoText1.text += str;
                    }
                    else if (index < 14)
                    {
                        infoText2.text += str;
                    }
                    else
                    {
                        Debug.LogWarning("ran out of room for info fields!");
                    }
                    index++;
                }
            }
            else if (parentMenu == "Main Menu")
            {
                //in most if not all cases where the main menu is loaded, it is because the user navigated back from a submenu
                //therefore it is a good place to save the values to the config json file.
                GetComponent<Image>().enabled = true; //enable black background                
                SaveConfig();
            }
            else if (parentMenu == "Hidden Menu")
            {
                GetComponent<Image>().enabled = false; //disable black background
            }
        }
    }

    public void SetupSubMenu(string valName)
    {
        getSmvValue = () => smv.config.GetValue(valName);
        setSmvValue = x => smv.config.SetValue(valName, x);
        defaultSmvValue = () => smv.config.SetValue(valName, smv.config.GetFullConfigValue(valName).defaultValue);
        float revertVal = smv.config.GetValue(valName);
        revertSmvValue = () => smv.config.SetValue(valName, revertVal);
        smvValueSlider.value = getSmvValue();
        smvValueSlider.onValueChanged.AddListener(arg => smv.config.SetValue(valName, smvValueSlider.value));
        smvValueSlider.minValue = smv.config.GetFullConfigValue(valName).min;
        smvValueSlider.maxValue = smv.config.GetFullConfigValue(valName).max;
        smvValueSlider.wholeNumbers = smv.config.GetFullConfigValue(valName).isInt;
    }

    public void Numpad(string button)
    {
        switch (button)
        {
            case "c":
                numpadText.text = "";
                break;
            case "=":
                float num;
                bool success = float.TryParse(numpadText.text, out num);
                if (success)
                {
                    num = Mathf.Clamp(num, smvValueSlider.minValue, smvValueSlider.maxValue);
                    setSmvValue(num);
                }
                numpadText.text = "";
                break;
            case "-=":
                success = float.TryParse(numpadText.text, out num);
                if (success)
                {
                    num = getSmvValue() - num;
                    num = Mathf.Clamp(num, smvValueSlider.minValue, smvValueSlider.maxValue);
                    setSmvValue(num);
                }
                break;
            case "+=":
                success = float.TryParse(numpadText.text, out num);
                if (success)
                {
                    num = getSmvValue() + num;
                    num = Mathf.Clamp(num, smvValueSlider.minValue, smvValueSlider.maxValue);
                    setSmvValue(num);
                }
                break;
            case "-":
                bool negative = numpadText.text.StartsWith("-");
                if (negative)
                {
                    numpadText.text = numpadText.text.Remove(0, 1);
                }
                else
                {
                    numpadText.text = numpadText.text.Insert(0, "-");
                }
                break;
            case ".":
                if (!numpadText.text.Contains("."))
                {
                    numpadText.text += ".";
                }
                break;
            default:
                numpadText.text += button;
                break;
        }
    }

    public void FlipX()
    {
        smv.config.flipX = !smv.config.flipX;
        SetToggleButtonColors();
        SaveConfig();
    }

    public void ColorTest()
    {
        smv.config.colorTest = !smv.config.colorTest;
        smv.config.numTest = false;
        SetToggleButtonColors();
        SaveConfig();
    }

    public void NumberTest()
    {
        smv.config.numTest = !smv.config.numTest;
        smv.config.colorTest = false;
        SetToggleButtonColors();
        SaveConfig();        
    }

    void SetToggleButtonColors()
    {
        flipXButton.GetComponent<Image>().color = smv.config.flipX ? Color.green : flipXColor;
        testCButton.GetComponent<Image>().color = smv.config.colorTest ? Color.green : testColor;
        testNumButton.GetComponent<Image>().color = smv.config.numTest ? Color.green : testColor;
    }

    public void AdvanceResolutionSettings()
    {
        int resIndex = (int)smv.config.rtResolution;
        var resNameArray = Enum.GetNames(typeof(RtResolution));
        resIndex++;
        if (resIndex >= resNameArray.Length) resIndex = 0;
        smv.config.rtResolution = (RtResolution)resIndex;

        //setup the resolution in the smv
        smv.SetupResolution();

        //set the text
        SetResolutionText(smv.config.rtResolution);

        //save config
        SaveConfig();
    }

    void SetResolutionText(RtResolution rtRes)
    {
        switch (rtRes)
        {
            case RtResolution._512x256_32maxViews:
                resolutionText.text =
                "512 x 256\n" +
                "<size=34>(32 views max)</size>";
                break;
            case RtResolution._512x512_16maxViews:
                resolutionText.text =
                "512 x 512\n" +
                "<size=34>(16 views max)</size>";
                break;
            case RtResolution._1024x512_32maxViews:
                resolutionText.text =
                "1024 x 512\n" +
                "<size=34>(32 views max)</size>";
                break;
            case RtResolution._1024x1024_16maxViews:
                resolutionText.text =
                "1024 x 1024\n" +
                "<size=34>(16 views max)</size>";
                break;
            default:
                resolutionText.text =
                "512 x 256\n" +
                "<size=34>(32 views max)</size>";
                break;
        }
    }

    public void DefaultConfigValue()
    {
        defaultSmvValue();
    }

    public void RevertConfigValue()
    {
        revertSmvValue();
    }
}
