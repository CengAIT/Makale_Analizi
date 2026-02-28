using Makale_Analizi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Makale_Analizi
{
    class Graph
    {
        public List<Node> Nodes { get; }
        public List<Edge> Edges { get; }

        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public void AddNode(Node node)
        {
            if (node == null)
                return;

            if (!Nodes.Contains(node))
                Nodes.Add(node);
        }

        public void AddEdge(Edge edge)
        {
            if (edge == null)
                return;
            Edges.Add(edge);
        }
    }
}