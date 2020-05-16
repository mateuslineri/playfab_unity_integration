using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabController : MonoBehaviour
{
    public GameObject CanvasMenu;
    public GameObject CanvasGame;
    public GameObject CanvasLogin;
    public GameObject CanvasRegister;

    //login variables
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public Button loginButton;

    //register variables
    public InputField usernameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;
    public Button registerButton;

    //stats variables
    public int playerLevel;
    public int playerPoints;
    public Text displayName;
    public Text displayPoints;
    public Text displayLevel;

    //general variables
    private string userEmail;
    private string username;
    private string userPassword;

    public void GetUserEmail(string emailIn) { userEmail = emailIn; }
    public void GetUserPassword(string passwordIn) { userPassword = passwordIn; }
    public void GetUsername(string usernameIn) { username = usernameIn; }

    void Start()
    {
        GoToMainMenu();

        if (string.IsNullOrEmpty(PlayFabSettings.TitleId)) {
            PlayFabSettings.TitleId = "45904";
        }

        if (PlayerPrefs.HasKey("EMAIL")) {
            GetPlayerPrefs();
            GetStats();
            GoToGame();

            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
        else {
#if UNITY_ANDROID
            var requestAndroid = LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileID(), CreateAccount = true };
            PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnMobileLoginSuccess, OnMobileLoginFailure);
#endif

#if UNITY_IOS
            var requestIOS = new LoginWithIOSDeviceIDRequest { DeviceId = ReturnMobileID(), CreateAccount = true };
            PlayFabClientAPI.LoginWithIOSDeviceID(requestIOS, OnMobileLoginSuccess, OnMobileLoginFailure);
#endif
        }      

    }

    void Update()
    {
        VerifyFields();
        DisplayUserInfo();
    }

#region login

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("OnLoginSucccess ok!!");
        SetPlayerPrefs();
        GetStats();
        GoToGame();

    }

    private void OnMobileLoginSuccess(LoginResult result)
    {
        Debug.Log("OnMobileLoginSucccess ok!!");
        GoToGame();
    }

    public void OnClickLogIn()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginFailure(PlayFabError error)
    { 
        Debug.Log("OnLoginFailure: " + error.GenerateErrorReport()); 
    }

    private void OnMobileLoginFailure(PlayFabError error)
    { 
        Debug.Log("OnMobileLoginFailure: " + error.GenerateErrorReport()); 
    }

#endregion login

#region register

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("OnRegisterSuccess ok");
        SetPlayerPrefs();
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = username }, OnDisplayName, OnRegisterFailure);
        GetStats(); // <-- AQUI
        GoToGame();
    }

    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " <---- Display name!");
    }

    public void OnClickRegister()
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError("OnRegisterFailure: " + error.GenerateErrorReport());
    }

#endregion register

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
        foreach (var eachStat in result.Statistics)
        {
            switch (eachStat.StatisticName)
            {
                case "Level":
                playerLevel = eachStat.Value;
                break;
                case "Points":
                playerPoints = eachStat.Value;
                break;
            }
        }
    }

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
        Debug.Log("Deu certo no Cloud Save Stats.");
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
        foreach (PlayerLeaderboardEntry player in result.Leaderboard) {
            Debug.Log(player.DisplayName + ": " + player.StatValue);
        }
    }

    void OnFailureGetLeaderdoarder(PlayFabError error)
    {
        Debug.LogError("Erro no GetLeaderboarder: " + error.GenerateErrorReport());
    }

#endregion leaderboard

#region utils

    private void VerifyFields()
    {
        loginButton.interactable = (emailLoginField.text.Length > 0 && passwordLoginField.text.Length > 0);
        registerButton.interactable = (usernameRegisterField.text.Length > 3 && emailRegisterField.text.Length > 5 && passwordRegisterField.text.Length > 8 && confirmPasswordRegisterField.text == passwordRegisterField.text);
    }

    private void DisplayUserInfo()
    {
        displayName.text = username;
        displayLevel.text = "Nível: " + playerLevel;
        displayPoints.text = "Pontos: " + playerPoints;
    }

    private void GetPlayerPrefs() 
    {
        userEmail = PlayerPrefs.GetString("EMAIL");
        username = PlayerPrefs.GetString("USERNAME");
        userPassword = PlayerPrefs.GetString("PASSWORD");
    }

    private void SetPlayerPrefs()
    {
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("USERNAME", username);
        PlayerPrefs.SetString("PASSWORD", userPassword);
    }

    public static string ReturnMobileID()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;

        return deviceID;
    }

    public void IncreaseLevel() 
    {
        playerLevel++;
        StartCloudUpdatePlayerStats();
    }

    public void IncreasePoints()
    {
        playerPoints++;
        StartCloudUpdatePlayerStats();
    }

#endregion utils

#region navigation

    public void GoToMainMenu()
    {
        PlayerPrefs.DeleteAll();
        CanvasMenu.SetActive(true);

        CanvasGame.SetActive(false);
        CanvasLogin.SetActive(false);
        CanvasRegister.SetActive(false);
    }

    public void GoToRegister()
    {
        CanvasRegister.SetActive(true);

        CanvasMenu.SetActive(false);
        CanvasGame.SetActive(false);
        CanvasLogin.SetActive(false);
    }

    public void GoToLogin()
    {
        CanvasLogin.SetActive(true);

        CanvasMenu.SetActive(false);
        CanvasGame.SetActive(false);
        CanvasRegister.SetActive(false);
    }

    public void GoToGame()
    {
        CanvasGame.SetActive(true);

        CanvasMenu.SetActive(false);
        CanvasLogin.SetActive(false);
        CanvasRegister.SetActive(false);
    }

    public void ExitApplication()
    {
        GoToMainMenu();
        Application.Quit();
    } 

#endregion navigation

#region devNotes

/*  
depois de criar o getStats, tem que alterar no script do Playfab (Automation > revisions > handlers.makeAPICall)
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

#endregion devNotes

}