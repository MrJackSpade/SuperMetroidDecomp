#include "native-bounded-cpu.h"

// #406: original-CPU direct G-Mode gate traversal and PLM re-enable timing.
// Kronic Boost ($8F:AE74) provides a retail blue downward gate. Room setup and
// two PLM frames run before direct G-Mode disables the handler and fills the
// 40-slot PLM pool. The wave shot crosses the gate; $84:83AD models the exact
// EnablePLMs call made by the X-Ray teardown on the selected frame.
int DiagnosticGModeGate(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "reactivateFrame,passedGateFrame,activationFrame,finalX,finalY,plmFlag,activePlms\n");

  const uint8 *state = RomFixedPtr(0x8fae81);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  uint16 population = state[20] | state[21] << 8;
  const int reactivation_frames[] = {-1, 4, 5};
  for (int test = 0; test < 3; test++) {
    int reactivate_frame = reactivation_frames[test];
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 32; room_height_in_blocks = 48;
    room_width_in_scrolls = 2; room_height_in_scrolls = 3;
    room_size_in_blocks = 3072; area_index = 2;
    DecompressToMem(level, g_ram + 0x10000);
    memcpy(BTS, (uint8 *)level_data + 3072, 1536);
    interactive_enemy_indexes[0] = 0xffff;
    samus_pose = samus_prev_pose = 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = 4;
    samus_movement_type = samus_prev_movement_type = 0;
    samus_x_pos = samus_prev_x_pos = 140;
    samus_y_pos = samus_prev_y_pos = 360;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
    samus_anim_frame_timer = 5; samus_health = samus_max_health = 99;
    hud_item_index = 0; equipped_beams = 1;
    button_config_shoot_x = 0x40; button_config_run_b = 0x8000;
    layer1_y_pos = 224; plm_flag = 0x8000;
    for (uint16 entry = population; ; entry += 6) {
      uint32 address = 0x8f0000 | entry;
      if (!GET_WORD(RomFixedPtr(address))) break;
      ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
    }
    int gate = -1;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
    if (gate < 0) { fclose(f); return 8; }
    ProbeRunBounded(0x8485b4); ProbeRunBounded(0x8485b4);
    if (plm_pre_instrs[gate] != 0xbb6b || plm_instr_list_ptrs[gate] != 0xbc44) {
      fclose(f); return 9;
    }

    plm_flag = 0;
    time_is_frozen_flag = 0;
    demo_input_instr_timer = 5;
    for (int i = 39; i >= 0; i--) {
      if (plm_header_ptr[i]) continue;
      plm_header_ptr[i] = 0xb974;
      plm_pre_instrs[i] = 0x853d;
      plm_instr_list_ptrs[i] = 0xaae3;
      plm_instruction_timer[i] = 1;
      plm_block_indices[i] = plm_block_indices[gate];
    }

    int passed_frame = -1, activation_frame = -1;
    uint16 previous = 0;
    for (int frame = 0; frame < 20; frame++) {
      uint16 input = frame == 0 ? 0x40 : 0;
      joypad1_lastkeys = input;
      joypad1_newkeys = input & ~previous;
      previous = input;
      if (frame == reactivate_frame) ProbeRunBounded(0x8483ad);
      ProbeRunBounded(0x90ac1c);
      ProbeRunBounded(0x90b80d);
      ProbeRunBounded(0x90aece);
      if (projectile_type[0] && projectile_x_pos[0] < 112 && passed_frame < 0)
        passed_frame = frame;
      if (plm_flag & 0x8000) ProbeRunBounded(0x8485b4);
      if (plm_pre_instrs[gate] == 0xbba3 &&
          plm_instr_list_ptrs[gate] == 0xbc51 && activation_frame < 0)
        activation_frame = frame;
      vram_write_queue_tail = 0;
    }
    int active = 0;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i]) active++;
    fprintf(f, "%d,%d,%d,%d,%d,%04X,%d\n", reactivate_frame,
      passed_frame, activation_frame, projectile_x_pos[0],
      projectile_y_pos[0], plm_flag, active);
  }
  fclose(f); return 0;
}
