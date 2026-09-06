// Console-only original-SPC-driver probe for #54. Include from sm_rtl.c.
// Inputs are extracted cartridge upload streams; this file contains no ROM data.
#include "snes/apu.h"
#ifdef _WIN32
__declspec(dllimport) unsigned int __stdcall SetErrorMode(unsigned int mode);
#endif

static bool ProbeLoadSpcStream(Apu *apu, const char *path) {
  size_t length = 0, cursor = 0;
  uint8 *data = ReadWholeFile(path, &length);
  if (!data) return false;
  while (cursor + 4 <= length) {
    unsigned count = data[cursor] | data[cursor + 1] << 8;
    unsigned address = data[cursor + 2] | data[cursor + 3] << 8;
    cursor += 4;
    if (!count) { free(data); return true; }
    if (count > length - cursor) break;
    for (unsigned i = 0; i < count; i++) apu->ram[(address + i) & 0xffff] = data[cursor + i];
    cursor += count;
  }
  free(data);
  fprintf(stderr, "Malformed SPC upload: %s\n", path);
  return false;
}

int DiagnosticSpcCpu(const char *engine, const char *music, bool tickTrace) {
#ifdef _WIN32
  SetErrorMode(1 | 2);
#endif
  Apu *apu = apu_init();
  apu_reset(apu);
  if (!ProbeLoadSpcStream(apu, engine) || !ProbeLoadSpcStream(apu, music)) {
    apu_free(apu);
    return 2;
  }
  // Execute initialization from the uploaded driver's entry, not the IPL upload
  // handshake. Commands wait a full second for initialization to finish.
  apu->spc->pc = 0x1500;
  if (tickTrace) {
    int target = 0x15c4, tick = 0;
    uint8 elapsed = 0;
    // These are the same two synchronization PCs used by the upstream SPC
    // translation comparator. Bound cycles as well as ticks to catch a stuck CPU.
    for (unsigned cycle = 0; cycle < 20000000 && tick < 3000; cycle++) {
      apu_cycle(apu);
      if (apu->dsp->sampleOffset == 534) apu->dsp->sampleOffset = 0;
      if (apu->spc->pc != target) continue;
      target ^= 0x15c4 ^ 0x15c5;
      if (target != 0x15c4) continue;
      printf("T %d %u ", tick, elapsed);
      for (int reg = 0; reg < 128; reg++) printf("%02X", apu->dsp->ram[reg]);
      printf(" ");
      for (int port = 0; port < 4; port++) printf("%02X", apu->ram[port]);
      printf("\n");
      elapsed = apu->spc->y;
      if (tick == 500) apu->inPorts[0] = 5;
      if (tick == 1800) apu->inPorts[1] = 8;
      if (tick == 2300) { apu->inPorts[1] = 2; apu->inPorts[2] = 0x71; apu->inPorts[3] = 1; }
      tick++;
      apu->hist.count = 0;
    }
    apu_free(apu);
    if (tick != 3000) { fprintf(stderr, "SPC tick trace timed out at %d\n", tick); return 4; }
    return 0;
  }
  bool heard = false;
  for (int frame = 0; frame < 600; frame++) {
    if (frame == 60) apu->inPorts[0] = 5;
    if (frame == 240) apu->inPorts[1] = 8; // Sustained Charge Beam, library one.
    // $82:BE17's cancellation messages, after four seconds of ordinary music.
    if (frame == 300) { apu->inPorts[1] = 2; apu->inPorts[2] = 0x71; apu->inPorts[3] = 1; }
    apu->hist.count = 0;
    while (apu->dsp->sampleOffset < 534) {
      apu_cycle(apu);
      if (apu->spc->stopped) {
        fprintf(stderr, "SPC stopped at frame %d PC=%04X\n", frame, apu->spc->pc);
        apu_free(apu);
        return 3;
      }
    }
    long long energy = 0;
    int peak = 0;
    for (int i = 0; i < 534 * 2; i++) {
      int value = apu->dsp->sampleBuffer[i];
      energy += (long long)value * value;
      int magnitude = value < 0 ? -value : value;
      if (magnitude > peak) peak = magnitude;
    }
    if (!heard && peak) { printf("FIRST_PCM frame=%d\n", frame); heard = true; }
    if (frame % 30 == 0 || (frame >= 298 && frame <= 315)) {
      printf("SPC frame=%d pc=%04X ports=%02X,%02X,%02X,%02X peak=%d mean-square=%lld FLG=%02X NON=%02X EON=%02X SRC=[",
        frame, apu->spc->pc, apu->outPorts[0], apu->outPorts[1], apu->outPorts[2], apu->outPorts[3],
        peak, energy / (534 * 2), apu->dsp->ram[0x6c], apu->dsp->ram[0x3d], apu->dsp->ram[0x4d]);
      for (int voice = 0; voice < 8; voice++) printf("%s%02X", voice ? "," : "", apu->dsp->ram[voice * 16 + 4]);
      printf("]\n");
    }
    apu->dsp->sampleOffset = 0;
  }
  apu_free(apu);
  return 0;
}

int DiagnosticDspSample(const char *engine, const char *music, unsigned source) {
#ifdef _WIN32
  SetErrorMode(1 | 2);
#endif
  if (source > 255) return 2;
  Apu *apu = apu_init();
  apu_reset(apu);
  if (!ProbeLoadSpcStream(apu, engine) || !ProbeLoadSpcStream(apu, music)) {
    apu_free(apu); return 2;
  }
  Dsp *dsp = apu->dsp;
  dsp_write(dsp, 0x6c, 0x20); // Echo writes disabled, no reset/mute.
  dsp_write(dsp, 0x5d, 0x6d); // Resident sample directory.
  dsp_write(dsp, 0x0c, 0x7f); dsp_write(dsp, 0x1c, 0x7f);
  dsp_write(dsp, 0x00, 0x7f); dsp_write(dsp, 0x01, 0x7f);
  dsp_write(dsp, 0x02, 0); dsp_write(dsp, 0x03, 0x10);
  dsp_write(dsp, 0x04, (uint8)source);
  dsp_write(dsp, 0x05, 0); dsp_write(dsp, 0x07, 0x7f);
  dsp_write(dsp, 0x5c, 0); dsp_write(dsp, 0x4c, 1);
  for (int frame = 0; frame < 128; frame++) {
    while (dsp->sampleOffset < 534) dsp_cycle(dsp);
    uint32 hash = 2166136261u;
    for (int i = 0; i < 534 * 2; i++) hash = (hash ^ (uint16)dsp->sampleBuffer[i]) * 16777619u;
    printf("S %d %08X\n", frame, hash);
    dsp->sampleOffset = 0;
  }
  apu_free(apu);
  return 0;
}
