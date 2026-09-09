// #413 diagnostic: execute untouched cartridge code in a bounded flat room.
// Include after native-release-probe.h; dispatch before SDL initialization.
// The bomb is constructed at the collision seam, not placed by a controller.
// This isolates hurt/bomb arbitration; it does not validate bomb placement/fuse.
int DiagnosticHurtBomb(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,bombFrame,jumpFrame,frame,input,x,y,pose,hurtTimer,hurtDirection,bombDirection,mover,ySpeed,yDirection,baseSpeed\n");
  for (int left = 0; left < 2; left++)
  for (int bombFrame = 0; bombFrame < 10; bombFrame++)
  for (int jumpFrame = 0; jumpFrame < 16; jumpFrame++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 16; x++) level_data[x] = level_data[16 * 16 + x] = 0x8000;
    for (int y = 0; y <= 16; y++) level_data[y * 16] = level_data[y * 16 + 15] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = 0x1004; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 160;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    RunAsmCode(0x90ec22, 0, 0, 0, 0);
    // Admit real enemy-projectile contact before the first traced movement.
    eproj_id[0] = 0x9642; eproj_properties[0] = 20; eproj_radius[0] = 0x0808;
    eproj_x_pos[0] = left ? 120 : 136; eproj_y_pos[0] = 160;
    RunAsmCode(0xa09894, 0, 0, 0, 0);
    RunAsmCode(0x90dde9, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
    uint16 previous = 0;
    for (int frame = 0; frame < 40; frame++) {
      uint16 input = frame >= jumpFrame ? (left ? 0x100 : 0x200) | 0x80 : 0;
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90ec22, 0, 0, 0, 0); RunAsmCode(0x90e90f, 0, 0, 0, 0);
      RunAsmCode(0x909c5b, 0, 0, 0, 0);
      if (frame == bombFrame) {
        bomb_counter = 1; projectile_type[5] = 0x500; projectile_damage[5] = 30;
        projectile_x_pos[5] = samus_x_pos; projectile_y_pos[5] = samus_y_pos;
        projectile_x_radius[5] = projectile_y_radius[5] = 8; projectile_variables[5] = 8;
        RunAsmCode(0xa09785, 0, 0, 0, 0);
        projectile_damage[5] = 0; bomb_counter = 0;
      }
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X,%04X,%04X,%04X%04X,%04X,%04X%04X\n",
        left, bombFrame, jumpFrame, frame, input, samus_x_pos, samus_x_subpos,
        samus_y_pos, samus_y_subpos, samus_pose, samus_knockback_timer, knockback_dir,
        bomb_jump_dir, samus_movement_handler, samus_y_speed, samus_y_subspeed,
        samus_y_dir, samus_x_base_speed, samus_x_base_subspeed);
    }
  }
  fclose(f); return 0;
}
