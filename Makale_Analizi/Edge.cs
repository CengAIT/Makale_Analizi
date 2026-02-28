using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Makale_Analizi
{
    public enum EdgeType
    {
        Citation,
        Sequence
    }

    class Edge
    {
        public Node Source { get; }
        public Node Target { get; }
        public EdgeType Type { get; }

        public Edge(Node source, Node target, EdgeType type = EdgeType.Citation)
        {
            this.Source = source;
            this.Target = target;
            this.Type = type;
        }
    }
}