using UnityEngine;

namespace Wands {

/// a source that spawns bounce lines
public sealed class BounceLineSource: MonoBehaviour, IEffectSource<Collision> {
    // -- config --
    [Header("config")]
    [Tooltip("the hit ring prefab")]
    [SerializeField] GameObject m_Prefab;

    // -- IEffectSource --
    public IEffect<Collision> Init(Collision wall) {
        var rot = Quaternion.LookRotation(Vector3.forward, wall.Normal);
        var obj = Instantiate(m_Prefab, wall.Pos, rot);
        var eff = obj.GetComponent<BounceLine>();
        return eff;
    }
}

}
