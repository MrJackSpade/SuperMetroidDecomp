// This file is deliberately tiny. Super Metroid's actual SPC music driver, SFX
// sequencer, BRR decoder, ADSR, echo, and mixer remain in the pinned, verbatim
// upstream sources vendored below this project. The bridge owns only allocation
// validation and a stable C ABI for .NET.

#include <stdint.h>
#include <stddef.h>
#include <stdlib.h>

#include "spc_player.h"

#if defined(_WIN32)
#define SM_AUDIO_EXPORT __declspec(dllexport)
#else
#define SM_AUDIO_EXPORT
#endif

// The desktop host requests 48 kHz stereo. dsp_getSamples resamples the native
// 534-sample (~32.04 kHz) SNES frame to this many host frames without changing the
// emulated sequencer's one-call-per-NMI timing.
enum { kHostSamplesPerVideoFrame = 800 };

SM_AUDIO_EXPORT SpcPlayer *sm_audio_create(void) {
  SpcPlayer *player = SpcPlayer_Create();
  if (player != NULL)
    SpcPlayer_Initialize(player);
  return player;
}

SM_AUDIO_EXPORT void sm_audio_destroy(SpcPlayer *player) {
  if (player == NULL)
    return;
  dsp_free(player->dsp);
  free(player);
}

SM_AUDIO_EXPORT int sm_audio_upload(SpcPlayer *player, const uint8_t *data, int length) {
  if (player == NULL || data == NULL || length < 2)
    return 0;

  // Cartridge upload streams contain repeated little-endian (length,target,data)
  // records and terminate with a zero length word. Upstream's faithful routine trusts
  // ROM; this public boundary must reject truncated buffers before handing them to it.
  int offset = 0;
  for (;;) {
    if (offset > length - 2)
      return 0;
    int byte_count = data[offset] | (data[offset + 1] << 8);
    offset += 2;
    if (byte_count == 0)
      break;
    if (offset > length - 2 || byte_count > length - offset - 2)
      return 0;
    offset += 2 + byte_count;
  }

  SpcPlayer_Upload(player, data);
  return 1;
}

SM_AUDIO_EXPORT int sm_audio_write_port(SpcPlayer *player, int port, uint8_t value) {
  if (player == NULL || (unsigned)port >= 4)
    return 0;
  player->input_ports[port] = value;
  return 1;
}

SM_AUDIO_EXPORT int sm_audio_read_port(SpcPlayer *player, int port) {
  if (player == NULL || (unsigned)port >= 4)
    return -1;
  return player->port_to_snes[port];
}

SM_AUDIO_EXPORT int sm_audio_generate_frame(
    SpcPlayer *player,
    int16_t *interleaved_stereo,
    int stereo_frame_count) {
  if (player == NULL || interleaved_stereo == NULL || stereo_frame_count <= 0)
    return 0;

  SpcPlayer_GenerateSamples(player);
  dsp_getSamples(player->dsp, interleaved_stereo, stereo_frame_count);
  return stereo_frame_count;
}

SM_AUDIO_EXPORT int sm_audio_default_samples_per_frame(void) {
  return kHostSamplesPerVideoFrame;
}
