using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpScreenBlocker : MonoBehaviour
{
    [SerializeField] private Image ScreenBlocker;
    private List<PopUpView> _activePopUps;
    private bool _initDone;

    private void Awake()
    {
        if (_initDone) return;
        _activePopUps = new List<PopUpView>();
        ScreenBlocker.enabled = false;
    }

    public void InitBlocker()
    {
        _initDone = true;
        ScreenBlocker.enabled = false;
        _activePopUps = new List<PopUpView>();
        gameObject.SetActive(true);
    }


    public void AddPopUpView(PopUpView popUp)
    {
        if (!_activePopUps.Contains(popUp))
        {
            _activePopUps.Add(popUp);
        }

        UpdateScreenBlockerState();
    }

    public void RemovePopUpView(PopUpView popUp)
    {
        if (_activePopUps.Contains(popUp))
        {
            _activePopUps.Remove(popUp);
        }

        UpdateScreenBlockerState();
    }

    public void UpdateScreenBlockerState()
    {
        ScreenBlocker.enabled = _activePopUps.Count > 0;
    }
}