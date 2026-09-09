// #472: seeded hurt or inert-projectile contact, then the full movement sequence.
// Include after native-release-probe.h. Dispatch before SDL initialization.
int DiagnosticDamageBoostSource(const char *rom, const char *output, int medium, int release, int contact) {
  if (medium < 0 || medium > 2 || release < 0 || release > 1 || contact < 0 || contact > 3) return 5;
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "timer,ball,left,source,forward,delay,frame,input,x,y,pose,animation,hurtTimer,hurtDirection,ySpeed,yDirection,baseSpeed,extraSpeed,previousPose,previousMetadata,olderPose,olderMetadata,medium,release,contact,health\n");
  for (int timerCase = 0; timerCase < (contact ? 1 : 2); timerCase++)
  for (int ball = 0; ball < 2; ball++)
  for (int left = 0; left < 2; left++)
  for (int source = 0; source < 2; source++)
  for (int forward = 0; forward < 2; forward++)
  for (int delay = 0; delay < 12; delay++) {
    int timer = timerCase || contact >= 2 ? 10 : 5;
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 16; x++) level_data[x] = level_data[16 * 16 + x] = 0x8000;
    for (int y = 0; y <= 16; y++) level_data[y * 16] = level_data[y * 16 + 15] = 0x8000;
    if (contact == 2) {
      int block = (source ? 9 : 10) * 16 + 8;
      level_data[block] = 0x2000; BTS[block] = 2;
    }
    if (contact == 3) {
      for (int x = 1; x < 15; x++) {
        level_data[11 * 16 + x] = 0xa000; BTS[11 * 16 + x] = source;
      }
    }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    if (medium == 1) { fx_y_pos = 8; fx_type = 6; }
    if (medium == 2) { lava_acid_y_pos = 8; fx_type = 2; }
    equipped_items = 4; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 160;
    if (contact == 3) samus_y_pos = samus_prev_y_pos = ball ? 169 : 155;
    samus_pose = samus_prev_pose = ball ? (left ? 0x41 : 0x1d) : (left ? 2 : 1);
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = ball ? 4 : 0;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    uint16 previous = forward ? (left ? 0x200 : 0x100) : 0;
    joypad1_lastkeys = previous; joypad1_newkeys = previous;
    if (!contact) { samus_knockback_timer = timer; knockback_x_dir = source; }
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    RunAsmCode(0x90ec22, 0, 0, 0, 0);
    if (!contact) {
      RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
    }
    for (int frame = -1; frame < 30; frame++) {
      uint16 input = previous;
      if (frame >= 0) {
        input = frame >= delay ? (left ? 0x100 : 0x200) | 0x80 : 0;
        // Release directional travel after three boost-input frames, retaining Jump.
        if (release && frame >= delay + 3) input = 0x80;
        samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
        samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
        joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
        RunAsmCode(0x90ec22, 0, 0, 0, 0); RunAsmCode(0x90e90f, 0, 0, 0, 0);
        RunAsmCode(0x909c5b, 0, 0, 0, 0);
        if (contact == 2) RunAsmCode(0x949b60, 0, 0, 0, 0);
        RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
        RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
        RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
        RunAsmCode(0x90eab3, 0, 0, 0, 0);
        RunAsmCode(0x90e9ce, 0, 0, 0, 0);
        if (contact == 1 && !frame) {
          // Inert projectile remains at its frame-start position; real overlap scan,
          // radius guards, touch instruction and hurt publication all run on CPU.
          eproj_id[0] = 0x9642; eproj_properties[0] = 20; eproj_radius[0] = 0x0808;
          eproj_x_pos[0] = source ? 120 : 136; eproj_y_pos[0] = 160;
          RunAsmCode(0xa09894, 0, 0, 0, 0);
        }
        RunAsmCode(0xa09169, 0, 0, 0, 0);
      }
      fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X,%04X,%04X%04X,%04X,%04X%04X,%04X%04X,%04X,%04X,%04X,%04X,%d,%d,%d,%04X\n",
        timer, ball, left, source, forward, delay, frame, input,
        samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos, samus_pose, samus_anim_frame,
        samus_knockback_timer, knockback_dir, samus_y_speed, samus_y_subspeed, samus_y_dir,
        samus_x_base_speed, samus_x_base_subspeed, samus_x_extra_run_speed, samus_x_extra_run_subspeed,
        samus_prev_pose, *(uint16 *)&samus_prev_pose_x_dir, samus_last_different_pose, *(uint16 *)&samus_last_different_pose_x_dir, medium, release, contact, samus_health);
    }
  }
  fclose(f); return 0;
}

int DiagnosticDamageBoostVariant(const char *rom, const char *output, int medium, int release) {
  return DiagnosticDamageBoostSource(rom, output, medium, release, 0);
}

int DiagnosticDamageBoost(const char *rom, const char *output) {
  return DiagnosticDamageBoostVariant(rom, output, 0, 0);
}
