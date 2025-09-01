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

    private Stack<FocusablePanel> focusablePanels = new Stack<FocusablePanel>();

    public void AddFocusable(FocusablePanel panel)
    {
        focusablePanels.Push(panel);
    }

    public void RemoveLastFocusable()
    {
        focusablePanels.Pop();
    }

    public FocusablePanel GetLastFocusable()
    {
        return focusablePanels.Peek();
    }
}

public abstract class FocusablePanel : MonoBehaviour
{

}
