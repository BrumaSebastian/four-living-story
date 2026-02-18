using FourLivingStory.ApiService.Modules.Rewards.Events;

namespace FourLivingStory.ApiService.Modules.Rewards;

public sealed class RewardCalculator
{
    private static readonly (int BaseXp, int BaseGold, double DropChance)[] _tiers =
    [
        (25,  5,  0.02), // Easy
        (60,  12, 0.05), // Medium
        (120, 24, 0.10), // Hard
        (250, 50, 0.20), // Extreme
    ];

    private static readonly (string Rarity, int Weight)[] _rarityWeights =
    [
        ("Common",    60),
        ("Uncommon",  25),
        ("Rare",      10),
        ("Epic",       4),
        ("Legendary",  1),
    ];

    private static readonly int _totalWeight = _rarityWeights.Sum(r => r.Weight);

    public RewardResult Calculate(
        string difficulty,
        int? targetQuantity,
        int? actualQuantity,
        IReadOnlyList<(Guid Id, string Rarity)> itemPool)
    {
        var (baseXp, baseGold, dropChance) = difficulty switch
        {
            "Easy"    => _tiers[0],
            "Medium"  => _tiers[1],
            "Hard"    => _tiers[2],
            "Extreme" => _tiers[3],
            _         => _tiers[0],
        };

        var bonusXp = 0;
        var bonusGold = 0;

        if (targetQuantity > 0 && actualQuantity > targetQuantity)
        {
            var ratio = Math.Min((double)(actualQuantity.Value - targetQuantity.Value) / targetQuantity.Value, 0.5);
            bonusXp   = (int)Math.Floor(baseXp   * ratio);
            bonusGold = (int)Math.Floor(baseGold * ratio);
        }

        var droppedItemId = RollItemDrop(dropChance, itemPool);

        return new RewardResult(baseXp, bonusXp, baseXp + bonusXp, baseGold + bonusGold, droppedItemId);
    }

    public RewardResult CalculateFixed(int baseXp, int baseGold) =>
        new(baseXp, 0, baseXp, baseGold, null);

    private static Guid? RollItemDrop(double dropChance, IReadOnlyList<(Guid Id, string Rarity)> itemPool)
    {
        if (itemPool.Count == 0 || Random.Shared.NextDouble() >= dropChance)
            return null;

        var roll = Random.Shared.Next(_totalWeight);
        var cumulative = 0;
        string? rarity = null;

        foreach (var (r, w) in _rarityWeights)
        {
            cumulative += w;
            if (roll < cumulative)
            {
                rarity = r;
                break;
            }
        }

        var candidates = itemPool.Where(i => i.Rarity == rarity).ToList();
        return candidates.Count > 0
            ? candidates[Random.Shared.Next(candidates.Count)].Id
            : null;
    }
}

public sealed record RewardResult(
    int BaseXp,
    int BonusXp,
    int TotalXp,
    int Gold,
    Guid? DroppedItemId);
