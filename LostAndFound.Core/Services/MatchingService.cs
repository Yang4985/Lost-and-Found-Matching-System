using LostAndFound.Core.Models;

namespace LostAndFound.Core.Services
{
    public class MatchingService
    {
        public int CalculateMatchScore(
            LostItemReport lostReport,
            FoundItemReport foundReport)
        {
            if (lostReport == null || foundReport == null)
            {
                return 0;
            }

            if (lostReport.Item == null || foundReport.Item == null)
            {
                return 0;
            }

            int score = 0;

            // Category is the most important field.
            // Different categories should not be recommended as matches.
            if (!TextMatches(
                lostReport.Item.GetCategory(),
                foundReport.Item.GetCategory()))
            {
                return 0;
            }

            score += 30;

            // Brand = 15 points
            if (TextMatches(
                lostReport.Item.GetBrand(),
                foundReport.Item.GetBrand()))
            {
                score += 15;
            }

            // Colour = 15 points
            if (TextMatches(
                lostReport.Item.GetColor(),
                foundReport.Item.GetColor()))
            {
                score += 15;
            }

            // Location = 15 points
            if (TextMatches(
                lostReport.LocationLost,
                foundReport.LocationFound))
            {
                score += 15;
            }

            // Date = 15 points
            int dayDifference = Math.Abs(
                (lostReport.DateLost.Date - foundReport.DateFound.Date).Days
            );

            if (dayDifference <= 1)
            {
                score += 15;
            }

            // Description / distinguishing features = 10 points
            if (DescriptionsAreSimilar(
                lostReport.Item.GetDescription(),
                foundReport.Item.GetDescription()))
            {
                score += 10;
            }

            return score;
        }


        public string GetMatchLevel(int score)
        {
            if (score >= 80)
            {
                return "High";
            }

            if (score >= 60)
            {
                return "Medium";
            }

            if (score >= 40)
            {
                return "Low";
            }

            return "Not Recommended";
        }


        private bool TextMatches(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first) ||
                string.IsNullOrWhiteSpace(second))
            {
                return false;
            }

            return string.Equals(
                first.Trim(),
                second.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }


        private bool DescriptionsAreSimilar(
            string lostDescription,
            string foundDescription)
        {
            if (string.IsNullOrWhiteSpace(lostDescription) ||
                string.IsNullOrWhiteSpace(foundDescription))
            {
                return false;
            }

            string[] lostWords = lostDescription
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string[] foundWords = foundDescription
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string lostWord in lostWords)
            {
                // Ignore short/common words
                if (lostWord.Length < 4)
                {
                    continue;
                }

                if (foundWords.Contains(lostWord))
                {
                    return true;
                }
            }

            return false;
        }
    }
}