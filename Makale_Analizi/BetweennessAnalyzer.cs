using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Makale_Analizi
{
    class BetweennessAnalyzer : GraphAnalyzer
    {
        public Dictionary<Node, double> betw = new Dictionary<Node, double>();

        public override void Analyze(Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                betw[node] = 0.0;
            }

            foreach (var source in graph.Nodes)
            {
                Stack<Node> stack = new Stack<Node>();
                Dictionary<Node, List<Node>> predecessors = new Dictionary<Node, List<Node>>();
                Dictionary<Node, int> distance = new Dictionary<Node, int>();
                Dictionary<Node, int> sigma = new Dictionary<Node, int>();

                foreach (var node in graph.Nodes)
                {
                    predecessors[node] = new List<Node>();
                    distance[node] = -1;
                    sigma[node] = 0;
                }

                distance[source] = 0;
                sigma[source] = 1;

                Queue<Node> queue = new Queue<Node>();
                queue.Enqueue(source);

                while (queue.Count > 0)
                {
                    Node v = queue.Dequeue();
                    stack.Push(v);

                    foreach (var edge in graph.Edges)
                    {
                        if (edge.Source != v)
                            continue;

                        Node w = edge.Target;

                        if (distance[w] < 0)
                        {
                            distance[w] = distance[v] + 1;
                            queue.Enqueue(w);
                        }

                        if (distance[w] == distance[v] + 1)
                        {
                            sigma[w] += sigma[v];
                            predecessors[w].Add(v);
                        }
                    }
                }

                Dictionary<Node, double> delta = new Dictionary<Node, double>();
                foreach (var node in graph.Nodes)
                {
                    delta[node] = 0.0;
                }

                while (stack.Count > 0)
                {
                    Node w = stack.Pop();

                    foreach (var v in predecessors[w])
                    {
                        double c = ((double)sigma[v] / sigma[w]) * (1.0 + delta[w]);
                        delta[v] += c;
                    }

                    if (w != source)
                    {
                        betw[w] += delta[w];
                    }
                }
            }

            if (graph.Nodes.Count > 2)
            {
                double maxBetw = betw.Values.Max();
                if (maxBetw > 0)
                {
                    foreach (var node in graph.Nodes.ToList())
                    {
                        betw[node] = betw[node] / maxBetw;
                    }
                }
            }
        }
    }
}