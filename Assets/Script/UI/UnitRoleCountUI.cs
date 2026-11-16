using UnityEngine;
using TMPro;
using System.Collections.Generic;


// 護衛モードと運搬モードの人数をUIに表示するクラス
public class UnitRoleCountUI : MonoBehaviour
{
    [Header("人数表示UI")]
    [SerializeField] private TMP_Text escortCountText;
    [SerializeField] private TMP_Text carryCountText;

    [Header("更新設定")]
    [SerializeField] private float updateInterval = 0.2f;

    private float updateTimer;


    private void Start()
    {
        UpdateRoleCounts();
    }


    void Update()
    {
        // 一定時間ごとに人数を更新
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateInterval = 0f;
            UpdateRoleCounts();
        }
    }


    // 各モードの人数を集計してUIを更新する
    private void UpdateRoleCounts()
    {
        // Registryから全ユニットを取得
        List<GameObject> players = PlayerUnitRegistry.GetAllPlayers();

        int escortCount = 0;
        int carryCount = 0;

        foreach (GameObject player in players)
        {
            if(player == null) continue;

            // PlayerStateManagerを取得
            PlayerStateManager stateManager = player.GetComponent<PlayerStateManager>();

            // 護衛モードの人数を火山
            if (stateManager.IsEscortMode())
            {
                escortCount++;
            }
            // 運搬モードの人数を加算
            else if (stateManager.IsCarryMode())
            {
                carryCount++;
            }
        }

        // UIに人数を表示
        if (escortCountText != null)
        {
            escortCountText.text = "EscortMode:" + escortCount;
        }
        if (carryCountText != null)
        {
            carryCountText.text = "CarryMode:" + carryCount;
        }
    }
}
