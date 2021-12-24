using UnityEngine;

namespace Wands {

/// a source that spawns hit rings
public sealed class HitRingSource: MonoBehaviour, IEffectSource<HitRingEvent> {
    // -- config --
    [Header("config")]
    [Tooltip("the hit ring prefab")]
    [SerializeField] GameObject m_Prefab;

    // -- IEffectSource --
    public IEffect<HitRingEvent> Init(HitRingEvent evt) {
        var obj = Instantiate(m_Prefab, evt.HitBox.Pos, Quaternion.identity);
        var eff = obj.GetComponent<HitRing>();
        return eff;
    }
}

}