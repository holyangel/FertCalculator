using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FertCalculatorMaui.Messages
{
    /// <summary>
    /// Message sent when the unit type (metric/imperial) is changed
    /// </summary>
    public class UnitChangedMessage : ValueChangedMessage<bool>
    {
        public UnitChangedMessage(bool useImperialUnits) : base(useImperialUnits)
        {
        }
    }
}
