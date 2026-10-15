namespace SuperMetroid.Core.Assets;

/// <summary>The kinds of actor or display element that publish whole-frame sprite draws.</summary>
/// <remarks>Append-only: numeric values appear in serialized display packets and debugger state.</remarks>
public enum SpriteSourceKind
{
    /// <summary>The Ceres or Zebes escape countdown.</summary>
    EscapeTimer,
}
