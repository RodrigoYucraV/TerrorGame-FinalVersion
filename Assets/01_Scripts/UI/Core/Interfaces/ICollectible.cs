using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectible
{
    string ItemID { get; }
    Sprite Icon { get; }
    void OnCollected();
}
