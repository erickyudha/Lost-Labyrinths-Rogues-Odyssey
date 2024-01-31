using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    public TMP_Text loadingText;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        loadingText.text = "Loading... " + SessionManager.loadingProgress + "%";
    }
}
