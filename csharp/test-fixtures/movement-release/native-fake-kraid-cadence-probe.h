#include "native-bounded-cpu.h"

// Both sides of the actual room, with the same room collision planes and a
// prescribed changing RNG input. Other enemies and Samus contact are excluded.
int DiagnosticFakeKraidCadence(const char *rom,const char *level,const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *in=fopen(level,"rb"); if(!in) return 4;
  uint16 width,height,words[4096]; uint8 bts[4096];
  if(fread(&width,2,1,in)!=1 || fread(&height,2,1,in)!=1 ||
     width*height>4096 || fread(words,2,width*height,in)!=width*height ||
     fread(bts,1,width*height,in)!=width*height) { fclose(in); return 5; }
  fclose(in);
  FILE *out=fopen(output,"wx"); if(!out) return 4;
  fprintf(out,"side,frame,random,x,walk,facing,walktimer,spittimer,clock0,clock1,clock2,selector,instruction,map,timer,spikes,spit\n");
  for(int side=0;side<2;side++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=width; room_height_in_blocks=height;
    memcpy(level_data,words,width*height*2); memcpy(BTS,bts,width*height);
    cur_enemy_index=192; random_number=9;
    layer1_x_pos=0x500; samus_x_pos=side?0x560:0x500; samus_y_pos=0;
    Enemy_FakeKraid *e=Get_FakeKraid(192);
    e->base.enemy_ptr=0xe0ff; e->base.x_pos=0x530; e->base.y_pos=160;
    e->base.x_width=32; e->base.y_height=24; e->base.properties=0x2800;
    e->base.bank=0xa6; g_ram[0x1786]=0xa6; eproj_enable_flag=0x8000;
    ProbeRunBoundedRegisters(0xa69a58,0,192,0);
    for(int frame=0;frame<2400;frame++) {
      random_number=(uint16)(9+frame*37);
      uint16 ids[18]; memcpy(ids,eproj_id,sizeof(ids));
      ProbeRunBoundedRegisters(0xa69ac2,0,192,0);
      ProbeRunBoundedRegisters(0xa0c26a,0,192,0);
      int spikes=0,spit=0;
      for(int i=0;i<18;i++) if(!ids[i] && eproj_id[i]) {
        if(eproj_id[i]==0x9db0) spit++;
        else if(eproj_id[i]==0x9dbe || eproj_id[i]==0x9dcc) spikes++;
        else { fclose(out); return 6; }
      }
      fprintf(out,"%d,%d,%u,%u,%d,%d,%u,%u,%u,%u,%u,%u,%04X,%04X,%u,%d,%d\n",
        side,frame,random_number,e->base.x_pos,(int16)e->fkd_var_B,(int16)e->fkd_var_C,
        e->fkd_var_D,e->fkd_var_E,e->fkd_var_03,e->fkd_var_04,e->fkd_var_05,e->fkd_var_07,
        e->base.current_instruction,e->base.spritemap_pointer,e->base.instruction_timer,spikes,spit);
      ProbeRunBoundedRegisters(0x868104,0,0,0);
    }
  }
  fclose(out); printf("Original-CPU Mini-Kraid cadence: 4800 frames captured.\n"); return 0;
}
