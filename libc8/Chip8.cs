using System;

namespace c8alpha
{
    static class Constants
    {
        public const int VRegCount = 16;
        public const int RamSizeBytes = 4096;
        public const int ScreenWidth = 64;
        public const int ScreenHeight = 32;
        public const int StackSize = 12;
        public const ushort SpriteFontLocation = 0x010;
        public static byte[] FontData = {
            0xF0, 0x90, 0x90, 0x90, 0xF0, //0
            0x20, 0x60, 0x20, 0x20, 0x70, //1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, //2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, //3
            0x90, 0x90, 0xF0, 0x10, 0x10, //4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, //5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, //6
            0xF0, 0x10, 0x20, 0x40, 0x40, //7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, //8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, //9
            0xF0, 0x90, 0xF0, 0x90, 0x90, //A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, //B
            0xF0, 0x80, 0x80, 0x80, 0xF0, //C
            0xE0, 0x90, 0x90, 0x90, 0xE0, //D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, //E
            0xF0, 0x80, 0xF0, 0x80, 0x80  //F;
        };
    }

    public class Chip8
    {
        private byte timerDelay;
        private byte timerSound;

        private byte[] V;
        private ushort I;
        private ushort PC;
        private byte[] RAM;
        private ushort[] stack;
        private byte stackPointer = 0;

        public bool[,] FrameBuffer;
        private Random r;

        public Chip8()
        {
            r = new Random();
            FrameBuffer = new bool[Constants.ScreenWidth, Constants.ScreenHeight];
            RAM = new byte[Constants.RamSizeBytes];
            stack = new ushort[Constants.StackSize];
            V = new byte[Constants.VRegCount];
            I = 0;
            timerDelay = 0;
            timerSound = 0;

            InitSpriteFont(Constants.SpriteFontLocation);

        }
        public void LoadRAM(byte[] ramData, ushort addr=0x200)
        {
            if(ramData.Length + addr > Constants.RamSizeBytes)
            {
                Console.WriteLine("RAM data exceeds memeory bounds, cancelling load.");
                return;
            }
            PC = addr;
            ramData.CopyTo(RAM, addr);
        }
        private void InitSpriteFont(ushort addr)
        {
            Constants.FontData.CopyTo(RAM, addr);
        }

        private void Push(ushort addr)
        {
            this.stack[stackPointer++] = addr;
        }

        private ushort Pop()
        {
            return this.stack[--stackPointer];
        }

        public void ClockCycle()
        {
            ushort addr;
            byte x, y, n, value;
            ushort opcode = (ushort)(RAM[PC++] << 8 | RAM[PC++]);
            byte nibble = (byte)(opcode >> 12);
            addr = (ushort)(opcode & 0x0FFF);
            value = (byte)(opcode & 0x00FF);
            n = (byte)(opcode & 0x000F);
            x = (byte)((opcode & 0x0F00) >> 8);
            y = (byte)((opcode & 0x00F0) >> 4);
#if DEBUG
            Console.WriteLine("Executing next instruction: ");
            Console.WriteLine("\tOpCode: 0x" + Convert.ToString(opcode, 16));
            if(x < Constants.VRegCount) Console.WriteLine("\tx: 0x" + Convert.ToString(x, 16) + " Vx: " + V[x].ToString());
            if (y < Constants.VRegCount) Console.WriteLine("\ty: 0x" + Convert.ToString(y, 16) + " Vy: " + V[y].ToString());
            Console.WriteLine("\tN: " + n.ToString());
            Console.WriteLine("\tNN: " + n.ToString());
            Console.WriteLine("\tAddress: 0x" + Convert.ToString(addr, 16));
#endif
            switch (nibble)
            {
                case 0x0:
                    if (n == 0)
                    {
                        ClearScreen();
                    }
                    else
                    {
                        ReturnFromSub();
                    }
                    break;
                case 0x1:
                    JumpToAddr(addr);
                    break;
                case 0x2:
                    ExecuteSub(addr);
                    break;
                case 0x3:
                    SkipIfVxEqual(x, value);
                    break;
                case 0x4:
                    SkipIfVxNotEqual(x, value);
                    break;
                case 0x5:
                    SkipIfVxEqualVy(x, y);
                    break;
                case 0x6:
                    StoreInVx(x, value);
                    break;
                case 0x7:
                    SkipIfVxNotEqualVy(x, y);
                    break;
                case 0x8:
                    switch (n) 
                    {
                        case 0:
                            StoreVyInVx(x, y);
                            break;
                        case 1:
                            StoreVxOrVy(x, y);
                            break;
                        case 2:
                            StoreVxAndVy(x, y);
                            break;
                        case 3:
                            StoreVxXorVy(x, y);
                            break;
                        case 4:
                            AddVyToVxWithCarry(x, y);
                            break;
                        case 5:
                            SubtractVyFromVx(x, y);
                            break;
                        case 6:
                            RshVyToVx(x, y);
                            break;
                        case 7:
                            SubtrackVxFromVy(x, y);
                            break;
                        case 0xE:
                            LshVyToVx(x, y);
                            break;
                        default:
                            throw new Exception("Unknown Opcode!");
                    }
                    
                    break;
                case 0x9:
                    SkipIfVxNotEqualVy(x, y);
                    break;
                case 0xA:
                    StoreAddrInI(addr);
                    break;
                case 0xB:
                    JumpToAddrPlusV0(addr);
                    break;
                case 0xC:
                    RandomVx(x, value);
                    break;
                case 0xD:
                    DrawSprite(x, y, n);
                    break;
                case 0xE:
                    if (n == 0xE) SkipIfKeyPressed(x);
                    else if (n == 1) SkipIfKeyNotPressed(x);
                    break;
                case 0xF:
                    switch(value)
                    {
                        case 0x07:
                            StoreDelayToVx(x);
                            break;
                        case 0x0A:
                            WaitForKey(x);
                            break;
                        case 0x15:
                            SetDelayTimer(x);
                            break;
                        case 0x18:
                            SetSoundTimer(x);
                            break;
                        case 0x1E:
                            AddVxToI(x);
                            break;
                        case 0x29:
                            SetIToSpriteData(x);
                            break;
                        case 0x33:
                            StoreBinaryDecimal(x);
                            break;
                        case 0x55:
                            StoreRegistersToRAM(x);
                            break;
                        case 0x65:
                            LoadRegistersFromRam(x);
                            break;
                        default:
                            throw new Exception("Unknown Opcode!");
                    }
                    break;
                default:
                    throw new Exception("Unknown Opcode!");
            }
        }

        private void Execute() //0NNN
        {
#if DEBUG
            Console.WriteLine("\tRunning execute (does nothing)");
#endif
        }

        private void ClearScreen() //00E0
        {
#if DEBUG
            Console.WriteLine("\tClearing scree");
#endif
            this.FrameBuffer.Initialize();

        }

        private void ReturnFromSub() //00EE
        {
#if DEBUG
            Console.WriteLine("\tReturn from sub");
#endif
            this.PC = Pop();
        }

        private void JumpToAddr(ushort addr) //1NNN
        {
#if DEBUG
            Console.WriteLine("\tJump to addr:" + Convert.ToString(addr, 16));
#endif
            this.PC = addr;
        }
        private void ExecuteSub(ushort addr) //2NNN
        {
#if DEBUG
            Console.WriteLine("\tExecute sub at " + Convert.ToString(addr, 16));
#endif
            Push(this.PC);
            this.PC = addr;
        }

        private void SkipIfVxEqual(byte x, byte value) //3XNN
        {
#if DEBUG
            Console.WriteLine("\tSkip if Vx = Val ");
#endif
            if (V[x] == value) SkipOpcode();
        }

        private void SkipIfVxNotEqual(byte x, byte value) //4XNN
        {
#if DEBUG
            Console.WriteLine("\tSkip if Vx != Val ");
#endif
            if (V[x] != value) SkipOpcode();
        }
        private void SkipIfVxEqualVy(byte x, byte y) //5XY0
        {
#if DEBUG
            Console.WriteLine("\tSkip if Vx = Vy ");
#endif
            if (V[x] == V[y]) SkipOpcode();
        }
        private void StoreInVx(byte x, byte value) //6XNN
        {
#if DEBUG
            Console.WriteLine("\tStore in Vx ");
#endif
            V[x] = value;
        }
        private void AddToVx(byte x, byte value) //7XNN
        {
#if DEBUG
            Console.WriteLine("\tAdd to in Vx ");
#endif
            V[x] += value;
        }
        private void StoreVyInVx(byte x, byte y) //8XY0
        {
#if DEBUG
            Console.WriteLine("\tStore in Vy ");
#endif
            V[x] = V[y];
        }
        private void StoreVxOrVy(byte x, byte y) //8XY1
        {
#if DEBUG
            Console.WriteLine("\tVx = Vx | Vy");
#endif
            V[x] = (byte)(V[x] | V[y]);
        }
        private void StoreVxAndVy(byte x, byte y) //8XY2
        {
#if DEBUG
            Console.WriteLine("\tVx = Vx & Vy");
#endif
            V[x] = (byte)(V[x] & V[y]);
        }
        private void StoreVxXorVy(byte x, byte y) //8XY3
        {
#if DEBUG
            Console.WriteLine("\tVx = Vx ^ Vy");
#endif
            V[x] = (byte)(V[x] ^ V[y]);
        }
        private void AddVyToVxWithCarry(byte x, byte y) //8XY4
        {
#if DEBUG
            Console.WriteLine("\tAdd with carry");
#endif
            int result = V[x] + V[y];
            if (result > 256)
            {
                V[0xF] = 1;
#if DEBUG
                Console.WriteLine("\tCarry flag set");
#endif
            }
            V[x] = (byte)result;
        }

        private void SubtractVyFromVx(byte x, byte y) //8XY5
        {
#if DEBUG
            Console.WriteLine("\tSubtract Vy from Vx");
#endif
            if (V[x] > V[y])
            {
#if DEBUG
                Console.WriteLine("\tBorrow flag set");
#endif
                V[0xF] = 1;
            }
            else
            {
                V[0xF] = 0;
#if DEBUG
                Console.WriteLine("\tBorrow flag NOT set");
#endif
            }
            V[x] -= V[y];
        }

        private void RshVyToVx(byte x, byte y) //8XY6
        {
#if DEBUG
            Console.WriteLine("\tRight shift");
#endif
            if (V[y] << 7 != 0) V[0xF] = 1;
            else V[0xF] = 0;

            V[x] = (byte)(V[y] >> 1);
        }

        private void SubtrackVxFromVy(byte x, byte y) //8XY7
        {
#if DEBUG
            Console.WriteLine("\tSubtract Vx from Vy");
#endif
            if (V[x] < V[y])
            {
#if DEBUG
                Console.WriteLine("\tBorrow flag set");
#endif
                V[0xF] = 1;
            }
            else
            {
                V[0xF] = 0;
#if DEBUG
                Console.WriteLine("\tBorrow flag NOT set");
#endif
            }
            V[x] = (byte)(V[y] - V[x]);
        }
        private void LshVyToVx(byte x, byte y) //8XYE
        {
#if DEBUG
            Console.WriteLine("\tLeft shift");
#endif
            if (V[y] >> 7 != 0) V[0xF] = 1;
            else V[0xF] = 0;

            V[x] = (byte)(V[y] << 1);
        }
        private void SkipIfVxNotEqualVy(byte x, byte y) //9XY0
        {
#if DEBUG
            Console.WriteLine("\tSkip if Vx != Vy");
#endif
            if (V[x] != V[y]) SkipOpcode();
        }

        private void StoreAddrInI(ushort addr) //ANNN
        {
#if DEBUG
            Console.WriteLine("\tSet addr into I");
#endif
            I = addr;
        }

        private void JumpToAddrPlusV0(ushort addr) //BNNN
        {
#if DEBUG
            Console.WriteLine("\tJump to V0 + addr, result is " + Convert.ToString(V[0x0] + addr, 16));
#endif
            PC = (byte)(addr + V[0x0]);
        }

        private void RandomVx(byte x, byte value) //CXNN
        {
#if DEBUG
            Console.WriteLine("\tSetting Vx to random number");
#endif
            V[x] = (byte)(r.Next(0, 256) & value);
        }
        /// <summary>
        /// Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. Each row of 8 pixels is read as bit-coded 
        /// starting from memory location I; I value doesn’t change after the execution of this instruction. As described above, VF is set to 1 if 
        /// any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="bytesOfSpriteData"></param>
        private void DrawSprite(byte x, byte y, byte bytesOfSpriteData) //DXYN
        {
#if DEBUG
            Console.WriteLine("\tDraw sprite");
            Console.WriteLine("\t\tSprite is " + bytesOfSpriteData + "b tall");
#endif
            bool[] pixelData;
            bool switchedOffPixel = false;
            for (int i = 0; i < bytesOfSpriteData; i++)
            {
                pixelData = ByteToBools(RAM[I + i]);
                for(int j = 0; j<8; j++)
                {
                    if (switchedOffPixel == false && FrameBuffer[(V[x] + j) % Constants.ScreenWidth, (V[y] + i) % Constants.ScreenHeight] == true && pixelData[j] == true)
                    {
                        switchedOffPixel = true;
                    }
                    FrameBuffer[(V[x]+ j)%Constants.ScreenWidth, (V[y] + i)%Constants.ScreenHeight] ^= pixelData[j];
                }
            }
            if (switchedOffPixel) V[0xF] = 1;
            else V[0xF] = 0;
        }

        private bool[] ByteToBools(byte b)
        {
            bool[] result = new bool[8];
            for(int i = 0; i<8; i++)
            {
                result[7-i] = (b & (1 << i)) != 0;
            }
            return result;
        }
        private void SkipIfKeyPressed(byte x) //EX9E
        {
#if DEBUG
            Console.WriteLine("\tSkip if key pressed");
#endif
        }
        private void SkipIfKeyNotPressed(byte x) //EXA1
        {
#if DEBUG
            Console.WriteLine("\tSkip if key NOT pressed");
#endif
        }
        private void StoreDelayToVx(byte x) //FX07
        {
#if DEBUG
            Console.WriteLine("\tStore delay timer");
#endif
            this.V[x] = this.timerDelay;
        }
        private void WaitForKey(byte x) //FX0A
        {
#if DEBUG
            Console.WriteLine("\tWait for key");
#endif
        }
        private void SetDelayTimer(byte x) //FX15
        {
#if DEBUG
            Console.WriteLine("\tSet delay timer");
#endif
            this.timerDelay = this.V[x];
        }
        private void SetSoundTimer(byte x) //FX18
        {
#if DEBUG
            Console.WriteLine("\tSet sound timer");
#endif
            this.timerSound = this.V[x];
        }
        private void AddVxToI(byte x) //FX1E
        {
#if DEBUG
            Console.WriteLine("\tOffset I by Vx");
#endif
            this.I += this.V[x];
        }
        private void SetIToSpriteData(byte x) //FX29
        {
#if DEBUG
            Console.WriteLine("\tSet I to font address");
#endif
            this.I = (ushort)(Constants.SpriteFontLocation + (x * 5));
        }
        private void StoreBinaryDecimal(byte x) //FX33
        {
#if DEBUG
            Console.WriteLine("\tStore binary deciaml");
#endif
            RAM[I] = (byte)((V[x] / 100) % 10);
            RAM[I+1] = (byte)((V[x] / 10) % 10);
            RAM[I+2] = (byte)((V[x]) % 10);
        }
        private void StoreRegistersToRAM(byte x) //FX55
        {
#if DEBUG
            Console.WriteLine("\tDump registers to RAM");
#endif
            for (int i = 0; i<=x; i++)
            {
                this.RAM[this.I] = this.V[i];
                this.I++;
            }
        }
        private void LoadRegistersFromRam(byte x) //FX65
        {
#if DEBUG
            Console.WriteLine("\tLoad registers from RAM");
#endif
            for (int i = 0; i <= x; i++)
            {
                this.V[i] = this.RAM[this.I];
                this.I++;
            }
        }

        private void SkipOpcode()
        {
            this.PC += 2;
        }
        public void DumpRegisters()
        {
            Console.WriteLine("===================");
            Console.WriteLine(" PC: 0x" + Convert.ToString(PC, 16));
            Console.WriteLine(" I: 0x" + Convert.ToString(I, 16));
            Console.WriteLine(" SP: 0x" + Convert.ToString(stackPointer, 16));
            Console.WriteLine("===================");
            for (int i = 0; i < Constants.VRegCount; i++)
            {
                Console.WriteLine(" V" + Convert.ToString(i, 16) + ": 0x" + Convert.ToString(V[i], 16));
            }
            Console.WriteLine("===================");
            Console.WriteLine(" DT: 0x" + Convert.ToString(timerDelay, 16));
            Console.WriteLine(" ST: 0x" + Convert.ToString(timerSound, 16));
            Console.WriteLine("===================");
        }
    }
}
