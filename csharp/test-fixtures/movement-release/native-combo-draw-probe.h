#include "native-bounded-cpu.h"

// Original particle animation + complete projectile/trail OAM output. Equipment
// changes while particles are alive; no inventory/room reload is synthesized.
int DiagnosticComboDraw(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"beam,camera,frame,count,types,oam,low,high\n");
  const int beams[]={1,2,4,8};
  for(int b=0;b<4;b++) for(int camera=0;camera<2;camera++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=0x1000|beams[b]; samus_power_bombs=2; hud_item_index=3;
    samus_x_pos=samus_y_pos=128; samus_pose_x_dir=8;
    layer1_x_pos=camera?80:0; layer1_y_pos=camera?64:0;
    ProbeRunBounded(0x90ccc0);
    for(int frame=0;frame<640;frame++) {
      samus_x_pos=128+frame%9; samus_y_pos=128+(frame/7)%9;
      if(frame==20) equipped_beams=0x1000|beams[(b+1)%4];
      nmi_frame_counter_word=frame;
      sfx_readpos[0]=sfx_writepos[0]=0; memset(sfx1_queue,0,16);
      for(int i=3;i>=0;i--) if(projectile_bomb_instruction_ptr[i]) {
        projectile_index=i*2;
        ProbeRunBoundedRegisters(0x900000|projectile_bomb_pre_instructions[i],0,i*2,0);
        if(projectile_bomb_instruction_ptr[i]) ProbeRunBounded(0x9381e9);
      }
      oam_next_ptr=0; memset(oam_ext,0,32);
      ProbeRunBounded(0x938254);
      fprintf(f,"%d,%d,%d,%04X,%04X%04X%04X%04X,%04X,",beams[b],camera,frame,
        projectile_counter,projectile_type[0],projectile_type[1],projectile_type[2],projectile_type[3],oam_next_ptr);
      for(int i=0;i<oam_next_ptr;i++) fprintf(f,"%02X",g_ram[0x370+i]);
      fprintf(f,",");
      for(int i=0;i<32;i++) fprintf(f,"%02X",g_ram[0x570+i]);
      fprintf(f,"\n");
    }
  }
  fclose(f); return 0;
}
