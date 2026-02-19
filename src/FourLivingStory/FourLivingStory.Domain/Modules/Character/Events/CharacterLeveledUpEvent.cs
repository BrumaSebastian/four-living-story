namespace FourLivingStory.Domain.Modules.Character.Events;

public sealed record CharacterLeveledUpEvent(Guid CharacterId, int OldLevel, int NewLevel);
