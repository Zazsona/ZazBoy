using System;

namespace ZazBoy.Console
{
    /// <summary>
    /// Class for handling the interrupt registers.
    /// </summary>
    public class InterruptHandler
    {
        public const ushort InterruptEnableRegister = 0xFFFF;
        public const ushort InterruptFlagRegister = 0xFF0F;
        public bool interruptMasterEnable;

        private MemoryMap memMap;

        public InterruptHandler(MemoryMap memMap)
        {
            this.memMap = memMap;
            interruptMasterEnable = true;
        }

        /// <summary>
        /// Sets whether the interrupt is enabled. Does not alter the IME or enabled state.
        /// </summary>
        /// <param name="interrupt">The interrupt to de/request</param>
        /// <param name="state">The new request state</param>
        public void SetInterruptRequested(InterruptType interrupt, bool state)
        {
            int bitPosition = (int)interrupt;
            byte interruptFlags = memMap.Read(InterruptFlagRegister);
            byte setFlags = SetBit(interruptFlags, bitPosition, state);
            memMap.Write(InterruptFlagRegister, setFlags);
        }

        /// <summary>
        /// Gets if the interrupt is requested in 0xFF0F
        /// </summary>
        /// <param name="interrupt">The interrupt type to check.</param>
        /// <returns>bool on requested.</returns>
        public bool IsInterruptRequested(InterruptType interrupt)
        {
            int bitPosition = (int)interrupt;
            byte interruptFlags = memMap.Read(InterruptFlagRegister);
            return GetBit(interruptFlags, bitPosition);
        }

        /// <summary>
        /// Sets whether the interrupt is enabled. Does not alter the IME.
        /// </summary>
        /// <param name="interrupt">The interrupt to toggle</param>
        /// <param name="enable">The new state</param>
        public void SetInterruptEnabled(InterruptType interrupt, bool enable)
        {
            int bitPosition = (int)interrupt;
            byte interruptEnables = memMap.Read(InterruptEnableRegister);
            byte setEnables = SetBit(interruptEnables, bitPosition, enable);
            memMap.Write(InterruptFlagRegister, setEnables);
        }

        /// <summary>
        /// Gets if the interrupt is enabled in 0xFFFF. This does not check the IME.
        /// </summary>
        /// <param name="interrupt">The interrupt type to check.</param>
        /// <returns>bool on enabled.</returns>
        public bool IsInterruptEnabled(InterruptType interrupt)
        {
            int bitPosition = (int)interrupt;
            byte interruptEnabled = memMap.Read(InterruptEnableRegister);
            return GetBit(interruptEnabled, bitPosition);
        }

        /// <summary>
        /// Sets the bit at the specified position for the given byte
        /// </summary>
        /// <param name="data">The byte containing the bit</param>
        /// <param name="bitPosition">The index of the bit (7-0)</param>
        /// <param name="set">True to set the bit to 1, false to set the bit to 0</param>
        /// <returns>The updated byte</returns>
        private byte SetBit(byte data, int bitPosition, bool set)
        {
            byte flagBit = (byte)(1 << bitPosition);
            if (set)
                return (byte)(data | flagBit); //Data or Flag Bit (Flag bit is always 1, forces a set)
            else
                return (byte)(data & ~flagBit); //Data AND (NOT Flag Bit) (Flag bit is always 0, forces unset)
        }

        /// <summary>
        /// Gets the bit at the specified position for the given byte
        /// </summary>
        /// <param name="data">The byte containing the bit</param>
        /// <param name="bitPosition">The index of the bit (7-0)</param>
        /// <returns>True if bit is 1, false is bit is 0</returns>
        private bool GetBit(byte data, int bitPosition)
        {
            byte flagBit = (byte)(1 << bitPosition);
            return ((data & flagBit) != 0); //Test bit is true
        }

        /// <summary>
        /// Gets the instruction address to jump to for the specified interrupt type.
        /// </summary>
        /// <param name="interrupt">The interrupt to get the address for</param>
        /// <returns>The address to execute from</returns>
        public byte GetInterruptJumpAddress(InterruptType interrupt)
        {
            switch (interrupt)
            {
                case InterruptType.VBlank:
                    return 0x40;
                case InterruptType.LCDStatus:
                    return 0x48;
                case InterruptType.Timer:
                    return 0x50;
                case InterruptType.Serial:
                    return 0x58;
                case InterruptType.Joypad:
                    return 0x60;
                default:
                    return 0x00;
            }
        }

        /// <summary>
        /// Gets the highest priority interrupt that is enabled and requested.<br></br>
        /// If the IME is disabled, this will always return InterruptType.None.
        /// </summary>
        /// <returns>The interrupt type to execute, or None if there are no valid options.</returns>
        public InterruptType GetActivePriorityInterrupt()
        {
            if (interruptMasterEnable)
            {
                for (int i = 0; i<5; i++)
                {
                    InterruptType interruptType = (InterruptType)i;
                    if (IsInterruptEnabled(interruptType) && IsInterruptRequested(interruptType))
                        return interruptType;
                }
            }
            return InterruptType.None;
        }

        /// <summary>
        /// The types of interrupt
        /// </summary>
        public enum InterruptType
        {
            VBlank,
            LCDStatus,
            Timer,
            Serial,
            Joypad,
            None
        }
    }
}
