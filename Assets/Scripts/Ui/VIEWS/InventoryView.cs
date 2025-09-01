using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : UiView
{
    public event Action OnGridRefresh;

    [Header("Inventory Elements")]
    [SerializeField]
    private SoulInformation SoulItemPlaceHolder;

    [SerializeField] private Text Description;
    [SerializeField] private Text Name;
    [SerializeField] private Image Avatar;
    [SerializeField] private Button UseButton;
    [SerializeField] private Button DestroyButton;

    private RectTransform _contentParent;
    private GameObject _currentSelectedGameObject;
    private SoulInformation _currentSoulInformation;
    private SoulItem _selectedSoulItem;
    private List<Selectable> _soulItems = new List<Selectable>();

    public override void Awake()
    {
        base.Awake();
        _contentParent = (RectTransform)SoulItemPlaceHolder.transform.parent;
        InitializeInventoryItems();
    }

    public List<Selectable> GetSelectables()
    {
        return _soulItems;
    }

    private void InitializeInventoryItems()
    {
        for (int i = 0, j = SoulController.Instance.Souls.Count; i < j; i++)
        {
            SoulInformation newSoul = Instantiate(SoulItemPlaceHolder.gameObject, _contentParent).GetComponent<SoulInformation>();
            newSoul.SetSoulItem(SoulController.Instance.Souls[i], () => SoulItem_OnClick(newSoul));

            _soulItems.Add(newSoul.GetSelectableButton());
        }

        SoulItemPlaceHolder.gameObject.SetActive(false);

        if (SoulController.Instance.Souls.Count > 0)
            _selectedSoulItem = SoulController.Instance.Souls[0];
    }

    private void OnEnable()
    {
        ClearSoulInformation();
        _contentParent.anchoredPosition = new Vector2(_contentParent.anchoredPosition.x, 0f);
    }

    private void ClearSoulInformation()
    {
        Description.text = "";
        Name.text = "";
        Avatar.sprite = null;
        SetupUseButton(false);
        SetupDestroyButton(false);
        _currentSelectedGameObject = null;
        _currentSoulInformation = null;
    }

    public void SoulItem_OnClick(SoulInformation soulInformation)
    {
        _currentSoulInformation = soulInformation;
        _currentSelectedGameObject = soulInformation.gameObject;
        SetupSoulInformation(soulInformation.soulItem);
    }

    private void SetupSoulInformation(SoulItem soulItem)
    {
        _selectedSoulItem = soulItem;

        Description.text = soulItem.Description;
        Name.text = soulItem.Name;
        Avatar.sprite = soulItem.Avatar;
        SetupUseButton(soulItem.CanBeUsed, soulItem);
        SetupDestroyButton(soulItem.CanBeDestroyed);
    }

    private void CantUseCurrentSoul()
    {
        PopUpInformation popUpInfo = new PopUpInformation { DisableOnConfirm = true, UseOneButton = true, Header = "CAN'T USE", Message = "THIS SOUL CANNOT BE USED IN THIS LOCALIZATION" };
        GUIController.Instance.ShowPopUpMessage(popUpInfo);
    }

    private void UseCurrentSoul(bool canUse, SoulItem soulItem)
    {
        if (!canUse)
        {
            CantUseCurrentSoul();
        }
        else
        {
            //USE SOUL
            GameEvents.ScoredPoints?.Invoke(soulItem.Reward);
            _soulItems.Remove(_currentSoulInformation.GetSelectableButton());

            Destroy(_currentSelectedGameObject);
            ClearSoulInformation();

            OnGridRefresh?.Invoke();
        }
    }

    private void DestroyCurrentSoul()
    {
        _soulItems.Remove(_currentSoulInformation.GetSelectableButton());

        Destroy(_currentSelectedGameObject);
        ClearSoulInformation();

        OnGridRefresh?.Invoke();
    }

    private void SetupUseButton(bool active, SoulItem soulItem = null)
    {
        if (_selectedSoulItem != null)
            UseButton.interactable = GameControlller.Instance.IsCurrentLocalization(_selectedSoulItem.UsableInLocalization);

        UseButton.onClick.RemoveAllListeners();
        if (active)
        {
            bool isInCorrectLocalization = GameControlller.Instance.IsCurrentLocalization(_currentSoulInformation.soulItem.UsableInLocalization);
            PopUpInformation popUpInfo = new PopUpInformation
            {
                DisableOnConfirm = isInCorrectLocalization,
                UseOneButton = false,
                Header = "USE ITEM",
                Message = "Are you sure you want to USE: " + _currentSoulInformation.soulItem.Name + " ?",
                Confirm_OnClick = () => UseCurrentSoul(isInCorrectLocalization, soulItem)
            };
            UseButton.onClick.AddListener(() => GUIController.Instance.ShowPopUpMessage(popUpInfo));
        }
        UseButton.gameObject.SetActive(active);
    }

    private void SetupDestroyButton(bool active)
    {
        DestroyButton.onClick.RemoveAllListeners();
        if (active)
        {
            PopUpInformation popUpInfo = new PopUpInformation
            {
                DisableOnConfirm = true,
                UseOneButton = false,
                Header = "DESTROY ITEM",
                Message = "Are you sure you want to DESTROY: " + Name.text + " ?",
                Confirm_OnClick = () => DestroyCurrentSoul()
            };
            DestroyButton.onClick.AddListener(() => GUIController.Instance.ShowPopUpMessage(popUpInfo));
        }

        DestroyButton.gameObject.SetActive(active);
    }
}