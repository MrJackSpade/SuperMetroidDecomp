using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>One consistent read of a render mailbox's private publication counters.</summary>
internal readonly record struct RenderMailboxCounterView(
    long Generation,
    long Published,
    long Taken,
    long Replaced,
    long Invalidated,
    bool HasPendingFrame);

/// <summary>
/// Verification views of private Core state whose debugger result fields were removed. Each
/// member reads the production state the removed field used to report.
/// </summary>
internal static partial class CoreAccess
{
    extension(CrateriaLightningPaletteFxProgramDefinition self)
    {
        /// <summary>
        /// The two timer-duration bytes that follow each <c>SetTimer</c> instruction: duration
        /// two after the second-timer instruction and duration one after the first-timer one.
        /// </summary>
        internal IReadOnlyList<(ushort Pointer, byte Value)> TimerMechanicsBytes =>
        [
            (unchecked((ushort)(PrivateState.Property<ushort>(self, "TimerTwoPointer") + 2)), 2),
            (unchecked((ushort)(PrivateState.Property<ushort>(self, "TimerOnePointer") + 2)), 1),
        ];
    }

    extension(SamusShinesparkState self)
    {
        /// <summary>Angular-travel word of departing crash-echo projectile slot three.</summary>
        internal SnesAngle FirstReleasedCrashEchoAngle =>
            PrivateState.Property<SnesAngle>(PrivateState.Field<object>(self, "_firstReleasedCrashEcho"), "Angle");

        /// <summary>Angular-travel word of departing crash-echo projectile slot four.</summary>
        internal SnesAngle SecondReleasedCrashEchoAngle =>
            PrivateState.Property<SnesAngle>(PrivateState.Field<object>(self, "_secondReleasedCrashEcho"), "Angle");

        /// <summary>Departing radius (projectile X velocity) of crash-echo projectile slot three.</summary>
        internal ushort FirstReleasedCrashEchoRadius =>
            PrivateState.Property<ushort>(PrivateState.Field<object>(self, "_firstReleasedCrashEcho"), "Radius");
    }

    extension(RoomPlmSystem self)
    {
        /// <summary>Block index of every active PLM, in native slot order.</summary>
        internal IReadOnlyList<int> ActivePlmBlockIndices =>
        [
            .. Group4ActivePlmSlots(self).Select(slot => PrivateState.Property<int>(slot, "BlockIndex")),
        ];

        /// <summary>Phase of every permanent-item PLM, in the same order as <see cref="RoomPlmSystem.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePhase> CollectiblePhases =>
        [
            .. Group4CollectibleItems(self).Select(item => PrivateState.Property<CollectiblePhase>(item, "Phase")),
        ];

        /// <summary>Presentation of every permanent-item PLM, in the same order as <see cref="RoomPlmSystem.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePresentation> CollectiblePresentations =>
        [
            .. Group4CollectibleItems(self).Select(item => PrivateState.Property<CollectiblePresentation>(item, "Presentation")),
        ];
    }

    extension(LatestRenderFrameMailbox self)
    {
        /// <summary>Reads every private counter under the mailbox's own lock.</summary>
        internal RenderMailboxCounterView VerificationCounters
        {
            get
            {
                lock (PrivateState.Field<object>(self, "sync"))
                {
                    return new RenderMailboxCounterView(
                        PrivateState.Field<long>(self, "generation"),
                        PrivateState.Field<long>(self, "published"),
                        PrivateState.Field<long>(self, "taken"),
                        PrivateState.Field<long>(self, "replaced"),
                        PrivateState.Field<long>(self, "invalidated"),
                        PrivateState.Field<RenderFrameSnapshot?>(self, "pending") is not null);
                }
            }
        }
    }

    extension(SuperMetroidRuntime self)
    {
        /// <summary>Cached gameplay TS byte the last accepted NMI uploaded with the window registers.</summary>
        internal byte CachedGameplaySubscreen =>
            (PrivateState.Field<GameplayWindowRegisterCache?>(self, "gameplayWindowRegisters")
                ?? throw new InvalidOperationException("Runtime has no gameplay window register cache."))
            .ReadByte(GameplayWindowRegisterAddresses.Subscreen);
    }

    private static IEnumerable<object> Group4ActivePlmSlots(RoomPlmSystem plms) =>
        PrivateState.Field<object[]>(plms, "_slots").Where(slot => PrivateState.Property<bool>(slot, "Active"));

    private static IEnumerable<object> Group4CollectibleItems(RoomPlmSystem plms) =>
        Group4ActivePlmSlots(plms)
            .Select(slot => PrivateState.Property<object?>(slot, "Item"))
            .Where(item => item is not null)
            .Select(item => item!);
}
