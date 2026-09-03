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
  free(player->reg_write_history);
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

// Temporary migration diagnostics. These expose read-only emulated state so the managed
// port can report the first exact divergence instead of merely saying that PCM differs.
SM_AUDIO_EXPORT int sm_audio_read_dsp_register(SpcPlayer *player, int address) {
  if (player == NULL || (unsigned)address >= 0x80)
    return -1;
  return dsp_read(player->dsp, (uint8_t)address);
}

SM_AUDIO_EXPORT int sm_audio_read_apu_ram(SpcPlayer *player, int address) {
  if (player == NULL || (unsigned)address >= 0x10000)
    return -1;
  return player->ram[address];
}

SM_AUDIO_EXPORT int sm_audio_begin_dsp_write_capture(SpcPlayer *player) {
  if (player == NULL)
    return 0;
  if (player->reg_write_history == NULL) {
    player->reg_write_history = (DspRegWriteHistory *)calloc(1, sizeof(DspRegWriteHistory));
    if (player->reg_write_history == NULL)
      return 0;
  }
  player->reg_write_history->count = 0;
  return 1;
}

SM_AUDIO_EXPORT int sm_audio_dsp_write_count(SpcPlayer *player) {
  if (player == NULL || player->reg_write_history == NULL)
    return -1;
  return (int)player->reg_write_history->count;
}

SM_AUDIO_EXPORT int sm_audio_dsp_write_address(SpcPlayer *player, int index) {
  if (player == NULL || player->reg_write_history == NULL ||
      (unsigned)index >= player->reg_write_history->count)
    return -1;
  return player->reg_write_history->addr[index];
}

SM_AUDIO_EXPORT int sm_audio_dsp_write_value(SpcPlayer *player, int index) {
  if (player == NULL || player->reg_write_history == NULL ||
      (unsigned)index >= player->reg_write_history->count)
    return -1;
  return player->reg_write_history->val[index];
}

SM_AUDIO_EXPORT int sm_audio_debug_value(SpcPlayer *player, int selector, int channel) {
  if (player == NULL || (unsigned)channel >= 8)
    return -1;
  Channel *c = &player->channel[channel];
  switch (selector) {
    case 0: return player->timer_cycles;
    case 1: return player->counter_sf0c;
    case 2: return player->music_ptr_toplevel;
    case 3: return player->fast_forward;
    case 4: return player->main_tempo_accum;
    case 5: return player->tempo;
    case 6: return player->block_count;
    case 7: return player->key_ON;
    case 8: return player->key_OFF;
    case 9: return player->cur_chan_bit;
    case 10: return player->is_chan_on;
    case 11: return c->pattern_order_ptr_for_chan;
    case 12: return c->note_ticks_left;
    case 13: return c->note_length;
    case 14: return c->instrument_id;
    case 15: return c->subroutine_num_loops;
    case 16: return c->saved_pattern_ptr;
    case 17: return c->pattern_start_ptr;
    case 18: return c->note_gate_off_fixedpt;
    case 19: return c->cutk;
    default: return -1;
  }
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
