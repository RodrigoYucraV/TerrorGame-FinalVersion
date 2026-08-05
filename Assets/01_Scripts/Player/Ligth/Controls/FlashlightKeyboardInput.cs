using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightKeyboardInput : IFlashlightInput
{
    public bool TogglePressed => Input.GetKeyDown(KeyCode.F);
    public bool RechargePressed => Input.GetKeyDown(KeyCode.R);
}
