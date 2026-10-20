namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Whether applying a pause label keeps the tilemap's attributes.</summary>
public enum LabelAttributes
{
    /// <summary>The label's attributes are written.</summary>
    Replace,

    /// <summary>The tilemap's existing attributes are kept.</summary>
    Preserve,
}
