namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>
    /// Copies diagnostic state without reading cartridge data or invoking gameplay.
    /// Hosts capture this before a frame and again on failure to expose partial mutations.
    /// </summary>
    public string CaptureGameplayFailureContext()
    {
        string location = $"frame=${FrameNumber:X4}; state=${(ushort)GameState:X2} {GameState}; " +
            $"room=$8F:{GameplayActiveRoomPointer:X4}; roomState=$8F:{GameplayActiveRoomStatePointer:X4}; door=$83:{GameplayActiveDoorPointer:X4}";
        if (runtime?.Samus is not { } samus)
            return location + "; Samus=not initialized";
        return location + $"; pose=${(int)samus.Pose:X2}; animationFrame={samus.AnimationFrame}; animationTimer={samus.AnimationFrameTimer}; " +
            $"animationList=${samus.AnimationDelayListAddress:X6}; position={samus.XPosition},{samus.YPosition}; " +
            $"input=${runtime.Controller1.Current:X4}; newlyPressed=${runtime.Controller1.NewlyPressed:X4}; inputLocked={samus.InputLocked}; " +
            $"drainedPhase={samus.Drained.Phase}; drainedHandler={samus.Drained.GetUpHandler}; " +
            $"escapeTimer={runtime.EscapeTimer.State}; fx={samus.LiquidPhysics.FxType}; medium={samus.LiquidPhysics.LiquidMedium}; " +
            $"waterY=${samus.LiquidPhysics.FxYPosition:X4}; acidY=${samus.LiquidPhysics.LavaAcidYPosition:X4}";
    }
}
