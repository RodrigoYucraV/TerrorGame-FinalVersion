using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearUsers : MonoBehaviour
{
    void Start()
    {
    }

    public void ClearAllPlayerPrefs()
    {

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Todos los datos de PlayerPrefs han sido eliminados.");
    }
}
