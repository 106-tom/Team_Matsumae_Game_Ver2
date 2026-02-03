//ネットワーク越しにやり取りする「行動データ」
[System.Serializable]
public class PlayerActionData
{
    public int PlayerId;
    public PlayerActionType ActionType;
    public int CardId;
    public int AttackerId;
    public int TargetId;
	public int LifeChangeAmount;
}
