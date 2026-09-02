# Issue 12: screen wrap after the first Missile room

This directory preserves the exact debug state supplied by the player after the
screen-wrap bug survived an earlier attempted fix. It is deliberately a copy of
live debug slot 1 so later state saves cannot destroy the reproduction.

## Reproduction

1. Load `slot-1.smstate`.
2. Press and hold Jump.
3. The gameplay view incorrectly wraps and leaves Samus outside the expected
   screen space.

The issue must be reproduced from this state before another fix is attempted and
must remain open until the player confirms the correction.

## Integrity

- Size: 2,106,617 bytes
- SHA-256: `9D585BCA6FD43C43E1FC8A9A71AFDEA87F504C321F7396086E390C77865548D8`
