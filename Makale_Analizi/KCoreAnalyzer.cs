using Makale_Analizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Makale_Analizi
{
    class KCoreAnalyzer : GraphAnalyzer
    {
        public Dictionary<Node, int> kCore = new Dictionary<Node, int>();

        public override void Analyze(Graph graph)
        {
            Dictionary<Node, int> degrees = new Dictionary<Node, int>();
            foreach (var node in graph.Nodes)
                degrees[node] = 0;

            foreach (var edge in graph.Edges)
            {
                degrees[edge.Source]++;
                degrees[edge.Target]++;
            }

            int k = 0;
            var remainingNodes = new HashSet<Node>(graph.Nodes);

            while (remainingNodes.Count > 0)
            {
                bool removed;
                do
                {
                    removed = false;
                    foreach (var node in remainingNodes.ToList())
                    {
                        if (degrees[node] <= k)
                        {
                            remainingNodes.Remove(node);
                            kCore[node] = k;

                            foreach (var edge in graph.Edges)
                            {
                                if (edge.Source == node && remainingNodes.Contains(edge.Target))
                                    degrees[edge.Target]--;
                                if (edge.Target == node && remainingNodes.Contains(edge.Source))
                                    degrees[edge.Source]--;
                            }

                            removed = true;
                        }
                    }
                } while (removed);

                k++;
            }
        }
    }
}