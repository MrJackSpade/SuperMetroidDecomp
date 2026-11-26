using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="AcceptedFrameBoundary"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record AcceptedFrameBoundaryDependencies
{
    /// <summary>The input half of the frame boundary: the controller latches and the frame counters.</summary>
    public required AcceptedFrameInput Input { get; init; }

    /// <summary>The HUD whose rows the effect overlay plane shows.</summary>
    public required HudState Hud { get; init; }

    /// <summary>Where the layers stand, which the shown frame captures.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>The frame's colours, which the shown frame latches with its scene (#3036).</summary>
    public required IFrameColourLatch Colours { get; init; }

    /// <summary>What the gameplay display shows, which the shown frame latches.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Sprite frame the frame boundary makes visible.</summary>
    public required SpriteFrameBuffer DisplayedSprites { get; init; }

    /// <summary>The shown layers the accepted frame latches.</summary>
    public required GameplayScreenLayers ScreenLayers { get; init; }

    /// <summary>Sprite frame the update finalized.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Samus, whose look the shown frame captures.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Active room, door, and scrolling sky.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>The suit transformation, whose colour the shown frame latches.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }

    /// <summary>What the X-ray scope reveals, built as the shown frame shows it.</summary>
    public required XrayReveal Xray { get; init; }
}
