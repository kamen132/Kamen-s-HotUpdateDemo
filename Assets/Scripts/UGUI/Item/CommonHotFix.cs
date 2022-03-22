using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonHotFix : BaseItem
{
    public TextMeshProUGUI m_Des;
    public Button m_ButtonConfire;
    public Button m_BuutonCanel;
    public void Show(string des,UnityAction confirmActcion,UnityAction CancleAciotn)
    {
        m_Des.text = des;
        AddButtonClickListener(m_ButtonConfire, () =>
        {
            confirmActcion();
            Destroy(gameObject);
        });
        AddButtonClickListener(m_BuutonCanel, () =>
        {
            CancleAciotn();
            Destroy(gameObject);
        });
    }
}
