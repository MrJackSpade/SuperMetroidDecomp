using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Direct ordered state-selection branches for every retail room.</summary>
/// <remarks>Native bank-$8F inline programs test named events, boss flags and
/// equipment predicates in order; the first successful condition wins. The 54
/// conditional programs are expressed as branches without stored clause arrays.
/// Unconditional defaults immediately follow the eleven-byte header and finish
/// word. Only the 262 retail room identities are accepted.</remarks>
public static class RoomStateSelectionDefinitions
{
    public const int RetailRoomCount = RoomHeaderDefinitions.RetailRoomCount;
    public const int ConditionalProgramCount = 54;

    /// <summary>Selects a state with native first-match precedence.</summary>
    public static ushort Select(ushort roomPointer, RoomStateSelectionContext selection)
    {
        _ = RoomHeaderDefinitions.Get(roomPointer);
        return Resolve(roomPointer, selection, null);
    }

    /// <summary>Returns the default first, then every alternative in native order.
    /// The caller-owned view is constructed on demand from the same branches;</n    /// it is never cached as a second program table.</summary>
    public static IReadOnlyList<ushort> GetStatePointers(ushort roomPointer)
    {
        _ = RoomHeaderDefinitions.Get(roomPointer);
        var variants = new List<ushort>();
        ushort fallback = Resolve(roomPointer, default, variants);
        variants.Insert(0, fallback);
        return variants.AsReadOnly();
    }

    private static ushort Resolve(ushort roomPointer, RoomStateSelectionContext selection, List<ushort>? variants)
    {
        ushort target = 0;
        bool Match(bool condition, ushort candidate)
        {
            target = candidate;
            variants?.Add(candidate);
            // Enumeration records every branch and reaches the default. Runtime
            // selection returns immediately on its first satisfied condition.
            return variants is null && condition;
        }
        switch (roomPointer)
        {
            case 0x91f8:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x9261)) return target;
                if (Match(selection.HasPowerBombs, 0x9247)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x922d)) return target;
                return 0x9213;
            case 0x92b3:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x92df)) return target;
                return 0x92c5;
            case 0x92fd:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x9348)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x932e)) return target;
                return 0x9314;
            case 0x96ba:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x9705)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x96eb)) return target;
                return 0x96d1;
            case 0x975c:
                if (Match(selection.HasMorphBallAndMissiles, 0x9787)) return target;
                return 0x976d;
            case 0x97b5:
                if (Match(selection.HasMorphBallAndMissiles, 0x97e0)) return target;
                return 0x97c6;
            case 0x9804:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x984f)) return target;
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), 0x9835)) return target;
                return 0x981b;
            case 0x9879:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesTimebombSet), 0x98c4)) return target;
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), 0x98aa)) return target;
                return 0x9890;
            case 0x9a44:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9a70)) return target;
                return 0x9a56;
            case 0x9a90:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9abc)) return target;
                return 0x9aa2;
            case 0x9dc7:
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), 0x9df3)) return target;
                return 0x9dd9;
            case 0x9e9f:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9ecb)) return target;
                return 0x9eb1;
            case 0x9f11:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9f3d)) return target;
                return 0x9f23;
            case 0x9f64:
                if (Match(selection.IsEventSet((byte)EventNumber.ZebesAwake), 0x9f90)) return target;
                return 0x9f76;
            case 0xa521:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xa54d)) return target;
                return 0xa533;
            case 0xa59f:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xa5cb)) return target;
                return 0xa5b1;
            case 0xa98d:
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), 0xa9b9)) return target;
                return 0xa99f;
            case 0xb283:
                if (Match(selection.IsBossDead(BossBits.AreaTorizo), 0xb2af)) return target;
                return 0xb295;
            case 0xb32e:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xb35a)) return target;
                return 0xb340;
            case 0xc98e:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xc9ba)) return target;
                return 0xc9a0;
            case 0xca08:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xca34)) return target;
                return 0xca1a;
            case 0xca52:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xca7e)) return target;
                return 0xca64;
            case 0xcaae:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcada)) return target;
                return 0xcac0;
            case 0xcaf6:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcb22)) return target;
                return 0xcb08;
            case 0xcb8b:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcbb7)) return target;
                return 0xcb9d;
            case 0xcbd5:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcc01)) return target;
                return 0xcbe7;
            case 0xcc27:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcc53)) return target;
                return 0xcc39;
            case 0xcc6f:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcc9b)) return target;
                return 0xcc81;
            case 0xcccb:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xccf7)) return target;
                return 0xccdd;
            case 0xcd13:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcd3f)) return target;
                return 0xcd25;
            case 0xcd5c:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcd88)) return target;
                return 0xcd6e;
            case 0xcda8:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xcdd4)) return target;
                return 0xcdba;
            case 0xcdf1:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xce1d)) return target;
                return 0xce03;
            case 0xce40:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xce6c)) return target;
                return 0xce52;
            case 0xce8a:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xceb6)) return target;
                return 0xce9c;
            case 0xcefb:
                if (Match(selection.IsEventSet((byte)EventNumber.MaridiaNoobTubeBroken), 0xcf27)) return target;
                return 0xcf0d;
            case 0xd78f:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xd7bb)) return target;
                return 0xd7a1;
            case 0xd8c5:
                if (Match(selection.IsEventSet((byte)EventNumber.ShaktoolClearedPath), 0xd8f1)) return target;
                return 0xd8d7;
            case 0xd95e:
                if (Match(selection.IsBossDead(BossBits.AreaMiniBoss), 0xd98a)) return target;
                return 0xd970;
            case 0xda60:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xda8c)) return target;
                return 0xda72;
            case 0xdae1:
                if (Match(selection.IsEventSet((byte)EventNumber.FirstMetroidHallCleared), 0xdb0d)) return target;
                return 0xdaf3;
            case 0xdb31:
                if (Match(selection.IsEventSet((byte)EventNumber.FirstMetroidShaftCleared), 0xdb5d)) return target;
                return 0xdb43;
            case 0xdb7d:
                if (Match(selection.IsEventSet((byte)EventNumber.SecondMetroidHallCleared), 0xdba9)) return target;
                return 0xdb8f;
            case 0xdbcd:
                if (Match(selection.IsEventSet((byte)EventNumber.SecondMetroidShaftCleared), 0xdbf9)) return target;
                return 0xdbdf;
            case 0xdc19:
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), 0xdc45)) return target;
                return 0xdc2b;
            case 0xdc65:
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), 0xdc91)) return target;
                return 0xdc77;
            case 0xdcb1:
                if (Match(selection.IsEventSet((byte)EventNumber.Unused14), 0xdcdd)) return target;
                return 0xdcc3;
            case 0xdd58:
                if (Match(selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss), 0xdda2)) return target;
                if (Match(selection.IsEventSet((byte)EventNumber.MotherBrainGlassDestroyed), 0xdd88)) return target;
                return 0xdd6e;
            case 0xdf45:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xdf71)) return target;
                return 0xdf57;
            case 0xdf8d:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xdfb9)) return target;
                return 0xdf9f;
            case 0xdfd7:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe003)) return target;
                return 0xdfe9;
            case 0xe021:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe04d)) return target;
                return 0xe033;
            case 0xe06b:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe097)) return target;
                return 0xe07d;
            case 0xe0b5:
                if (Match(selection.IsBossDead(BossBits.AreaBoss), 0xe0e1)) return target;
                return 0xe0c7;
            default: return unchecked((ushort)(roomPointer + RoomHeaderRomData.FixedHeaderByteCount + 2));
        }
    }
}
