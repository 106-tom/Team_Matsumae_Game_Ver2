using UnityEngine;

public class TurnEndMarker : MonoBehaviour
{
    private void OnMouseDown()
    {
        // プレイヤーのターンでのみ有効にする（安全対策）
        if (GameManager.Instance.IsPlayerTurn(GameManager.Instance.CurrentPlayerId))
        {

        }
    }
}
