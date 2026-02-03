using UnityEngine;

public class NetworkPlayerController : MonoBehaviour
{
    public int playerId;

    //サーバー通信を仮想化（本番はネットワークイベントで呼び出す）
    public void OnReceiveAction(PlayerActionData actionData)
    {
        switch (actionData.ActionType)
        {
            case PlayerActionType.SelectCard:
                HandleSelectCard(actionData);
                break;

            case PlayerActionType.Attack:
                HandleAttack(actionData);
                break;
        }
    }

    private void HandleSelectCard(PlayerActionData actionData)
    {
        Card card = CardRegistry.GetCardById(actionData.CardId);
        if (card != null)
        {
            GameManager.Instance.SelectCard(playerId, card);
            Debug.Log($"[Network] Player {playerId} selected {card.id}");
        }
    }

    private void HandleAttack(PlayerActionData actionData)
    {
        Card attacker = CardRegistry.GetCardById(actionData.AttackerId);
        Card defender = CardRegistry.GetCardById(actionData.TargetId);

        if (attacker != null && defender != null)
        {
            GameManager.Instance.SelectCard(playerId, attacker);
            GameManager.Instance.SelectTarget(playerId, defender);
            Debug.Log($"[Network] Player {playerId} attacks {defender.id}");
        }
    }
}
