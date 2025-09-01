using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameplayNavigationView : FocusablePanel
{
    [SerializeField] private List<Selectable> buttonSelectables;

    private List<Selectable> _combatSelectables = new List<Selectable>();
    private Selectable _currentSelected;
    private List<Selectable> _activeList = new List<Selectable>();

    private InputControls _inputActions;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _inputActions = new InputControls();
    }

    private void Start()
    {
        NavigationController.Instance.AddFocusable(this);

        _activeList = _combatSelectables;
        _currentSelected = null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnEnable()
    {
        _inputActions.Gameplay.Enable();
        AttachListeners();
    }

    private void OnDisable()
    {
        DettachListeners();
        _inputActions.Gameplay.Disable();
    }

    private void AttachListeners()
    {
        GameEvents.EnemyCreated += AddCombatButton;
        GameEvents.EnemyKilled += RemoveCombatButton;

        _inputActions.Gameplay.Left.performed += MoveLeft;
        _inputActions.Gameplay.Right.performed += MoveRight;
        _inputActions.Gameplay.Confirm.performed += OnConfirm;
        _inputActions.Gameplay.UpperFocus.performed += ctx => ChangeFocus(1);
        _inputActions.Gameplay.LowerFocus.performed += ctx => ChangeFocus(-1);
    }

    private void DettachListeners()
    {
        GameEvents.EnemyCreated -= AddCombatButton;
        GameEvents.EnemyKilled -= RemoveCombatButton;

        _inputActions.Gameplay.Left.performed -= MoveLeft;
        _inputActions.Gameplay.Right.performed -= MoveRight;
        _inputActions.Gameplay.Confirm.performed -= OnConfirm;
        _inputActions.Gameplay.UpperFocus.performed -= ctx => ChangeFocus(1);
        _inputActions.Gameplay.LowerFocus.performed -= ctx => ChangeFocus(-1);
    }

    private void AddCombatButton(IEnemy enemy)
    {
        Selectable selectable = enemy.GetCombatButton();
        _combatSelectables.Add(selectable);

        _combatSelectables.Sort((a, b) =>
            _mainCamera.WorldToScreenPoint(a.transform.position).x
                .CompareTo(_mainCamera.WorldToScreenPoint(b.transform.position).x));
    }

    private void RemoveCombatButton(IEnemy enemy)
    {
        Selectable selectable = enemy.GetCombatButton();
        _combatSelectables.Remove(selectable);

        if (_currentSelected == selectable)
        {
            _currentSelected = null;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void MoveLeft(InputAction.CallbackContext ctx)
    {
        if (_activeList.Count == 0 || NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (_currentSelected == null)
        {
            _currentSelected = _activeList
                .Where(s => s != null && s.gameObject.activeInHierarchy)
                .OrderByDescending(s => _mainCamera.WorldToScreenPoint(s.transform.position).x)
                .FirstOrDefault();

            if (_currentSelected != null)
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
            return;
        }

        int index = _activeList.IndexOf(_currentSelected);
        int safety = _activeList.Count;

        do
        {
            if (index == -1) index = 0;
            index = (index - 1 + _activeList.Count) % _activeList.Count;
            safety--;

            if (_activeList[index] != null && _activeList[index].gameObject.activeInHierarchy)
            {
                _currentSelected = _activeList[index];
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
                return;
            }

        } while (safety > 0);
    }

    private void MoveRight(InputAction.CallbackContext ctx)
    {
        if (_activeList.Count == 0 || NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (_currentSelected == null)
        {
            _currentSelected = _activeList
                .Where(s => s != null && s.gameObject.activeInHierarchy)
                .OrderBy(s => _mainCamera.WorldToScreenPoint(s.transform.position).x)
                .FirstOrDefault();

            if (_currentSelected != null)
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
            return;
        }

        int index = _activeList.IndexOf(_currentSelected);
        int safety = _activeList.Count;

        do
        {
            if (index == -1) index = 0;
            index = (index + 1) % _activeList.Count;
            safety--;

            if (_activeList[index] != null && _activeList[index].gameObject.activeInHierarchy)
            {
                _currentSelected = _activeList[index];
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
                return;
            }

        } while (safety > 0);
    }

    private void OnConfirm(InputAction.CallbackContext ctx)
    {
        if (_currentSelected != null)
            _currentSelected.Select();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {

    }

    private void ChangeFocus1(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        List<Selectable> newList = null;
        bool pickLeft = true;

        if (value <= -.75f)
        {
            newList = _combatSelectables;
            pickLeft = true;
        }
        else if (value >= .75f)
        {
            newList = buttonSelectables;
            pickLeft = false;
        }
        else
        {
            return;
        }

        if (newList == null || newList.Count == 0)
            return;

        if (_currentSelected != null)
        {
            float currentX = _mainCamera.WorldToScreenPoint(_currentSelected.transform.position).x;

            Selectable closest = newList
                .Where(s => s != null && s.gameObject.activeInHierarchy)
                .OrderBy(s => Mathf.Abs(_mainCamera.WorldToScreenPoint(s.transform.position).x - currentX))
                .FirstOrDefault();

            _activeList = newList;
            _currentSelected = closest;
        }
        else
        {
            _activeList = newList;

            IEnumerable<Selectable> candidates = newList
                .Where(s => s != null && s.gameObject.activeInHierarchy);

            _currentSelected = pickLeft
                ? candidates.OrderBy(s => _mainCamera.WorldToScreenPoint(s.transform.position).x).FirstOrDefault()
                : candidates.OrderByDescending(s => _mainCamera.WorldToScreenPoint(s.transform.position).x).FirstOrDefault();
        }

        if (_currentSelected != null)
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
    }

    private void ChangeFocus(int direction)
    {
        if (NavigationController.Instance.GetLastFocusable() != this)
            return;

        if (direction == -1)
            _activeList = _combatSelectables;
        else if (direction == 1)
            _activeList = buttonSelectables;
        else
            return;

        _currentSelected = _activeList.FirstOrDefault();
        EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
    }
}
