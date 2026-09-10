#include "native-bounded-cpu.h"

// Replay the recorded body/camera trajectory through the ORIGINAL CPU arm AI
// and instruction interpreter. This isolates arm animation/anchor visibility;
// the input's body phase timing is a stimulus, not an independently verified oracle.
static void ArmWrite(int address, uint16 value) { *(uint16 *)(g_ram+address)=value; }
static uint16 ArmRead(int address) { return *(uint16 *)(g_ram+address); }
int DiagnosticKraidArm(const char *rom, const char *input, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *in=fopen(input,"r"); if(!in) return 4;
  FILE *out=fopen(output,"wx"); if(!out) { fclose(in); return 4; }
  char line[512];
  if(!fgets(line,sizeof(line),in)) return 5;
  fprintf(out,"frame,phase,bodyx,bodyy,camerax,cameray,armx,army,properties,instruction,map,timer\n");
  cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
  g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
  cur_enemy_index=64;
  ArmWrite(0xfa6+64,0xa7); // Arm instruction bank.
  g_ram[0x1786]=0xa7; // Enemy-main dispatcher seeds the long instruction callback bank.
  int frame,bx,by,cx,cy,ax,ay,timer,count=0,mismatch=0;
  unsigned phase,properties,instruction,map,previousPhase=0;
  while(fgets(line,sizeof(line),in)) {
    if(sscanf(line,"%d,%x,%d,%d,%d,%d,%d,%d,%x,%x,%x,%d",
      &frame,&phase,&bx,&by,&cx,&cy,&ax,&ay,&properties,&instruction,&map,&timer)!=12) return 5;
    if(frame==-1) {
      ArmWrite(0xf86+64,properties); ArmWrite(0xf92+64,instruction);
      ArmWrite(0xf8e+64,map); ArmWrite(0xf94+64,timer);
      continue;
    }
    // DeathInitialize disables touch; initialization/fade and sink install these
    // real cartridge lists. Run their contents rather than copying recorded maps.
    if(frame==0) ArmWrite(0xf86+64,ArmRead(0xf86+64)|0x0400);
    if(phase!=previousPhase && phase==0xc3f9) {
      ArmWrite(0xf92+64,0x8af0); ArmWrite(0xf94+64,1);
    }
    if(phase!=previousPhase && phase==0xc537) {
      ArmWrite(0xf92+64,0x8aa4); ArmWrite(0xf94+64,1);
    }
    // End of sinking deletes the population; comparison covers the live arm only.
    if(phase==0xc715) break;
    ArmWrite(0xf7a,bx); ArmWrite(0xf7e,by);
    layer1_x_pos=cx; layer1_y_pos=cy;
    ProbeRunBoundedRegisters(0xa7b7bd,0,64,0);
    ProbeRunBoundedRegisters(0xa0c26a,0,64,0);
    unsigned nx=ArmRead(0xf7a+64),ny=ArmRead(0xf7e+64),np=ArmRead(0xf86+64);
    unsigned ni=ArmRead(0xf92+64),nm=ArmRead(0xf8e+64),nt=ArmRead(0xf94+64);
    fprintf(out,"%d,%04X,%d,%d,%d,%d,%u,%u,%04X,%04X,%04X,%u\n",
      frame,phase,bx,by,cx,cy,nx,ny,np,ni,nm,nt);
    if(nx!=ax || ny!=ay || np!=properties || ni!=instruction || nm!=map || nt!=timer) {
      fprintf(stderr,"Arm mismatch frame %d: native=%04X/%04X/%04X/%04X/%04X/%04X expected=%04X/%04X/%04X/%04X/%04X/%04X\n",
        frame,nx,ny,np,ni,nm,nt,ax,ay,properties,instruction,map,timer);
      mismatch++;
    }
    previousPhase=phase; count++;
  }
  fclose(in); fclose(out);
  printf("Original-CPU arm comparison: %d frames, %d mismatches.\n",count,mismatch);
  return count==0 || mismatch ? 7 : 0;
}
