using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public InputField usernameField;
    public InputField passwordField;

    public Button loginButton;

    public PlayerStats playerStats;

    private string userEmail;
    private string username;
    private string userPassword;

    public void GetUserEmail(string emailIn) { userEmail = emailIn; }
    public void GetUserPassword(string passwordIn) { userPassword = passwordIn; } 
    public void GetUsername(string usernameIn) { username = usernameIn; }

    public void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId)) {
            PlayFabSettings.TitleId = "45904";
        }

        if (PlayerPrefs.HasKey("EMAIL")) {
            userEmail = PlayerPrefs.GetString("EMAIL");
            username = PlayerPrefs.GetString("USERNAME");
            userPassword = PlayerPrefs.GetString("PASSWORD");
            playerStats.GetStats();

            SceneManager.LoadScene(3);

            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        } else {
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

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("OnLoginSucccess ok!!");

        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("USERNAME", username);
        PlayerPrefs.SetString("PASSWORD", userPassword);

        playerStats.GetStats();

        SceneManager.LoadScene(3);

    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log("OnLoginFailure: " + error.GenerateErrorReport());
    }

    private void OnMobileLoginSuccess(LoginResult result)
    {
        Debug.Log("OnMobileLoginSucccess ok!!");

        SceneManager.LoadScene(3);
    }

    private void OnMobileLoginFailure(PlayFabError error)
    {
        Debug.Log("OnMobileLoginFailure: " + error.GenerateErrorReport());
    }

    public void OnClickLogIn() 
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    public static string ReturnMobileID()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;

        return deviceID;
    }

    void Update()
    {
        VerifyFields();
    }

    private void VerifyFields()
    {
        loginButton.interactable = (usernameField.text.Length > 0 && passwordField.text.Length > 0);
    }
}