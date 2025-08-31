using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UiNavigationView : MonoBehaviour
{
    [SerializeField] private List<Selectable> selectables;
    [SerializeField] private LayoutType layoutType;
    [SerializeField] private bool closeWithCancelButton;
    [Space]
    [SerializeField] private Selectable _currentSelected;

    private InputControls _inputActions;

    private void Awake()
    {
        _inputActions = new InputControls();
    }

    private void Start()
    {
        if (selectables.Count > 0 && _currentSelected == null)
        {
            _currentSelected = selectables[0];
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }
    }

    private void OnEnable()
    {
        _inputActions.UI.Enable();
        AttachListeners();

        if (_currentSelected != null)
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
    }

    private void OnDisable()
    {
        DettachListeners();
        _inputActions.UI.Disable();
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
            uiView.GetBackButton().onClick.Invoke();
        }
    }

    private void PreviousPosition(InputAction.CallbackContext ctx)
    {
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
}

public enum LayoutType
{
    HORIZONTAL,
    VERTICAL
}
