// #442: narrow, two-actor original-CPU comparison after the real Ice setup.
// Include after native-release-probe.h. Generated seeds remain private.
int DiagnosticZebetiteSkip(const char *rom, const char *movement, const char *actors, const char *output, int offset) {
  // -1 selects the longer controller-earned approach: twenty frames right,
  // one frame left, then twenty-four Jump frames separated by one release.
  bool approach = offset == -1;
  if (!approach && offset != 0 && offset != 8 && offset != 20) return 4;
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  size_t size = 0; uint8 *seed = ReadWholeFile(movement, &size);
  if (!seed || size != 3200) { free(seed); return 5; }
  uint32 *w = (uint32 *)seed;
  if (w[0] != 0x31564f4d || w[1] != 0xdd58 || w[2] != 64 || w[3] != 16 || (w[6] != 4 && w[6] != 0x28) || w[16] != 2) { free(seed); return 6; }
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  room_ptr = w[1]; room_width_in_blocks = w[2]; room_height_in_blocks = w[3]; room_size_in_blocks = w[2] * w[3] * 2;
  room_width_in_scrolls = 4; room_height_in_scrolls = 1;
  for (int i = 0; i < 4; i++) scrolls[i] = 1;
  up_scroller = RomFixedPtr(0x8fdd58)[6]; down_scroller = RomFixedPtr(0x8fdd58)[7];
  for (uint32 i = 0; i < w[2] * w[3]; i++) { level_data[i] = seed[128+i*3] | seed[129+i*3]<<8; BTS[i] = seed[130+i*3]; }
  samus_x_pos = w[4]>>16; samus_x_subpos = w[4]; samus_y_pos = w[5]>>16; samus_y_subpos = w[5];
  samus_pose = w[6]; samus_x_radius = w[7]; samus_y_radius = w[8];
  samus_x_base_speed = w[9]>>16; samus_x_base_subspeed = w[9]; samus_x_extra_run_speed = w[10]>>16; samus_x_extra_run_subspeed = w[10];
  samus_x_accel_mode = w[11]; samus_has_momentum_flag = w[12]; samus_x_speed_divisor = w[13]; samus_x_decel_mult = w[14];
  equipped_items = w[15]; equipped_beams = w[16]; fx_type = w[17]; fx_y_pos = w[18]; lava_acid_y_pos = w[19];
  fx_liquid_options = w[20]; liquid_physics_type = w[21]; samus_anim_frame = w[22]; samus_anim_frame_timer = w[23]; samus_anim_frame_buffer = w[24];
  samus_y_speed = w[25]>>16; samus_y_subspeed = w[25]; samus_y_dir = w[26]; samus_total_x_speed = w[27]>>16; samus_total_x_subspeed = w[27];
  extra_samus_x_displacement = w[28]>>16; extra_samus_x_subdisplacement = w[28]; extra_samus_y_displacement = w[29]>>16; extra_samus_y_subdisplacement = w[29];
  samus_y_accel = w[30]; samus_y_subaccel = w[31]; free(seed);
  uint8 *supplement = ReadWholeFile(actors, &size);
  if (!supplement || size != 152 || memcmp(supplement, "ZSK1", 4)) { free(supplement); return 7; }
  uint16 *a = (uint16 *)(supplement + 4);
  uint16 first_nmi = a[0]; random_number = a[1];
  samus_prev_pose = a[2]; samus_prev_pose_x_dir = a[3]; samus_prev_movement_type = a[3]>>8;
  samus_last_different_pose = a[4]; samus_last_different_pose_x_dir = a[5]; samus_prev_movement_type2 = a[5]>>8;
  samus_pose_x_dir = RomFixedPtr(0x91b629 + 8 * samus_pose)[0];
  samus_movement_type = RomFixedPtr(0x91b629 + 8 * samus_pose)[1];
  samus_health = samus_max_health = a[6]; samus_invincibility_timer = a[7];
  layer1_x_pos = ideal_layer1_xpos = a[8]; layer1_y_pos = ideal_layer1_ypos = a[9];
  memcpy(gEnemyData(128), supplement + 24, 64); memcpy(gEnemyData(192), supplement + 88, 64); free(supplement);
  if (gEnemyData(128)->enemy_ptr != 0xe27f || gEnemyData(192)->enemy_ptr != 0xd23f || !gEnemyData(192)->frozen_timer) return 8;
  samus_prev_x_pos = samus_x_pos; samus_prev_y_pos = samus_y_pos;
  samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20; button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  first_free_enemy_index = 256; enemy_index_to_shake = 0xffff;
  FILE *f = fopen(output, "w"); if (!f) return 9;
  fprintf(f,"frame,input,x,y,pose,anim,timer,xradius,yradius,health,frozen\n"); uint16 previous = 0x840;
  for (int frame = 120; frame < (approach ? 220 : 160); frame++) {
    uint16 input = approach
      ? (frame < 140 ? 0x100 : frame == 140 ? 0x200 : 0x200 | ((frame-141)%25 < 24 ? 0x80 : 0))
      : (frame < 120 + offset ? 0x100 : 0x200 | ((frame - 120 - offset)%36 < 24 ? 0x80 : 0));
    joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
    nmi_frame_counter_word = first_nmi + frame - 119; nmi_frame_counter_byte = (uint8)nmi_frame_counter_word;
    memset(enemy_drawing_queue_sizes,0,16);
    RunAsmCode(0x808111,0,0,0,0); RunAsmCode(0xa08eb6,0,0,0,0); RunAsmCode(0x90e695,0,0,0,0);
    RunAsmCode(0xa09785,0,0,0,0); RunAsmCode(0xa08fd4,0,0,0,0);
    samus_contact_damage_index = 0; RunAsmCode(0x900000|samus_movement_handler,0,0,0,0);
    uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0x9094ec,0xa09169,0xa08687};
    for (int i=0;i<9;i++) RunAsmCode(stages[i],0,0,0,0);
    fprintf(f,"%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n",frame,input,((uint32)samus_x_pos<<16)|samus_x_subpos,
      ((uint32)samus_y_pos<<16)|samus_y_subpos,samus_pose,samus_anim_frame,samus_anim_frame_timer,samus_x_radius,samus_y_radius,samus_health,gEnemyData(192)->frozen_timer);
  }
  fclose(f); return 0;
}
