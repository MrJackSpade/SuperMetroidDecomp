using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>Shared cartridge shadow-register ownership and accepted-NMI publication.</summary>
public sealed partial class SuperMetroidRuntime
{

    /// <summary>
    /// Window and screen-selection bytes uploaded by the most recent accepted NMI.
    /// A lag NMI deliberately leaves this snapshot unchanged.
    /// </summary>
    public GameplayWindowRegisterSnapshot DisplayedGameplayWindowRegisters =>
        GameplayWindowRegisters.Displayed;

    [field: NonSerialized]
    private GameplayWindowRegisterCache GameplayWindowRegisters
    {
        get
        {
            if (field is not null)
                return field;

            field = new GameplayWindowRegisterCache();
            field.InitializeWindowAndScreenSelection();
            return field;
        }
    }

    /// <summary>
    /// Reattaches nonserialized routing after construction or debugger-state restoration.
    /// The cache itself is presentation hardware and is republished from a clean layer-
    /// blending baseline on the next main-loop pass.
    /// </summary>
    private void BindGameplayWindowRegisters() =>
        Projectiles.BindGameplayWindowRegisters(GameplayWindowRegisters);

    /// <summary>
    /// Models the window/screen subset of the ordinary layer-blending initializer before
    /// projectile callbacks run. Edge and logic bytes intentionally survive; native
    /// <c>$88:8075</c> clears selection/admission and assigns TM/TS, but does not clear them.
    /// </summary>
    private void PrepareGameplayWindowRegisters()
    {
        GameplayWindowRegisterCache registers = GameplayWindowRegisters;
        registers.InitializeWindowAndScreenSelection();

        // Door and Mother Brain presentation already translate the non-default TM owners.
        // Seed the same shared byte before a Chainsaw slot-four word store can corrupt it.
        SnesMainScreenLayers mainScreen = DoorTransitionMainScreenLayers ??
            (SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj |
                (Enemies.MotherBrain?.DeathBg2Hidden == true
                    ? SnesMainScreenLayers.None
                    : SnesMainScreenLayers.Bg2));
        registers.WriteByte(
            GameplayWindowRegisterAddresses.MainScreen,
            (byte)mainScreen);
        if (DoorTransitionMainScreenLayers is null &&
            RoomLayer3Fx.LayerBlendConfiguration == LayerBlendingConfiguration.WaterfallSubtractive)
        {
            registers.WriteByte(GameplayWindowRegisterAddresses.MainScreen,
                (byte)WaterfallRoomDisplayRules.MainScreen);
            registers.WriteByte(GameplayWindowRegisterAddresses.Subscreen,
                (byte)WaterfallRoomDisplayRules.Subscreen);
        }
    }
}
