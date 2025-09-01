using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnAutoPlay;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnMoves.onClick.AddListener(OnClickMoves);
        //btnTimer.onClick.AddListener(OnClickTimer);
        btnAutoPlay.onClick.AddListener(OnClickAutoPlay);
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        //if (btnTimer) btnTimer.onClick.RemoveAllListeners();
        if (btnAutoPlay) btnAutoPlay.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickAutoPlay()
    {
        Debug.Log("Click to AutoPlayButton");
        m_mngr.LoadLevelMoves();
        m_mngr.ToggleAutoPlay();
    }


    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
