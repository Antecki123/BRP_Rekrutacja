using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryNavigationView : FocusablePanel
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int columns = 3;
    [Space]
    [SerializeField] private Selectable useAction;
    [SerializeField] private Selectable destroyAction;

    private List<Selectable> _selectablesGrid;
    private Selectable _currentSelected;
    private Button _closeButton;
    private bool _inGridNavigation;

    private InventoryView _inventoryView;
    private InputControls _inputActions;

    private void Awake()
    {
        _inGridNavigation = true;

        if (TryGetComponent(out InventoryView inventoryView))
        {
            _inventoryView = inventoryView;
            _closeButton = _inventoryView.GetBackButton();
        }

        _inputActions = new InputControls();
        _selectablesGrid = new List<Selectable>();
    }

    private void Start()
    {
        _selectablesGrid.Clear();
        _selectablesGrid = _inventoryView.GetSelectables();
    }

    private void OnEnable()
    {
        NavigationController.Instance.AddFocusable(this);

        _inputActions.UI.Enable();
        AttachListeners();

        _currentSelected = null;
    }

    private void OnDisable()
    {
        DettachListeners();
        _inputActions.UI.Disable();

        NavigationController.Instance.RemoveLastFocusable();
    }

    private void AttachListeners()
    {
        _inputActions.UI.Confirm.performed += OnConfirm;
        _inputActions.UI.Cancel.performed += OnCancel;

        _inputActions.UI.Up.performed += OnUp;
        _inputActions.UI.Down.performed += OnDown;
        _inputActions.UI.Left.performed += OnLeft;
        _inputActions.UI.Right.performed += OnRight;

        _inventoryView.OnGridRefresh += RefreshGrid;
    }

    private void DettachListeners()
    {
        _inputActions.UI.Confirm.performed -= OnConfirm;
        _inputActions.UI.Cancel.performed += OnCancel;

        _inputActions.UI.Up.performed -= OnUp;
        _inputActions.UI.Down.performed -= OnDown;
        _inputActions.UI.Left.performed -= OnLeft;
        _inputActions.UI.Right.performed -= OnRight;

        _inventoryView.OnGridRefresh -= RefreshGrid;
    }

    private void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (!_inGridNavigation || !ctx.performed)
            return;

        _inGridNavigation = false;

        Selectable action = GetFirstAvailableAction();
        if (action == null)
        {
            _inGridNavigation = true;
            return;
        }

        StartCoroutine(SelectAfterRelease(action, ctx.action));
    }

    private IEnumerator SelectAfterRelease(Selectable target, InputAction submitAction)
    {
        yield return null;

        while (submitAction != null && submitAction.IsPressed())
            yield return null;

        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (_inGridNavigation)
        {
            _closeButton.onClick.Invoke();
        }
        else
        {
            _inGridNavigation = true;
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }
    }

    private void OnUp(InputAction.CallbackContext ctx)
    {
        if (!_inGridNavigation || NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (_currentSelected == null && _selectablesGrid.Count > 0)
        {
            _currentSelected = _selectablesGrid.FirstOrDefault();
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }

        if (_currentSelected == null)
            return;

        int index = _selectablesGrid.IndexOf(_currentSelected);
        int row = index / columns;
        int col = index % columns;

        if (row == 0)
            return;

        int newIndex = (row - 1) * columns + col;
        if (newIndex < _selectablesGrid.Count)
        {
            _currentSelected = _selectablesGrid[newIndex];
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
            EnsureVisible(_currentSelected);

            _currentSelected.Select();
            CustomSubmit();
        }
    }

    private void OnDown(InputAction.CallbackContext ctx)
    {
        if (!_inGridNavigation || NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (_currentSelected == null && _selectablesGrid.Count > 0)
        {
            _currentSelected = _selectablesGrid.FirstOrDefault();
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }

        if (_currentSelected == null)
            return;

        int index = _selectablesGrid.IndexOf(_currentSelected);
        int row = index / columns;
        int col = index % columns;

        int newIndex = (row + 1) * columns + col;
        if (newIndex < _selectablesGrid.Count)
        {
            _currentSelected = _selectablesGrid[newIndex];
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
            EnsureVisible(_currentSelected);

            _currentSelected.Select();
            CustomSubmit();
        }
    }

    private void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!_inGridNavigation || NavigationController.Instance.GetLastFocusable() != this)
        {
            ActionPanelNavigation(-1);
            return;
        }

        if (_currentSelected == null && _selectablesGrid.Count > 0)
        {
            _currentSelected = _selectablesGrid.FirstOrDefault();
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }

        if (_currentSelected == null)
            return;

        int index = _selectablesGrid.IndexOf(_currentSelected);
        if (index % columns == 0)
            return;

        _currentSelected = _selectablesGrid[index - 1];
        EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);

        _currentSelected.Select();
        CustomSubmit();
    }

    private void OnRight(InputAction.CallbackContext ctx)
    {
        if (!_inGridNavigation || NavigationController.Instance.GetLastFocusable() != this)
        {
            ActionPanelNavigation(1);
            return;
        }

        if (_currentSelected == null && _selectablesGrid.Count > 0)
        {
            _currentSelected = _selectablesGrid.FirstOrDefault();
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }

        if (_currentSelected == null)
            return;

        int index = _selectablesGrid.IndexOf(_currentSelected);
        if (index % columns == columns - 1 || index + 1 >= _selectablesGrid.Count)
            return;

        _currentSelected = _selectablesGrid[index + 1];
        EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);

        _currentSelected.Select();
        CustomSubmit();
    }

    private void EnsureVisible(Selectable selectable)
    {
        Vector3 selectedLocalPos = content.InverseTransformPoint(_currentSelected.transform.position);

        float viewportHeight = scrollRect.viewport.rect.height;

        float viewportTop = -content.localPosition.y;
        float viewportBottom = viewportTop - viewportHeight;

        RectTransform selectedRectTransform = selectable.GetComponent<RectTransform>();
        float elementTop = selectedLocalPos.y + selectedRectTransform.rect.height / 2;
        float elementBottom = selectedLocalPos.y - selectedRectTransform.rect.height / 2;

        if (elementTop > viewportTop)
        {
            float offset = elementTop - viewportTop;
            content.localPosition -= new Vector3(0, offset, 0);
        }
        else if (elementBottom < viewportBottom)
        {
            float offset = viewportBottom - elementBottom - selectedRectTransform.rect.y;
            content.localPosition += new Vector3(0, offset, 0);
        }
    }

    private void CustomSubmit()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current != null)
        {
            ExecuteEvents.Execute<ISubmitHandler>(
                current,
                new BaseEventData(EventSystem.current),
                ExecuteEvents.submitHandler
            );
        }
    }

    private void ActionPanelNavigation(int direction)
    {
        Selectable selectedAction = null;

        if (direction == -1 && useAction.gameObject.activeInHierarchy && useAction.interactable)
        {
            selectedAction = useAction;

        }
        else if (direction == 1 && destroyAction.gameObject.activeInHierarchy && destroyAction.interactable)
        {
            selectedAction = destroyAction;
        }

        if (selectedAction != null)
            EventSystem.current.SetSelectedGameObject(selectedAction.gameObject);
    }

    private Selectable GetFirstAvailableAction()
    {
        if (useAction.gameObject.activeInHierarchy && useAction.interactable)
            return useAction;
        else if (destroyAction.gameObject.activeInHierarchy && destroyAction.interactable)
            return destroyAction;
        else return null;
    }

    private void RefreshGrid()
    {
        _inGridNavigation = true;
        _selectablesGrid = _inventoryView.GetSelectables();
    }
}
