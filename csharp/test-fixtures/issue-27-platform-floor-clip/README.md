# Issue 27: downward platform clip

This directory preserves the exact debug state supplied by the player after the
platform-clipping bug survived an earlier attempted fix. It is deliberately a copy
of live debug slot 0: the desktop application may overwrite the live slot at any
time, while this fixture must remain immutable for regression work.

## Reproduction

1. Load `slot-0.smstate`.
2. Jump.
3. Hold Down while Samus descends.
4. Samus incorrectly falls through the platform.

The issue must be reproduced from this state before another fix is attempted and
must remain open until the player confirms the correction.

## Integrity

- Size: 2,094,460 bytes
- SHA-256: `FA21EB27AC59A58356B8647FC95265B67CE6A2D30DF6B13E8BAF9018F0DB7997`
