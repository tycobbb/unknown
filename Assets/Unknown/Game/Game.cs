using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Game: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the configs for each player")]
    [SerializeField] PlayerConfig[] mPlayerConfigs;

    // -- props --
    /// the current number of players
    int mNumPlayers = 0;

    // -- commands --
    /// reset the current scene
    void Reset() {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    /// take a screenshot of the scene
    void Screenshot() {
        var app = Application.productName.ToLower();
        ScreenCapture.CaptureScreenshot($"{app}.png");
    }

    // -- events --
    /// catch the reset input event
    public void OnReset(InputAction.CallbackContext ctx) {
        Reset();
    }

    /// catch the screenshot input event
    public void OnScreenshot(InputAction.CallbackContext ctx) {
        Screenshot();
    }

    /// catch the player joined event
    public void OnPlayerJoined(UnityEngine.InputSystem.PlayerInput input) {
        // get the player
        var player = input.GetComponent<Player>();
        if (player == null) {
            return;
        }

        // grab the next config
        Debug.Log($"i {mNumPlayers} cfgs {mPlayerConfigs}");
        var config = mPlayerConfigs[mNumPlayers];
        mNumPlayers++;

        // and prepare this player
        player.Join(config);
    }
}
