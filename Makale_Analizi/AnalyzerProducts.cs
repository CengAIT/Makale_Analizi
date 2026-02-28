using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Makale_Analizi
{
    class AnalyzerProducts
    {
        public Dictionary<Node, (int inDegree, int outDegree)> DegreeMap { get; }
        public Dictionary<Node, double> BetweennessMap { get; }
        public Dictionary<Node, int> KCoreMap { get; }

        public AnalyzerProducts(
            Dictionary<Node, (int inDegree, int outDegree)> degreeMap,
            Dictionary<Node, double> betweennessMap,
            Dictionary<Node, int> kCoreMap)
        {
            DegreeMap = degreeMap;
            BetweennessMap = betweennessMap;
            KCoreMap = kCoreMap;
        }
    }
}