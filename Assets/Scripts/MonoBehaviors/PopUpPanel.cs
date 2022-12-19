using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
    }
    public void Show(bool val)
    {
        this.gameObject.SetActive(val);
    }
}
