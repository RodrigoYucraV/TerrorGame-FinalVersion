using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IFuelConsumer
{
    float FuelAmount { get; }
}

public interface IAmmoUser
{
    int CurrentAmmo { get; }
}