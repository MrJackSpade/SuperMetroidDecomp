namespace SuperMetroid.Core.Assets;

/// <summary>
/// The installed key of each <see cref="ActorDepth"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class ActorDepthKeys
{
    /// <summary>Each ActorDepth member's installed key.</summary>
    internal static InstalledKeys<ActorDepth> Installed { get; } = new(new Dictionary<ActorDepth, string>
    {
        [ActorDepth.BehindRoom] = "BehindRoom",
        [ActorDepth.BehindFrontParts] = "BehindFrontParts",
        [ActorDepth.InFront] = "InFront",
    });
}
