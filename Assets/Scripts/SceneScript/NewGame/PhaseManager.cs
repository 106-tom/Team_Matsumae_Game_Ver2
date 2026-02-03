// ===================== PhaseManager.cs =====================
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PhaseManager : MonoBehaviourPun
{
    public static PhaseManager Instance;

    public enum Phase
    {
        None,
        Start,
        Draw,
        Mana,
        Main,
        Attack,
        End
    }

    public Phase CurrentPhase { get; private set; } = Phase.None;

    [SerializeField] private Phase currentPhaseDebug;

    private Coroutine phaseRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        yield return null; // Awake完了待ち
    }

    // --- ターン開始時に呼ぶ ---
    public void StartPhase()
    {
        if (!PlayerManager.LocalPlayer.IsMyTurn) return;

        StartCoroutine(RunPhaseSequence());
    }

    // --- フェーズの順序で進めるコルーチン ---
    private IEnumerator RunPhaseSequence()
    {
        // Start Phase
        CurrentPhase = Phase.Start;
        SyncPhase();
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Phase: " + CurrentPhase);
        yield return null; // 演出などあればここで待つ

        // Draw Phase
        CurrentPhase = Phase.Draw;
        SyncPhase();
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Phase: " + CurrentPhase);
        yield return StartCoroutine(DrawManager.Instance.DrawCardProcess(1)); // ドロー処理完了まで待つ

        // Mana Phase
        CurrentPhase = Phase.Mana;
        SyncPhase();
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Phase: " + CurrentPhase);
        ManaManager2.Instance.localPlayerManaUI?.ShowManaOptions();
        yield return null; // 必要なら演出待機

        // Main Phase
        CurrentPhase = Phase.Main;
        SyncPhase();
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Phase: " + CurrentPhase);
        yield return null;

        // Attack Phase
        CurrentPhase = Phase.Attack;
        SyncPhase();
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Phase: " + CurrentPhase);
        yield return null;

        // End Phase
        CurrentPhase = Phase.End;
        SyncPhase();
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Phase: " + CurrentPhase);

        // ターン終了
        TurnManager.Instance.EndTurn();
    }

    // --- 他のプレイヤーにフェーズ同期 ---
    private void SyncPhase()
    {
        photonView.RPC(nameof(RPC_SyncPhase), RpcTarget.Others, (int)CurrentPhase);
    }

    // --- 他プレイヤー側で受け取る ---
    [PunRPC]
    private void RPC_SyncPhase(int phase)
    {
        CurrentPhase = (Phase)phase;
        currentPhaseDebug = CurrentPhase;
        Debug.Log("Remote Phase: " + CurrentPhase);

        // ドロー処理はローカルプレイヤーだけが行うのでここでは開始しない
        if (!PlayerManager.LocalPlayer.IsMyTurn)
        {
            if (CurrentPhase == Phase.Mana)
            {
                ManaManager2.Instance.localPlayerManaUI?.ShowManaOptions();
            }
        }
    }
}
