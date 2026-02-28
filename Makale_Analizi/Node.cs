using System;
using System.Collections.Generic;

namespace Makale_Analizi
{
    class Node : IEquatable<Node>
    {
        public string Id { get; }
        public string Title { get; }
        public int Year { get; }

        public Node(string id, string title, int year)
        {
            Id = id ?? string.Empty;
            Title = title ?? "Başlıksız";
            Year = year;
        }

        public bool Equals(Node? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Node);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Id} - {Title}";
        }
    }
}