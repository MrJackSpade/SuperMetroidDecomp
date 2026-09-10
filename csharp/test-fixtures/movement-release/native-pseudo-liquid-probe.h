#include "native-bounded-cpu.h"

// #421: one real movement-handler call at both sides of the top-surface boundary.
// Charge is seeded at this handler boundary; input-earned retention is tested separately.
int DiagnosticPseudoLiquid(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f,"pose,anim,charge,gravity,medium,offset,disabled,contact\n");
  const uint16 poses[] = {0x19,0x1a,0x81,0x82,0x83,0x84,0x4d,0x4e};
  const uint16 frames[] = {0,2,3,22,23}, charges[] = {59,60,120};
  for(int p=0;p<8;p++) for(int a=0;a<5;a++) for(int c=0;c<3;c++)
  for(int gravity=0;gravity<2;gravity++) for(int medium=0;medium<4;medium++)
  for(int offset=-1;offset<=1;offset++) for(int disabled=0;disabled<2;disabled++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16; room_size_in_blocks=512;
    interactive_enemy_indexes[0]=0xffff;
    samus_x_pos=samus_prev_x_pos=128; samus_y_pos=samus_prev_y_pos=128;
    samus_pose=samus_prev_pose=poses[p];
    samus_pose_x_dir=samus_prev_pose_x_dir=(p&1)?4:8;
    samus_movement_type=samus_prev_movement_type2=p<4?3:p<6?0x14:2;
    samus_anim_frame=frames[a]; samus_anim_frame_timer=2;
    samus_x_speed_table_pointer=0x9f55; samus_y_dir=2;
    equipped_items=gravity?0x20:0; samus_suit_palette_index=gravity?4:0;
    flare_counter=charges[c]; joypad1_lastkeys=button_config_jump_a=0x80;
    grapple_beam_function=0xc4f0;
    ProbeRunBounded(0x90ec22);
    uint16 surface=128-samus_y_radius+offset;
    fx_y_pos=medium==1?surface:0xffff;
    lava_acid_y_pos=medium>=2?surface:0xffff;
    fx_type=medium==1?6:medium==2?2:medium==3?4:0;
    fx_liquid_options=disabled?4:0;
    ProbeRunBounded(p<4?0x90a436:p<6?0x90a734:0x90a42e);
    fprintf(f,"%02X,%d,%d,%d,%d,%d,%d,%04X\n",poses[p],frames[a],charges[c],gravity,medium,offset,disabled,samus_contact_damage_index);
  }
  fclose(f); return 0;
}
