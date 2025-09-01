using UnityEngine;
using UnityEngine.UI;

public class SoulEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private GameObject InteractionPanelObject;
    [SerializeField] private GameObject ActionsPanelObject;
    [SerializeField] private SpriteRenderer EnemySpriteRenderer;
    [Space]
    [SerializeField] private Button combatButton;

    private SpawnPoint _enemyPosition;
    private EnemyModel _enemyModel;

    public void SetupEnemy(Sprite sprite, SpawnPoint spawnPoint, EnemyModel enemyModel)
    {
        EnemySpriteRenderer.sprite = sprite;
        _enemyPosition = spawnPoint;
        _enemyModel = enemyModel;
        gameObject.SetActive(true);

        GameEvents.EnemyCreated?.Invoke(this);
    }

    public SpawnPoint GetEnemyPosition() => _enemyPosition;

    public GameObject GetEnemyObject() => gameObject;

    public Selectable GetCombatButton() => combatButton;

    private void ActiveCombatWithEnemy()
    {
        ActiveInteractionPanel(false);
        ActiveActionPanel(true);
    }

    private void ActiveInteractionPanel(bool active)
    {
        InteractionPanelObject.SetActive(active);
    }

    private void ActiveActionPanel(bool active)
    {
        ActionsPanelObject.SetActive(active);
    }

    private void UseBow()
    {
        // USE BOW
        GameEvents.EnemyKilled?.Invoke(this);

        int calculatedScore = GetScoreForDamage(DamageType.DISTANCE);
        GameEvents.ScoredPoints?.Invoke(calculatedScore);
    }

    private void UseSword()
    {
        // USE SWORD
        GameEvents.EnemyKilled?.Invoke(this);

        int calculatedScore = GetScoreForDamage(DamageType.MEELEE);
        GameEvents.ScoredPoints?.Invoke(calculatedScore);
    }

    private int GetScoreForDamage(DamageType damageType)
    {
        if (damageType == _enemyModel.Weakness)
        {
            return Mathf.RoundToInt(_enemyModel.Reward * 1.5f);
        }

        return _enemyModel.Reward;
    }

    #region OnClicks

    public void Combat_OnClick()
    {
        ActiveCombatWithEnemy();
    }

    public void Bow_OnClick()
    {
        UseBow();
    }

    public void Sword_OnClick()
    {
        UseSword();
    }

    #endregion
}


public interface IEnemy
{
    SpawnPoint GetEnemyPosition();
    GameObject GetEnemyObject();
    Selectable GetCombatButton();
}