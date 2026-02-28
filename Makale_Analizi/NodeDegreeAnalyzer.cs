using Makale_Analizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Makale_Analizi
{
    class NodeDegreeAnalyzer : GraphAnalyzer
    {
        public Dictionary<Node, (int inDegree, int outDegree)> nodeDegreeMap =
            new Dictionary<Node, (int inDegree, int outDegree)>();

        public override void Analyze(Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                nodeDegreeMap[node] = (0, 0);
            }

            foreach (var edge in graph.Edges)
            {
                var source = edge.Source;
                var target = edge.Target;

                var sourceDegrees = nodeDegreeMap[source];
                nodeDegreeMap[source] = (
                    sourceDegrees.inDegree,
                    sourceDegrees.outDegree + 1
                );

                var targetDegrees = nodeDegreeMap[target];
                nodeDegreeMap[target] = (
                    targetDegrees.inDegree + 1,
                    targetDegrees.outDegree
                );
            }
        }
    }
}