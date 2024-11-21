
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private int m_Gem;

    public void AddGem(int gem)
    {
        m_Gem += gem;
        Debug.Log("Gem Count: " + m_Gem);
    }
}
