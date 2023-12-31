using System;

namespace MyNamespace.Input.Enums
{
    [Flags]
    public enum ButtonState
    {
        None = 0,
        Pressed = 0b001,
        Held = 0b010,
        PressedOrHeld = Pressed | Held,
        Released = 0b100,
    }
}