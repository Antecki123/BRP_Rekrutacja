using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameplayNavigationView : MonoBehaviour
{
    [SerializeField] private List<Selectable> buttonSelectables;
    [SerializeField] private List<Selectable> _combatSelectables;

    [SerializeField] private Selectable _currentSelected;
    private List<Selectable> _activeList;
    private bool _inCombat;

    private InputControls _inputActions;
    private Camera _mainCamera;

    public bool Gameplay; //DEBUG
    public bool Ui; //DEBUG

    private void Update() //DEBUG
    {
        if (_inputActions != null)
        {
            Gameplay = _inputActions.Gameplay.enabled;
            Ui = _inputActions.UI.enabled;
        }
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
        _inputActions = new InputControls();
        _activeList = _combatSelectables;
    }

    private void OnEnable()
    {
        _inputActions.Gameplay.Enable();
        AttachListeners();
    }

    private void OnDisable()
    {
        DettachListeners();
    }

    private void AttachListeners()
    {
        GameEvents.EnemyCreated += AddCombatButton;
        GameEvents.EnemyKilled += RemoveCombatButton;

        _inputActions.Gameplay.Left.performed += MoveLeft;
        _inputActions.Gameplay.Right.performed += MoveRight;
        _inputActions.Gameplay.Confirm.performed += OnConfirm;
        //_inputActions.Gameplay.Return.performed += OnCancel;

        _inputActions.Gameplay.ChangeFocus.performed += ChangeFocus;
    }

    private void DettachListeners()
    {
        GameEvents.EnemyCreated -= AddCombatButton;
        GameEvents.EnemyKilled -= RemoveCombatButton;

        _inputActions.Gameplay.Left.performed -= MoveLeft;
        _inputActions.Gameplay.Right.performed -= MoveRight;
        _inputActions.Gameplay.Confirm.performed -= OnConfirm;
        //_inputActions.Gameplay.Return.performed -= OnCancel;

        _inputActions.Gameplay.ChangeFocus.performed -= ChangeFocus;
    }

    private void AddCombatButton(IEnemy enemy)
    {
        Selectable selectable = enemy.GetCombatButton();

        _combatSelectables.Add(selectable);
        _combatSelectables = _combatSelectables
            .OrderBy(s => _mainCamera.WorldToScreenPoint(s.transform.position).x)
            .ToList();
    }

    private void RemoveCombatButton(IEnemy enemy)
    {
        Selectable selectable = enemy.GetCombatButton();

        _combatSelectables.Remove(selectable);
        _currentSelected = null;
    }

    private void MoveLeft(InputAction.CallbackContext ctx)
    {
        int index = _activeList.IndexOf(_currentSelected);
        int safety = _activeList.Count;

        do
        {
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
        int index = _activeList.IndexOf(_currentSelected);
        int safety = _activeList.Count;

        do
        {
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

    // TODO
    private void OnCancel(InputAction.CallbackContext ctx)
    {
        _inCombat = false;
    }

    private void ChangeFocus(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();

        if (value < 0)
            _activeList = _combatSelectables;
        else if (value > 0)
            _activeList = buttonSelectables;
        else
            return;

        if (_activeList != null && _activeList.Count > 0)
        {
            _currentSelected = _activeList.FirstOrDefault(s => s != null && s.gameObject.activeInHierarchy);
            if (_currentSelected != null)
                EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
        }
    }
}
