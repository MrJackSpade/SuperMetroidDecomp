#include "native-bounded-cpu.h"

static int GreenGateLoadRetailRom(const char *rom) {
  if (!SnesInit(rom)) return 2;
  size_t rom_length = 0;
  uint8 *retail = ReadWholeFile(rom, &rom_length);
  size_t header = rom_length & 0x7fff;
  if (!retail || (header != 0 && header != 512) ||
      rom_length - header > g_snes->cart->romSize) {
    free(retail);
    return 3;
  }
  memcpy(g_snes->cart->rom, retail + header, rom_length - header);
  free(retail);
  return 0;
}

// #405: original-CPU Green Hill Zone Grapple/Speed Booster gate matrix.
// The center row is the published successful setup; the surrounding rows are
// fixed-point controls which distinguish the cartridge's four-probe result from
// the incorrect "cancel on the first solid substep" interpretation.
int DiagnosticGreenGateGrapple(const char *rom, const char *output) {
  int status = GreenGateLoadRetailRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "startX,baseSpeed,opened,openFrame,samusX,samusSubX,samusY,pose,grappleX,grappleY,grappleFunction\n");

  static const struct { uint16 x, base_speed; } candidates[] = {
    { 1663, 1 }, { 1664, 1 }, { 1664, 2 }, { 1665, 1 }, { 1665, 2 },
  };
  const uint8 *state = RomFixedPtr(0x8f9e5f);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  uint16 population = state[20] | state[21] << 8;
  for (int candidate = 0; candidate < 5; candidate++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = 0x9e52;
    room_width_in_blocks = 128; room_height_in_blocks = 64;
    room_width_in_scrolls = 8; room_height_in_scrolls = 4;
    room_size_in_blocks = 128 * 64 * 2; area_index = 1;
    DecompressToMem(level, g_ram + 0x10000);
    memcpy(BTS, (uint8 *)level_data + 128 * 64 * 2, 128 * 64);
    interactive_enemy_indexes[0] = 0xffff;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    layer1_x_pos = 1472; layer1_y_pos = 768; plm_flag = 0x8000;
    for (uint16 entry = population; ; entry += 6) {
      uint32 address = 0x8f0000 | entry;
      if (!GET_WORD(RomFixedPtr(address))) break;
      ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
    }
    int gate = -1;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
    if (gate < 0) { fclose(f); return 8; }
    ProbeRunBounded(0x8485b4); ProbeRunBounded(0x8485b4);

    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = candidates[candidate].x;
    samus_x_subpos = samus_prev_x_subpos = 0;
    samus_y_pos = samus_prev_y_pos = 939;
    samus_y_subpos = samus_prev_y_subpos = 0;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_pose = samus_prev_pose = 0x0c;
    samus_pose_x_dir = samus_prev_pose_x_dir = 4;
    samus_movement_type = samus_prev_movement_type = 1;
    samus_x_base_speed = candidates[candidate].base_speed;
    samus_x_base_subspeed = 0;
    samus_x_extra_run_speed = 4; samus_x_extra_run_subspeed = 0;
    samus_has_momentum_flag = 1; speed_boost_counter = 0x400;
    samus_contact_damage_index = 1;
    equipped_items = 0x6000; hud_item_index = 4;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    button_config_shoot_x = 0x40; button_config_up = 0x800;
    button_config_down = 0x400; button_config_left = 0x200;
    button_config_right = 0x100; button_config_aim_up_R = 0x10;
    button_config_aim_down_L = 0x20; button_config_itemcancel_y = 0x4000;
    button_config_itemswitch = 0x2000;

    uint16 previous = 0;
    int open_frame = -1;
    for (int frame = 0; frame < 21; frame++) {
      uint16 input = 0x280 | (frame >= 1 ? 0x40 : 0);
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      ProbeRunBounded(0x90e695); samus_contact_damage_index = 1;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      ProbeRunBounded(0x908000); ProbeRunBounded(0x90dde9);
      ProbeRunBounded(0x91e8b6); ProbeRunBounded(0x91eb88);
      ProbeRunBounded(0x90eab3); ProbeRunBounded(0x90e9ce);
      ProbeRunBounded(0x8485b4); vram_write_queue_tail = 0;
      if (plm_timers[gate]) { open_frame = frame; break; }
    }
    fprintf(f, "%u,%u,%d,%d,%u,%u,%u,%02X,%u,%u,%04X\n",
      candidates[candidate].x, candidates[candidate].base_speed,
      open_frame >= 0, open_frame, samus_x_pos, samus_x_subpos,
      samus_y_pos, samus_pose, grapple_beam_end_x_pos,
      grapple_beam_end_y_pos, grapple_beam_function);
  }
  fclose(f); return 0;
}
