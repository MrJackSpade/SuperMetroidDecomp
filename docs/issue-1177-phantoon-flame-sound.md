# Phantoon casual flame sound library

Affected version: v0.4.5, carried forward from the latest explicit player
diagnostic. The player reports the blue eye/fire actors making the wrong sound;
the exact event was unavailable on follow-up.

Source inspection found a definite error in SpawnCasualFlame. Native
$A7:CF68 loads sound $1D and $A7:CF6B calls QueueSound_Lib3_Max6. The port stored
$1D in LastMaterializationSound, whose legacy publisher routes through library
two. The same numeric ID denotes a different sound in that library. Reference:
InsaneFirebat disassembly revision 362be646929cf8e483f692b73a6561cfc2dc1d0d,
bank A7, SpawnCasualFlame. Rain fall/impact ($86:9AA3/9AD5, library three $1D)
and rage waves ($A7:D8E9, library three $29) already use the correct libraries.

The new `--phantoon-flame-sound` regression executes the actual spawn callback
and audio publication. Before the fix it fails with expected Library3:$1D,
actual Library2:$1D. Replaced the materialization latch write with a direct typed
sound request using the cartridge library and Max6 capacity, defined in
PhantoonSoundDefinitions. Separate flame calls are preserved, and materialization
sound is no longer overwritten by a flame produced in the same frame.

The regression confirms one real flame spawn plus its exact library/ID/capacity,
two independent subsequent flame calls, and preservation of the library-two
materialization sound. The fixture rejects gameplay cartridge reads. Resource
gates pass (PLM, queued-VRAM-DMA and enemy-visual; existing analyzer warnings
remain). This confirms the identified routing mismatch, not
the tester's unspecified event by ear; player confirmation remains pending.
