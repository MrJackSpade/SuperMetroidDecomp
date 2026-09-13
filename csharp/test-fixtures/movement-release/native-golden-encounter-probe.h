#include "native-bounded-cpu.h"

// Original game-state-eight execution, with only initial state and physical
// controller words supplied. No boss decisions or projectile outcomes injected.
int DiagnosticGoldenEncounter(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
  g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
  room_ptr=0xb283;
  ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
  ProbeRunBounded(0xa08a1e); ProbeRunBounded(0xa08a9e);
  if(num_enemies_in_room!=1) { fclose(f); return 8; }
  fx_y_pos=lava_acid_y_pos=0xffff;
  equipped_items=collected_items=1; equipped_beams=collected_beams=0;
  game_state=8; reg_INIDISP=15;
  samus_health=samus_max_health=9999;
  samus_missiles=samus_max_missiles=100;
  samus_super_missiles=samus_max_super_missiles=99; hud_item_index=2;
  samus_x_pos=samus_prev_x_pos=384; samus_y_pos=samus_prev_y_pos=395;
  layer1_x_pos=256; layer1_y_pos=229;
  samus_pose=samus_prev_pose=1; samus_pose_x_dir=samus_prev_pose_x_dir=8;
  samus_anim_frame_timer=1; samus_x_speed_table_pointer=0x9f55;
  samus_input_handler=0xe913; samus_movement_handler=0xa337; grapple_beam_function=0xc4f0;
  frame_handler_alfa=0xe695; frame_handler_beta=0xe725;
  frame_handler_gamma=0xe90e; samus_draw_handler=0xeb52;
  button_config_run_b=0x8000; button_config_jump_a=0x80; button_config_shoot_x=0x40;
  button_config_aim_up_R=0x10; button_config_aim_down_L=0x20;
  button_config_itemcancel_y=0x4000; button_config_itemswitch=0x2000;
  hdma_objects_enable_flag=0x8000;
  ProbeRunBounded(0x868000); ProbeRunBounded(0x89ab82);
  random_number=0x1234;
  fprintf(f,"frame,input,x,y,subX,subY,health,flash,list,timer,function,pre,vx,vy,gravity,turn,flags,random,samusX,samusY,samusHealth,pose\n");
  uint16 previous=0;
  for(int frame=0;frame<3000;frame++) {
    nmi_frame_counter_word=nmi_frame_counter_byte=frame+1;
    uint16 input=frame==60?0x200:frame>=500 && frame%20==0?0x40:0;
    joypad1_lastkeys=input; joypad1_newkeys=input&~previous; previous=input;
    oam_next_ptr=vram_write_queue_tail=vram_read_queue_tail=0;
    ProbeRunBounded(0x8884b9); ProbeRunBounded(0x808111); ProbeRunBounded(0x828b44);
    Enemy_Torizo *e=Get_Torizo(0);
    fprintf(f,"%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n",
      frame,input,e->base.x_pos,e->base.y_pos,e->base.x_subpos,e->base.y_subpos,
      e->base.health,e->base.flash_timer,e->base.current_instruction,e->base.instruction_timer,
      e->toriz_var_E,e->toriz_var_F,e->toriz_var_A,e->toriz_var_B,e->toriz_var_C,e->toriz_var_03,
      e->toriz_parameter_2,random_number,samus_x_pos,samus_y_pos,samus_health,samus_pose);
  }
  fclose(f); return 0;
}
