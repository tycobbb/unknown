using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UnityEngine;

/// extensions for making tweens
public static class TweenExt {
    /// tween the lens w/ a linear value
    public static TweenerCore<float, float, FloatOptions> Tween(
        this Lens<float> prop,
        float src,
        Linear<float> dst
    ) {
        return prop.Tween(src, dst.Val, dst.Len);
    }

    /// tween the lens w/ a relative value
    public static TweenerCore<float, float, FloatOptions> Tween(
        this Lens<float> prop,
        Func<float, float> transform,
        float duration
    ) {
        var v0 = prop.Val;
        var v1 = transform.Invoke(v0);
        return prop.Tween(v0, v1, duration);
    }


    /// tween the lens
    public static TweenerCore<float, float, FloatOptions> Tween(
        this Lens<float> prop,
        float src,
        float dst,
        float duration
    ) {
        // set the initial value
        prop.Val = src;

        // create the tween
        var tween = DOTween.To(
            ( ) => prop.Val,
            (v) => prop.Val = v,
            dst,
            duration
        );

        return tween;
    }

    /// tween the lens
    public static TweenerCore<Color, Color, ColorOptions> Tween(
        this Lens<Color> prop,
        Color src,
        Color dst,
        float duration
    ) {
        // set the initial value
        prop.Val = src;

        // create the tween
        var tween = DOTween.To(
            ( ) => prop.Val,
            (v) => prop.Val = v,
            dst,
            duration
        );

        return tween;
    }
}