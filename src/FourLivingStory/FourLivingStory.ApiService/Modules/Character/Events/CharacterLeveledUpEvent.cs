namespace FourLivingStory.ApiService.Modules.Character.Events;

public sealed record CharacterLeveledUpEvent(Guid CharacterId, int OldLevel, int NewLevel);
