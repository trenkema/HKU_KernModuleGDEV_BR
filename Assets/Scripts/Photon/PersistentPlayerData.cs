using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentPlayerData : MonoBehaviour
{
    public static PersistentPlayerData PPD;

    public int hatIndex { private set; get; }
    public int chestIndex { private set; get; }
    public int pantsIndex { private set; get; }
    public int shoesIndex { private set; get; }

    private void Awake()
    {
        if (PPD != null)
        {
            Destroy(PPD);
        }
        else
        {
            PPD = this;
        }

        DontDestroyOnLoad(this);
    }

    public void setHatIndex(int index)
    {
        hatIndex = index;
    }

    public void setChestIndex(int index)
    {
        chestIndex = index;
    }

    public void setPantsIndex(int index)
    {
        pantsIndex = index;
    }

    public void setShoesIndex(int index)
    {
        shoesIndex = index;
    }
}
