using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int playerLevel;
    public int playerPoints;

    public Text displayName;
    public Text displayPoints;
    public Text displayLevel;

    /*
    public void SetStats() 
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest{
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "Level", Value = playerLevel },
                new StatisticUpdate { StatisticName = "Points", Value = playerPoints },
            }
        },
        result => { Debug.Log("SetStats OK!! "); },
        error => { Debug.LogError("Erro no SetStats: " + error.GenerateErrorReport()); } );
    }
    */

    #region stats

    public void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError("Erro no GetStats: " + error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {
        foreach (var eachStat in result.Statistics) {
            switch(eachStat.StatisticName)
            {
                case "Level" :
                    playerLevel = eachStat.Value;
                    break;
                case "Points" :
                    playerPoints = eachStat.Value;
                    break;
            }
        }
    }

    /*  
    depois disso, tem que alterar no script do Playfab (Automation > revisions > handlers.makeAPICall)
    adicionando os campos necessários DENTRO DO ARRAY e mudar o nome da função para "UpdatePlayerStats" 

    Ex:
    handlers.UpdatePlayerStats = function(args, context)
    {
        var request = {
            PlayFabId: currentPlayerId, Statistics:
        [{
        StatisticName: "PlayerLevel",
                    Value: args.playerLevel
                },
                {
        StatisticName: "PlayerPoints",
                    Value: args.playerPoints
                }]
        };
    */

    public void StartCloudUpdatePlayerStats()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest() 
        {
            FunctionName = "UpdatePlayerStats",
            FunctionParameter = new { Level = playerLevel, Points = playerPoints },
        }, OnCloudUpdateStats, OnErrorShared);
    }

    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        Debug.Log("Deu certo no Cloud Save Stats");
    }

    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log("Erro no Cloud Save Stats: " + error.GenerateErrorReport());
    }

    #endregion stats

    #region leaderboard

    public void GetLeaderboarder()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "Level", MaxResultsCount = 20 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboarder, OnFailureGetLeaderdoarder);
    }

    void OnGetLeaderboarder(GetLeaderboardResult result)
    {
        foreach(PlayerLeaderboardEntry player in result.Leaderboard) {
            Debug.Log(player.DisplayName + ": " + player.StatValue);
        }
    }

    void OnFailureGetLeaderdoarder(PlayFabError error)
    {
        Debug.LogError("Erro no GetLeaderboarder: " + error.GenerateErrorReport());
    }

    #endregion leaderboard
}
