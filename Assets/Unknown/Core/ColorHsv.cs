using UnityEngine;

/// an hsv color
public struct ColorHsv {
    // -- props --
    /// the hue
    public readonly float H;

    /// the saturation
    public readonly float S;

    /// the value
    public readonly float V;

    // -- lifetime --
    public ColorHsv(Color c) {
        Color.RGBToHSV(c, out H, out S, out V);
    }

    public ColorHsv(float h, float s, float v) {
        H = h;
        S = s;
        V = v;
    }

    // -- operators --
    /// add to each component
    public ColorHsv Add(float h = 0.0f, float s = 0.0f, float v = 0.0f) {
        return new ColorHsv(H + h, S + s, V + v);
    }

    /// multiply each component
    public ColorHsv Multiply(float h = 1.0f, float s = 1.0f, float v = 1.0f) {
        return new ColorHsv(H * h, S * s, V * v);
    }

    // -- queries --
    /// get rgb color from hsv
    public Color ToRgb(float a = 1.0f) {
        var color = Color.HSVToRGB(Mathf.Repeat(H, 1.0f), S, V);
        color.a = a;
        return color;
    }
}