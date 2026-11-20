namespace SuperMetroid.Core.Assets;

/// <summary>How the draws of each kind of sprite source are shaded (<see cref="SpriteSourceKind"/>).</summary>
public static class SpriteSourceKindDefinitions
{
    /// <summary>
    /// The sources whose draws show their pictures' group's shading over their own, each with its group: the escape
    /// timer's. Every other source's draws show their own shading under the display's fade.
    /// </summary>
    public static readonly IReadOnlyDictionary<SpriteSourceKind, GameplayPictures> GroupShaded = new Dictionary<SpriteSourceKind, GameplayPictures>
    {
        [SpriteSourceKind.EscapeTimer] = GameplayPictures.EscapeTimer,
    };
}
