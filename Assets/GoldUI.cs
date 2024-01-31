using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    private TMP_Text textObject;
    // Start is called before the first frame update
    void Start()
    {
        textObject = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        textObject.text = SessionManager.goldCarried.ToString();
    }
}
