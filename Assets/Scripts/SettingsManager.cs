using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{

    public Button saveOptions;

    public GameObject OptionsPanel = null;

    private bool optionsPanelOpened = true;

    public InputField maxJerk;
    public InputField maxAcc;
    public InputField maxDeacc;
    public InputField maxVel;
    
    public InputField maxStepX;
    public InputField maxStepY;
    public InputField maxStepTheta;
    public InputField lookaheadDist;
    
    public InputField driveRadius;
    public InputField driveWidth;
    

    private void Awake()
    {
        OptionsPanel.SetActive(optionsPanelOpened);

        saveOptions.onClick.AddListener(() => saveCallback());
    }

    // Start is called before the first frame update
    void Start()
    {
        maxJerk.SetTextWithoutNotify("10");
        maxAcc.text = "168";
        maxDeacc.text = "-72";
        maxVel.text = "120";

        maxStepX.text = "2";
        maxStepY.text = "0.25";
        maxStepTheta.text = "0.1";
        lookaheadDist.text = "15";

        driveRadius.text = "12.5";
        driveWidth.text = "25";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void saveCallback()
    {
        robotScript rb_Script = GameObject.Find("Robot").GetComponent<robotScript>();

        rb_Script.setTrajectoryConstants(float.Parse(maxJerk.text),
                                         float.Parse(maxAcc.text),
                                         float.Parse(maxDeacc.text),
                                         float.Parse(maxVel.text));
        rb_Script.setPathGenerationConstants(float.Parse(maxStepX.text),
                                             float.Parse(maxStepY.text),
                                             float.Parse(maxStepTheta.text),
                                             float.Parse(lookaheadDist.text));
        rb_Script.setRobotConstants(float.Parse(driveRadius.text),
                                    float.Parse(driveWidth.text));

    }

    public float getInputFieldValue(string inputFieldName)
    {
        return float.Parse(GameObject.Find(inputFieldName).GetComponent<InputField>().text);
    }
}
