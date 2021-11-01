using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// the game
public class Game: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the configs for each player")]
    [SerializeField] PlayerConfig[] m_PlayerConfigs;

    // -- props --
    /// the current player count
    int m_NumPlayers;

    /// the array of players
    Player[] m_Players = new Player[2];

    /// if the players are colliding
    bool m_IsColliding;

    // -- lifecycle --
    void FixedUpdate() {
        RunCommands();
        TryCollide();
    }

    // -- commands --
    /// run commands based on inputs
    void RunCommands() {
        var k = Keyboard.current;

        var r = k.rKey;
        var s = k.sKey;
        var ctrl = k.ctrlKey;

        if (ctrl.isPressed && r.wasPressedThisFrame) {
            Reset();
        }

        if (ctrl.isPressed && s.wasPressedThisFrame) {
            Screenshot();
        }
    }

    /// add a new player to the game
    void AddPlayer(Player player) {
        var i = m_NumPlayers;

        // prepare player
        player.Join(m_PlayerConfigs[i]);

        // add to game
        m_Players[i] = player;
        m_NumPlayers++;
    }

    /// check collisions
    void TryCollide() {
        if (m_NumPlayers != 2) {
            return;
        }

        var p1 = m_Players[0];
        var p2 = m_Players[1];

        // if the collision state changed
        var isColliding = p1.Overlaps(p2) || p2.Overlaps(p1);
        if (m_IsColliding == isColliding) {
            return;
        }

        // and it's colliding, send the enter event
        if (isColliding) {
            p1.OnCollision(p2);
            p2.OnCollision(p1);
        }

        // update state
        m_IsColliding = isColliding;
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

        AddPlayer(player);
    }
}
