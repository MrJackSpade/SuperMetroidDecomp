namespace SuperMetroid.Core.Hud;

/// <summary>One gameplay message's definition (#2422, #2542).</summary>
/// <param name="MinimumDisplayFrames">
/// The updates the message stays displayed at least once its window has opened, before it takes input; none for a save
/// confirmation, which takes input as soon as its window opens.
/// </param>
/// <param name="Confirmation">What the message asks the player to confirm.</param>
/// <param name="PanelBinding">The binding whose button the message's panel names, or none for a panel that names no button.</param>
public sealed partial record GameplayMessageDefinition(
    int? MinimumDisplayFrames,
    GameplayMessageConfirmation Confirmation,
    GameplayMessagePanelButtonBinding? PanelBinding);
