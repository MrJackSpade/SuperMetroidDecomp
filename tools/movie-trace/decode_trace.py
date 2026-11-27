"""Decodes a committed compact movie trace with the playthroughs' layout, to check the layout as reconstructed and to look at what the records hold.

Usage: decode_trace.py <trace folder> [first update] [count]
"""
import gzip
import json
import struct
import sys

# The playthrough layout ("movie-desync"), as csharp/test-support/MovieReplay/ReplayTraceMemoryCatalog.cs gives it: (start, length).
RANGES = [
    (0x05b6, 2),                      # the NMI counter
    (0x05e5, 2),                      # the random number
    (0x0629, 0x0650 - 0x0629),        # the sound queues
    (0x079b, 2),                      # the room
    (0x090f, 0x0915 + 2 - 0x090f),    # the view: X part, X, Y part, Y
    (0x093f, 2),                      # Ceres status
    (0x0943, 0x0948 - 0x0943),        # the escape's time
    (0x0998, 2),                      # the game state
    (0x099c, 2),                      # the door's function
    (0x09a2, 0x09a8 + 2 - 0x09a2),    # equipment and beams, switched on and owned
    (0x09c2, 0x09d6 + 2 - 0x09c2),    # energy to reserves
    (0x0a1c, 2),                      # her pose
    (0x0a94, 0x0a98 - 0x0a94),        # her animation
    (0x0af6, 0x0afc + 2 - 0x0af6),    # her place
    (0x0b2c, 0x0b30 - 0x0b2c),        # her speed down
    (0x0b42, 0x0b4c - 0x0b42),        # her speeds across
    (0x0b64, 0x0c2c - 0x0b64),        # her shots
    (0x0dba, 0x0dc0 - 0x0dba),        # slope and total speed
] + [(0x0f78 + slot * 64, 0x1e) for slot in range(32)] + [
    (0x0fa8, 0x0fae - 0x0fa8),        # Ridley's words (inside enemy slot 0's stride, after its prefix)
    (0x106a, 0x106e - 0x106a),        # the baby's velocity
    (0x1997, 18 * 2),                 # what enemies launch: kinds
    (0x1a4b, 18 * 2),                 # their places across
    (0x1a93, 18 * 2),                 # their places down
    (0x1b23, 0x1bb3 - 0x1b23),        # their variables to timers
    (0x1f51, 2),                      # the cinematic's function
    (0x2000, 2),                      # Ridley's tail function
    (0x2020, 20 * 7),                 # Ridley's tail
    (0x8026, 2),                      # Ridley's getaway
]
RANGES.sort()
OFFSET = {}
at = 0
for start, length in RANGES:
    OFFSET[start] = at
    at += length
PAYLOAD = at


def offset_of(address: int) -> int:
    for start, length in RANGES:
        if start <= address < start + length:
            return OFFSET[start] + address - start
    raise KeyError(hex(address))


def word(payload: bytes, address: int) -> int:
    at = offset_of(address)
    return payload[at] | (payload[at + 1] << 8)


def records(folder: str):
    manifest = json.load(open(folder + "/compact-manifest.json", encoding="utf-8"))
    if manifest["payloadBytes"] != PAYLOAD:
        raise SystemExit(f"the layout comes to {PAYLOAD} bytes; the trace's payload is {manifest['payloadBytes']}")
    raw = gzip.open(folder + "/expected.bin.gz", "rb").read()
    size = 8 + PAYLOAD
    if len(raw) != size * manifest["recordCount"]:
        raise SystemExit(f"{len(raw)} bytes is not {manifest['recordCount']} records of {size}")
    previous = bytes(PAYLOAD)
    for index in range(manifest["recordCount"]):
        frame, pc = struct.unpack_from("<II", raw, index * size)
        delta = raw[index * size + 8:(index + 1) * size]
        previous = bytes(a ^ b for a, b in zip(previous, delta))
        yield index, frame, pc, previous


if __name__ == "__main__":
    folder = sys.argv[1]
    first = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    count = int(sys.argv[3]) if len(sys.argv) > 3 else 12
    updates = json.load(gzip.open(folder + "/updates.json.gz", "rt", encoding="utf-8"))
    by_record = {u["expectedRecord"]: u for u in updates["updates"] if u.get("expectedRecord") is not None}
    print("payload", PAYLOAD, "bytes; updates", len(updates["updates"]), "; first update", json.dumps(updates["updates"][0])[:300])
    shown = 0
    for index, frame, pc, payload in records(folder):
        update = by_record.get(index)
        if update is None or update["update"] < first:
            continue
        print(f"update {update['update']:>7} frame {frame:>7} {update['kind'][:14]:<14} in {update['input']:#06x} "
              f"state {word(payload, 0x0998):#04x} room {word(payload, 0x079b):#06x} rng {word(payload, 0x05e5):#06x} "
              f"Samus {word(payload, 0x0af6)}.{word(payload, 0x0af8):04x},{word(payload, 0x0afa)}.{word(payload, 0x0afc):04x} "
              f"energy {word(payload, 0x09c2)} view {word(payload, 0x0911)},{word(payload, 0x0915)}")
        shown += 1
        if shown >= count:
            break
