using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Wands {

/// the game
public class Game: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the configs for each player")]
    [SerializeField] PlayerConfig[] m_PlayerConfigs;

    // -- props --
    /// the current player count
    int m_NumPlayers;

    /// the players
    Player[] m_Players = new Player[2];

    /// the walls containing the players
    Wall[] m_Walls = SquareWall.InitSquare();

    /// the collision mask
    /// p1 <-> p2    : 0b1........
    /// p1 <-> walls : 0b.1111....
    /// p2 <-> walls : 0b.....1111
    int m_Collisions = 0b000000000;

    /// if reset was pressed
    bool m_IsReset;

    /// if screenshot was pressed
    bool m_IsScreenshot;

    // -- lifecycle --
    void Start() {
        var prefab = GetComponent<PlayerInputManager>().playerPrefab;
        Instantiate(prefab);
        Instantiate(prefab);
    }

    void FixedUpdate() {
        // read input
        ReadHotkeys();

        // run game logic
        Hotkeys();
        Collide();
    }

    // -- commands --
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
    void Collide() {
        // run collisions
        var mask = 0b0_0000_0000;
        CollidePlayers(ref mask);
        CollideStage(ref mask);

        // update current mask
        m_Collisions = mask;
    }

    /// run player collisions
    void CollidePlayers(ref int mask) {
        // if we have two players
        if (m_NumPlayers < 2) {
            return;
        }

        var p1 = m_Players[0];
        var p2 = m_Players[1];

        // if a collision happened
        var c1 = p1.Collide(p2);
        var c2 = p2.Collide(p1);
        if (!c1 && !c2) {
            return;
        }

        var bit = 0b1;

        // if it just happened, fire events
        if (IsJustColliding(bit)) {
            // if simultaneous hit
            if (c1 && c2) {
                p1.OnPlayerContact(p2, mutual: true);
            }
            // if player 1 hit
            else if (c1) {
                p1.OnPlayerContact(p2, mutual: false);
            }
            // if player 2 hit
            else {
                p2.OnPlayerContact(p1, mutual: false);
            }
        }

        // update state
        mask |= bit;
    }

    /// run stage collisions
    void CollideStage(ref int mask) {
        // for each player
        for (var i = 0; i < m_NumPlayers; i++) {
            var player = m_Players[i];

            // and each wall
            for (var j = 0; j < m_Walls.Length; j++) {
                var wall = m_Walls[j];

                // check for a collision
                var contact = player.Collide(wall);

                // if it happened
                if (contact == null) {
                    continue;
                }

                // find collision bit (see `m_Collisions`)
                var pos = 1 + j + i * m_Walls.Length;
                var bit = 1 << pos;

                // if it just happened
                if (IsJustColliding(bit)) {
                    player.OnWallContact(contact.Value);
                }

                mask |= bit;
            }
        }
    }

    /// read hotkey inputs
    void ReadHotkeys() {
        var k = Keyboard.current;
        m_IsReset = k.ctrlKey.isPressed && k.rKey.wasPressedThisFrame;
        m_IsScreenshot = k.ctrlKey.isPressed && k.sKey.wasPressedThisFrame;
    }

    /// run hotkeys
    void Hotkeys() {
        if (m_IsReset) {
            Reset();
        }

        if (m_IsScreenshot) {
            Screenshot();
        }
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

    // -- queries --
    /// if the collision bit turned on this frame
    bool IsJustColliding(int bit) {
        return bit != 0 && (bit & m_Collisions) == 0;
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

}