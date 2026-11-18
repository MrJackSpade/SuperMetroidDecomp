using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="MessageBoxFlow"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record MessageBoxFlowDependencies
{
    /// <summary>The runtime's audio requests, which the HUD update after a message box queues its selection sound into.</summary>
    public required GameplayAudioRequests AudioRequests { get; init; }

    /// <summary>Whether gameplay time is frozen, which the HUD update after a message box reads.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The HUD, whose counters a closing message box refreshes.</summary>
    public required HudState Hud { get; init; }

    /// <summary>The message box, whose yes-or-no answer a save station and the gunship read.</summary>
    public required GameplayMessageBoxState MessageBox { get; init; }


    /// <summary>The suit transformation a Varia or Gravity Suit pickup starts once its message closes.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }

    /// <summary>The session's shared game state, which records the saves the gunship and save stations make.</summary>
    public required SharedGameState System { get; init; }
}
