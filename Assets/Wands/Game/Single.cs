using UnityEngine;

namespace Wands {

/// a dependency container
public sealed class Single: MonoBehaviour {
    // -- module --
    /// the shared instance
    static Single s_Get;

    /// get the module
    public static Single Get {
        get => s_Get;
    }

    // -- props --
    [Header("single")]
    [Tooltip("the prefab for the hit effect")]
    public GameObject Hit;

    // -- lifecycle --
    void Awake() {
        if (s_Get == null) {
            s_Get = this;
        }
    }
}

}