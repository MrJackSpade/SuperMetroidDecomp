// #444: connected input / collision / frozen-support replay on the original CPU.
// Include after native-release-probe.h. MOV1 and documented actor metadata are private.
int DiagnosticFrozenCrawlerClimb(const char *rom, const char *seed_path, const char *output, int shoot) {
  if (shoot < 20 || shoot > 45) return 4;
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  size_t size = 0; uint8 *seed = ReadWholeFile(seed_path, &size);
  if (!seed || size < 128) { free(seed); return 5; }
  uint32 *w = (uint32 *)seed;
  if (w[0] != 0x31564f4d || w[1] != 0x91f8 || w[2] != 144 || w[3] != 80 ||
      size != 128 + w[2] * w[3] * 3 || w[6] != 1 || w[16] != 2) { free(seed); return 6; }
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  room_ptr = w[1]; room_width_in_blocks = w[2]; room_height_in_blocks = w[3]; room_size_in_blocks = w[2] * w[3] * 2;
  room_width_in_scrolls = w[2] / 16; room_height_in_scrolls = w[3] / 16;
  // FlatFloorMovementFixture retains Landing Site's explicit camera table, including
  // the five adjacent bytes copied by the retail room loader. Do not invent blue cells.
  memcpy(scrolls, RomFixedPtr(0x8f9283), 50);
  up_scroller = RomFixedPtr(0x8f91f8)[6];
  down_scroller = RomFixedPtr(0x8f91f8)[7];
  for (uint32 i = 0; i < w[2] * w[3]; i++) {
    level_data[i] = seed[128 + i * 3] | seed[129 + i * 3] << 8; BTS[i] = seed[130 + i * 3];
  }
  samus_x_pos = w[4] >> 16; samus_x_subpos = w[4]; samus_y_pos = w[5] >> 16; samus_y_subpos = w[5];
  samus_pose = samus_prev_pose = w[6]; samus_x_radius = w[7]; samus_y_radius = w[8];
  samus_x_base_speed = w[9] >> 16; samus_x_base_subspeed = w[9];
  samus_x_extra_run_speed = w[10] >> 16; samus_x_extra_run_subspeed = w[10];
  samus_x_accel_mode = w[11]; samus_has_momentum_flag = w[12]; samus_x_speed_divisor = w[13]; samus_x_decel_mult = w[14];
  equipped_items = w[15]; equipped_beams = w[16]; fx_type = w[17]; fx_y_pos = w[18]; lava_acid_y_pos = w[19];
  fx_liquid_options = w[20]; liquid_physics_type = w[21]; samus_anim_frame = w[22]; samus_anim_frame_timer = w[23]; samus_anim_frame_buffer = w[24];
  samus_y_speed = w[25] >> 16; samus_y_subspeed = w[25]; samus_y_dir = w[26];
  samus_total_x_speed = w[27] >> 16; samus_total_x_subspeed = w[27];
  extra_samus_x_displacement = w[28] >> 16; extra_samus_x_subdisplacement = w[28];
  extra_samus_y_displacement = w[29] >> 16; extra_samus_y_subdisplacement = w[29];
  samus_y_accel = w[30]; samus_y_subaccel = w[31]; free(seed);
  samus_pose_x_dir = samus_prev_pose_x_dir = samus_last_different_pose_x_dir = 8; samus_last_different_pose = 1;
  samus_prev_x_pos = samus_x_pos; samus_prev_y_pos = samus_y_pos;
  samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
  samus_health = samus_max_health = 99; samus_super_missiles = samus_max_super_missiles = 10; hud_item_index = 2;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20; button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  first_free_enemy_index = 64; enemy_index_to_shake = 0xffff;
  Enemy_FireZoomer *e = (Enemy_FireZoomer *)gEnemyData(0);
  e->base.enemy_ptr = 0xdcff; e->base.bank = 0xa3; e->base.x_pos = 128; e->base.y_pos = 120;
  e->base.x_width = e->base.y_height = 8; e->base.properties = 0x2800; e->base.extra_properties = 0x8000;
  e->base.health = 15; e->base.spritemap_pointer = 58638; e->base.current_instruction = 57956;
  e->base.instruction_timer = 0xffff; e->base.layer = 5; e->base.frame_counter = 1;
  e->fzr_var_B = 0xff80; e->fzr_var_F = 0xe7f2;
  FILE *f = fopen(output, "w"); if (!f) return 7;
  fprintf(f,"frame,input,x,y,pose,anim,timer,health,supers,cameraX,cameraY,ex,exsub,ey,eysub,ehp,frozen,flash,ai,list,listTimer,map,vx,vy,function,fallsub,fall,return,turn");
  for(int i=0;i<4;i++) fprintf(f,",support%d",i);
  for(int i=0;i<5;i++) fprintf(f,",shotType%d,shotX%d,shotY%d",i,i,i);
  fprintf(f,"\n");
  uint16 previous = 0;
  for(int frame=0;frame<500;frame++) {
    uint16 input = frame == 0 ? 0x40 : 0;
    if(frame==13) input |= 0x4000; if(frame==16) input |= 0x200;
    if(frame>=shoot && frame<shoot+4) input |= 0x40;
    if(frame>=60 && frame<110) input |= 0x280;
    joypad1_lastkeys=input; joypad1_newkeys=input & ~previous; previous=input;
    nmi_frame_counter_word=frame+1; nmi_frame_counter_byte=(uint8)(frame+1);
    memset(enemy_drawing_queue_sizes,0,16);
    RunAsmCode(0xa08eb6,0,0,0,0); RunAsmCode(0x90e695,0,0,0,0); RunAsmCode(0xa09785,0,0,0,0); RunAsmCode(0xa08fd4,0,0,0,0);
    samus_contact_damage_index=0; RunAsmCode(0x900000|samus_movement_handler,0,0,0,0);
    uint32 stages[]={0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0x9094ec,0xa09169,0xa08687};
    for(int i=0;i<9;i++) RunAsmCode(stages[i],0,0,0,0);
    fprintf(f,"%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u",frame,input,
      ((uint32)samus_x_pos<<16)|samus_x_subpos,((uint32)samus_y_pos<<16)|samus_y_subpos,samus_pose,samus_anim_frame,samus_anim_frame_timer,samus_health,samus_super_missiles,layer1_x_pos,layer1_y_pos,
      e->base.x_pos,e->base.x_subpos,e->base.y_pos,e->base.y_subpos,e->base.health,e->base.frozen_timer,e->base.flash_timer,e->base.ai_handler_bits,e->base.current_instruction,e->base.instruction_timer,e->base.spritemap_pointer,
      e->fzr_var_A,e->fzr_var_B,e->fzr_var_F,e->fzr_var_01,e->fzr_var_02,e->fzr_var_03,e->fzr_var_04);
    for(int i=0;i<4;i++) fprintf(f,",%u",enemy_index_colliding_dirs[i]);
    for(int i=0;i<5;i++) fprintf(f,",%u,%u,%u",projectile_type[i],projectile_x_pos[i],projectile_y_pos[i]);
    fprintf(f,"\n");
  }
  fclose(f); return 0;
}
