using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Wands {

/// a source that spawns hit rings
public sealed class HitRingSource: MonoBehaviour, IEffectSource<HitRingEvent> {
    // -- config --
    [Header("config")]
    [Tooltip("the hit prefab")]
    [SerializeField] GameObject m_Prefab;

    // -- IEffectSource --
    public IEffect<HitRingEvent> Init(HitRingEvent evt) {
        var obj = Instantiate(m_Prefab, evt.HitBox.Pos, Quaternion.identity);
        var hit = obj.GetComponent<HitRing>().IntoEffect(source: this);
        return hit;
    }
}

}