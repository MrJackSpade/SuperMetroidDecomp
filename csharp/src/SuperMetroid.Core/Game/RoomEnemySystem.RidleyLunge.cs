namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Ports $A6:BA85: death has priority over power-bomb dodge and ordinary retreat.</summary>
    private void HandleNorfairRidleyMissedLunge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (unchecked((short)(state.ZeroHealthLungeCount - RidleyLungeDefinitions.DeathLungeLimit)) >= 0)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            BeginNorfairRidleyDeathRoar(slot, state);
            return;
        }
        if (_audioPowerBomb is { Flag: not 0 })
        {
            state.Function = RidleyAiFunction.NorfairReturnToArena;
            TickNorfairRidleyPowerBombDodge(slot, state);
            return;
        }
        state.Function = RidleyAiFunction.NorfairHoverSetup;
        state.TailWhipRequest = 1;
    }

    /// <summary>Ports $A6:BD4E, the power-bomb branch of missed-lunge recovery.</summary>
    private void TickNorfairRidleyPowerBombDodge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (_audioPowerBomb is not { Flag: not 0 } bomb)
        {
            state.FightMode = 1;
            state.Function = state.GrabState == 0
                ? RidleyAiFunction.NorfairSelectAttack : RidleyAiFunction.NorfairCarrySetup;
            return;
        }
        state.FightMode = 2;
        ushort targetX = unchecked((short)(bomb.XPosition - RidleyLungeDefinitions.DodgeSplitX)) >= 0
            ? RidleyLungeDefinitions.DodgeLeftX : RidleyLungeDefinitions.DodgeRightX;
        ushort targetY = unchecked((short)(bomb.YPosition - RidleyLungeDefinitions.DodgeSplitY)) >= 0
            ? RidleyLungeDefinitions.DodgeUpperY : RidleyLungeDefinitions.DodgeLowerY;
        MoveNorfairRidleyToward(slot, state, targetX, targetY, ReadRidleyHealthMovementDivisorIndex(state));
    }
}
