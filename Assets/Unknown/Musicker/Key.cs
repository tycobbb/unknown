/// a western-musical key
public readonly struct Key {
    // -- props --
    /// the root tone for this key
    readonly Tone mRoot;

    // -- lifetime --
    /// create a new key with the root
    public Key(Tone root) {
        mRoot = root;
    }

    public Key(Root root) {
        mRoot = new Tone((int)root);
    }

    // -- queries --
    /// transpose tone to this key
    public Tone Transpose(Tone tone) {
        return tone.Transpose(mRoot);
    }
}