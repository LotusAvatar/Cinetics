using UnityEngine;
using System.Collections;

public enum ButtonType
{
    Calibrate,
    LoadLevel,
    Instructions,
    CloseInstructions,
    ClosePopUp
}


public class ProgressHolder : MonoBehaviour
{
    public ButtonType buttonType;
    public int level = -1;
    public UISprite progress;
}
