using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// the game
public class Game: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the configs for each player")]
    [SerializeField] PlayerConfig[] mPlayerConfigs;


    // -- props --
    /// the current player count
    int mNumPlayers = 0;

    /// the array of players
    Player[] mPlayers = new Player[2];

    /// if the players are colliding
    bool mIsColliding = false;

    // -- lifecycle --
    void FixedUpdate() {
        TryCollide();
    }

    // -- commands --
    /// collide the players together if necessary
    void TryCollide() {
        if (mNumPlayers != 2) {
            return;
        }

        var player1 = mPlayers[0];
        var player2 = mPlayers[1];

        // if the collision state changed
        var isColliding = player1.Overlaps(player2);
        if (mIsColliding == isColliding) {
            return;
        }

        // and it's colliding, send the enter event
        if (isColliding) {
            player1.OnCollision(player2);
            player2.OnCollision(player1);
        }

        // update state
        mIsColliding = isColliding;

    }

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
    public void OnPlayerJoined(Object obj) {
        // ignore call on startup where obj is the game (b/c it has an input?)
        var input = obj as PlayerInput;
        if (input == null) {
            return;
        }

        // make sure we have a player
        var player = input.GetComponent<Player>();
        if (player == null) {
            return;
        }

        var i = mNumPlayers;

        // join the game
        player.Join(mPlayerConfigs[i]);

        // update state
        mPlayers[i] = player;
        mNumPlayers++;
    }
}
