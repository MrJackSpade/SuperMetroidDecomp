namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// What a title demonstration's input playback watches for before each of its steps (#1554): the condition that ends or
/// changes the recording the demonstration plays. Values are identities only; nothing reads them as numbers.
/// </summary>
public enum AttractDemoInputWatch
{
    /// <summary>The title leaving the demonstration, which ends the demonstration's input.</summary>
    TitleLeaving,

    /// <summary>
    /// The diagonal-shinespark demonstration's release: on the first step that plays, the shinespark's input continues
    /// into its continuation and the demonstration then watches for <see cref="TitleLeaving"/>.
    /// </summary>
    ShinesparkRelease,
}
