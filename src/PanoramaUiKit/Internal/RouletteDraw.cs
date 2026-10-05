using PanoramaUiKit.Shared.Roulette;

namespace PanoramaUiKit.Internal;

/// <summary>The weighted draw behind a roulette: the winner, and the filler cards around it.</summary>
internal static class RouletteDraw
{
    /// <summary>
    /// One item index per card of the strip. Card <paramref name="winnerCell"/> holds the winner, the rest
    /// are drawn with the same weights, so a common item shows up often on the strip and a rare one rarely.
    /// Null when no item has a weight above zero.
    /// </summary>
    public static int[]? Strip(IReadOnlyList<RouletteItem> items, int cells, int winnerCell, Random random, out int winner)
    {
        winner = -1;
        double total = 0;
        foreach (RouletteItem item in items)
        {
            total += Math.Max(0f, item.Weight);
        }

        if (total <= 0)
        {
            return null;
        }

        winner = Pick(items, total, random);
        int[] strip = new int[cells];
        for (int i = 0; i < cells; i++)
        {
            strip[i] = Pick(items, total, random);
        }

        strip[winnerCell] = winner;
        return strip;
    }

    private static int Pick(IReadOnlyList<RouletteItem> items, double total, Random random)
    {
        double roll = random.NextDouble() * total;
        int last = 0;
        for (int i = 0; i < items.Count; i++)
        {
            double weight = Math.Max(0f, items[i].Weight);
            if (weight <= 0)
            {
                continue;
            }

            last = i;
            roll -= weight;
            if (roll < 0)
            {
                return i;
            }
        }

        // Rounding left a sliver of the roll over: the last item that can be drawn.
        return last;
    }
}
