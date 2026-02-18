namespace FourLivingStory.ApiService.Modules.Rewards.Events;

public sealed record RewardGrantedEvent(
    Guid CharacterId,
    int BaseXp,
    int BonusXp,
    int TotalXp,
    int Gold,
    Guid? DroppedItemId);
