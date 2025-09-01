using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour
{
    #region Singleton

    private static NavigationController _instance;

    public static NavigationController Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<NavigationController>();
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    private Stack<FocusablePanel> _focusablePanels = new Stack<FocusablePanel>();

    public void AddFocusable(FocusablePanel panel)
    {
        _focusablePanels.Push(panel);
    }

    public void RemoveLastFocusable()
    {
        if (_focusablePanels.Count > 1)
            _focusablePanels.Pop();
    }

    public FocusablePanel GetLastFocusable()
    {
        if (_focusablePanels.Count > 0)
            return _focusablePanels.Peek();
        else
            return null;
    }
}

public abstract class FocusablePanel : MonoBehaviour
{

}
