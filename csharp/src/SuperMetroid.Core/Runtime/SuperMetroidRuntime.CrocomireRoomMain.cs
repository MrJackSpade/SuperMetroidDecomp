using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>The hidden-wall branch of MainASM_CrocomiresRoomShaking at $8F:E8CD/$E942.</summary>
    private void RunCrocomireComebackRoomMain()
    {
        if (ActiveRoom?.State.MainCallback != RoomMainCallback.CrocomireRoomShaking ||
            Enemies.Crocomire is not { } boss ||
            boss.Body.Properties.HasAny(EnemyProperties.Invisible) ||
            boss.DeathSequenceIndex != CrocomireDeathPhase.RumbleHiddenWall)
            return;

        var death = Enemies.CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire's comeback room callback has no death state.");
        BackgroundScroll.SetBg1VerticalScrollRegister(unchecked((ushort)(
            BackgroundScroll.Layer1YPosition + BackgroundScroll.Bg1YOffset + death.RumbleYOffset)));
    }
}
