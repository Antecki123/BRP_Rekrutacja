using UnityEngine;

[CreateAssetMenu(fileName = "Soul Item", menuName = "Soul Item", order = 0)]
public class SoulItem : ScriptableObject
{
    public GameLocalization UsableInLocalization = GameLocalization.DUNGEON;
    public Sprite Avatar;
    public string Name;
    [TextArea(1, 5)]
    public string Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";
    public bool CanBeUsed;
    public bool CanBeDestroyed;
    public int Reward;
}

public enum DamageType
{
    NONE = 0,
    MEELEE = 1,
    DISTANCE = 2
}
