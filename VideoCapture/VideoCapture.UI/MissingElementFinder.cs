using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VideoCapture.Common;

namespace VideoCapture.UI
{
    public class MissingElementFinder
    {
        private class MinimalRegionTag
        {
            public string DisplayText { get; set; }
            public double Confidence { get; set; }

            public override int GetHashCode()
            {
                return this.DisplayText.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return this.DisplayText.Equals(((MinimalRegionTag)obj).DisplayText);
            }
        }

        private static readonly string[] Tags =
        {
            "kuerbis",
            "vampir",
            "hexe",
            "geist",
            "fledermaus",
            "katze",
            "ritter",
            "eule"
        };

        private readonly List<MinimalRegionTag> currentRegionTags = new List<MinimalRegionTag>();
        public string MissingTag;

        public void AddRegionTags(IEnumerable<RegionTag> regionTags)
        {
            if (regionTags.Any())
            {
                this.currentRegionTags.AddRange(this.GetMinimalRegionTags(regionTags));
                this.CalculateMissingTag();
            }
            else
            {
                this.Reset();
            }
        }

        private List<MinimalRegionTag> GetMinimalRegionTags(IEnumerable<RegionTag> regionTags)
        {
            var result = regionTags.Select(r => new MinimalRegionTag { DisplayText = r.DisplayText, Confidence = r.Confidence }).ToList();

            // Add missing Tags with zero confidence
            result.AddRange(Tags
                .Where(t => !result.Any(r => r.DisplayText == t))
                .Select(t => new MinimalRegionTag { DisplayText = t, Confidence = 0 }));

            return result;
        }

        private void CalculateMissingTag()
        {
            var regionTags = this.currentRegionTags
                .GroupBy(r => r.DisplayText)
                .Select(grp => new MinimalRegionTag { DisplayText = grp.Key, Confidence = grp.Average(x => x.Confidence) })
                .OrderBy(x => x.Confidence);

            // Assert missing element is the one with lowest confidence. (Confidence can also be zero, if tag has not been found.)
            this.MissingTag = regionTags.FirstOrDefault()?.DisplayText;
        }

        public void Reset()
        {
            this.currentRegionTags.Clear();
            this.MissingTag = default;
        }
    }
}
