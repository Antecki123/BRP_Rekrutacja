using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UiNavigationView : FocusablePanel
{
    [SerializeField] private List<Selectable> selectables;
    [SerializeField] private LayoutType layoutType;
    [Space]
    [SerializeField] private bool closeWithCancelButton;
    [SerializeField] private bool rememberLastButton;

    private Selectable _currentSelected;
    private InputControls _inputActions;

    private void Awake()
    {
        _inputActions = new InputControls();
    }

    private void OnEnable()
    {
        NavigationController.Instance.AddFocusable(this);

        _inputActions.UI.Enable();
        AttachListeners();

        if (selectables.Count == 0)
            return;

        StartCoroutine(DelayedSelect());
    }

    private void OnDisable()
    {
        if (!rememberLastButton)
            _currentSelected = null;

        DettachListeners();
        _inputActions.UI.Disable();

        NavigationController.Instance.RemoveLastFocusable();
    }

    private void AttachListeners()
    {
        _inputActions.UI.Confirm.performed += OnConfirm;
        _inputActions.UI.Cancel.performed += OnCancel;

        if (layoutType == LayoutType.VERTICAL)
        {
            _inputActions.UI.Up.performed += PreviousPosition;
            _inputActions.UI.Down.performed += NextPosition;
        }
        else if (layoutType == LayoutType.HORIZONTAL)
        {
            _inputActions.UI.Left.performed += PreviousPosition;
            _inputActions.UI.Right.performed += NextPosition;
        }
    }

    private void DettachListeners()
    {
        _inputActions.UI.Confirm.performed -= OnConfirm;
        _inputActions.UI.Cancel.performed -= OnCancel;

        if (layoutType == LayoutType.VERTICAL)
        {
            _inputActions.UI.Up.performed -= PreviousPosition;
            _inputActions.UI.Down.performed -= NextPosition;
        }
        else if (layoutType == LayoutType.HORIZONTAL)
        {
            _inputActions.UI.Left.performed -= PreviousPosition;
            _inputActions.UI.Right.performed -= NextPosition;
        }
    }

    private void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (_currentSelected != null)
            _currentSelected.Select();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (closeWithCancelButton && TryGetComponent(out UiView uiView))
        {
            NavigationController.Instance.RemoveLastFocusable();
            uiView.GetBackButton().onClick.Invoke();
        }
    }

    private void PreviousPosition(InputAction.CallbackContext ctx)
    {
        if (NavigationController.Instance.GetLastFocusable() != this)
            return;

        int index = selectables.IndexOf(_currentSelected);
        if (index <= 0)
            return;

        for (int i = index - 1; i >= 0; i--)
        {
            if (selectables[i] != null && selectables[i].gameObject.activeInHierarchy
                && selectables[i].IsInteractable())
            {
                _currentSelected = selectables[i];
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
                return;
            }
        }
    }

    private void NextPosition(InputAction.CallbackContext ctx)
    {
        if (NavigationController.Instance.GetLastFocusable() != this)
            return;

        int index = selectables.IndexOf(_currentSelected);
        if (index >= selectables.Count - 1)
            return;

        for (int i = index + 1; i < selectables.Count; i++)
        {
            if (selectables[i] != null && selectables[i].gameObject.activeInHierarchy
                && selectables[i].IsInteractable())
            {
                _currentSelected = selectables[i];
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
                return;
            }
        }
    }

    private IEnumerator DelayedSelect()
    {
        yield return null;

        if (!rememberLastButton || _currentSelected == null)
            _currentSelected = GetFirstAvailableSelectable();

        if (_currentSelected != null)
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
    }

    private Selectable GetFirstAvailableSelectable()
    {
        Selectable selectable = null;
        foreach (var s in selectables)
        {
            if (s.gameObject.activeInHierarchy && s.IsInteractable())
                return s;
        }

        return selectable;
    }
}

public enum LayoutType
{
    HORIZONTAL,
    VERTICAL
}
