using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace FertCalculatorMaui.Messages;

/// <summary>
/// Message sent when a fertilizer should be added to the current mix
/// </summary>
public class AddFertilizerToMixMessage : ValueChangedMessage<Fertilizer>
{
    public AddFertilizerToMixMessage(Fertilizer fertilizer) : base(fertilizer)
    {
    }
}

/// <summary>
/// Message sent when fertilizers have been updated (added, edited, or deleted)
/// </summary>
public class FertilizersUpdatedMessage : ValueChangedMessage<Fertilizer>
{
    public FertilizersUpdatedMessage(Fertilizer fertilizer) : base(fertilizer)
    {
    }
}
