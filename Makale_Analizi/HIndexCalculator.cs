using Makale_Analizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Makale_Analizi
{
    internal class HIndexResult
    {
        public int HIndex { get; set; }
        public double HMedian { get; set; }
        public List<Node> HCoreNodes { get; set; } = new List<Node>();
    }

    internal static class HIndexCalculator
    {
        public static HIndexResult Calculate(Node targetNode, Graph fullGraph, Dictionary<Node, int> citationCounts)
        {
            var citingNodes = fullGraph.Edges
                .Where(e => e.Target == targetNode && e.Type == EdgeType.Citation)
                .Select(e => e.Source)
                .Distinct()
                .ToList();

            var stats = citingNodes
                .Select(n => new { Node = n, Citations = citationCounts.ContainsKey(n) ? citationCounts[n] : 0 })
                .OrderByDescending(x => x.Citations)
                .ToList();

            int h = 0;
            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i].Citations >= (i + 1)) h = i + 1;
                else break;
            }

            var hCoreList = stats.Take(h).Select(x => x.Node).ToList();

            double median = 0;
            if (hCoreList.Count > 0)
            {
                var vals = stats.Take(h).Select(x => x.Citations).OrderBy(x => x).ToList();
                int c = vals.Count;
                if (c % 2 == 1) median = vals[c / 2];
                else median = (vals[(c / 2) - 1] + vals[c / 2]) / 2.0;
            }

            return new HIndexResult { HIndex = h, HMedian = median, HCoreNodes = hCoreList };
        }
    }
}