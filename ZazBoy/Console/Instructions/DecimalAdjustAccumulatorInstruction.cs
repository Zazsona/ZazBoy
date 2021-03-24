using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy.Console.Instructions
{
    /// <summary>
    /// Instruction for retroactively performing a BCD (Binary-coded decimal) add/sub.<br></br>
    /// BCD being a form of binary representation where (in this case) each nibble is used to represent a base 10 digit (0-9). E.g, 1001 0000 == 90 (1001 == 9, 0000 == 0)
    /// 
    /// Credit to AJW on NesDev for explaining this instruction: https://forums.nesdev.com/viewtopic.php?t=15944
    /// </summary>
    public class DecimalAdjustAccumulatorInstruction : Instruction
    {
        public DecimalAdjustAccumulatorInstruction() : base(0x00, 0x27, 4)
        {

        }

        protected override void Execute()
        {
            CPU cpu = GameBoy.Instance().CPU;
            if (cpu.subtractionFlag)
            {
                if (cpu.carryFlag)
                    cpu.registerA -= 0x60; //As 9 is the new max value, make it so that 9 is the point at which a carry occurred (15-6 == 9)
                if (cpu.halfCarryFlag)
                    cpu.registerA -= 0x06; //Same as above, applied to lower nibble.
            }
            else
            {
                if (cpu.carryFlag || cpu.registerA > 0x99) //If we're above BCD's maximum expressible value...
                {
                    cpu.registerA += 0x60;  //As with sub, 9 is the overflow point, so add 6 to align with standard binary. (1001(9) + 0110(6) = 1111(15))
                    cpu.carryFlag = true;
                }
                if (cpu.halfCarryFlag || ((cpu.registerA & 0x0F) > 0x09)) //If the lower nibble is beyond BCD's maximum for a single digit (9)...
                    cpu.registerA += 0x06;  //Set it as a binary overflow
                    //This would, in theory, set a half carry, but the instruction forces that to false. Presumably because in BCD this isn't a "half", but a digit?
            }
            cpu.zeroFlag = cpu.registerA == 0;
            cpu.halfCarryFlag = false;
        }
    }
}
