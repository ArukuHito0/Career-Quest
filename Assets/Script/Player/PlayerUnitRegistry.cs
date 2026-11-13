using UnityEngine;
using System.Collections.Generic;

// シーン内のユニットを自動的に管理するクラス
public class PlayerUnitRegistry : MonoBehaviour
{
    // 登録されているPlayerUnitの一覧
    private static readonly List<GameObject> playerUnits = new List<GameObject>();

    // 外部からプレイヤー一覧を取得するためのプロパティ
    public static IReadOnlyList<GameObject> PlayerUnits => playerUnits;


    // PlayerUnitを登録する
    public static void Register(GameObject player)
    {
        if (player == null) return;

        if (!playerUnits.Contains(player))
        {
            playerUnits.Add(player);
        }
    }


    // PlayerUnitの登録を解除する
    public static void Unregister(GameObject player)
    {
        if (player == null) return;

        playerUnits.Remove(player);
    }


    // 登録されているPlayerUnitをすべて取得する
    public static List<GameObject> GetAllPlayers()
    {
        playerUnits.RemoveAll(player => player == null);

        return new List<GameObject>(playerUnits);
    }

    // PlayerMoveを持つユニットをすべて取得する
    public static PlayerMove[] GetAllPlayerMoves()
    {
        playerUnits.RemoveAll(player => player == null);

        List<PlayerMove> moves = new List<PlayerMove>();

        foreach (GameObject player in PlayerUnits)
        {
            PlayerMove playerMove = player.GetComponent<PlayerMove>();

            if (playerMove != null)
            {
                moves.Add(playerMove);
            }
        }

        return moves.ToArray();
    }

    private void OnDestroy()
    {
        playerUnits.Clear();
    }
}
