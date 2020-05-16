using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Register : MonoBehaviour
{
    private string userEmail;
    private string username;
    private string userPassword;

    public PlayerStats playerStats;

    public void GetUserEmail(string emailIn) { userEmail = emailIn; }
    public void GetUserPassword(string passwordIn) { userPassword = passwordIn; }
    public void GetUsername(string usernameIn) { username = usernameIn; }

    public InputField usernameField;
    public InputField emailField;
    public InputField passwordField;
    public InputField confirmPasswordField;

    public Button registerButton;

    void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId)) {
        PlayFabSettings.TitleId = "45904";
        }
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("OnRegisterSuccess ok");

        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("USERNAME", username);
        PlayerPrefs.SetString("PASSWORD", userPassword);

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = username }, OnDisplayName, OnRegisterFailure); 

        playerStats.GetStats();
        SceneManager.LoadScene(3);
    }

    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " <---- Display name!");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError("OnRegisterFailure: " + error.GenerateErrorReport());
    }

    public void OnClickRegister()
    {

        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    void Update()
    {
        VerifyFields();
    }

    private void VerifyFields()
    {
        registerButton.interactable = (usernameField.text.Length > 3 && emailField.text.Length > 5 && passwordField.text.Length > 8 && confirmPasswordField.text == passwordField.text);
    }
}
