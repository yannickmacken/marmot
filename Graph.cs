using System;
using System.Collections.Generic;
using System.Linq;

namespace Marmot
{
    public class Graph
    {
        public List<string> Nodes { get; set; }
        public List<Tuple<string, string>> Edges { get; set; }
        public List<string> Rooms { get; set; }
        public List<double> Areas { get; set; }

        public Graph(List<string> nodes = null, List<Tuple<string, string>> edges = null, List<string> rooms = null, List<double> areas = null)
        {
            Nodes = nodes ?? new List<string>();
            Edges = edges ?? new List<Tuple<string, string>>();
            Rooms = rooms ?? new List<string>();
            Areas = areas ?? new List<double>();

            // Sort edges for consistency
            foreach (var edge in Edges)
            {
                var sortedEdge = edge.Item1.CompareTo(edge.Item2) < 0 ? edge : new Tuple<string, string>(edge.Item2, edge.Item1);
                Edges[Edges.IndexOf(edge)] = sortedEdge;
            }

            Edges = Edges.Distinct().ToList();  // Remove duplicate edges
            Nodes = Nodes.Distinct().ToList();  // Remove duplicate nodes
        }

        public bool HasNode(string node)
        {
            return Nodes.Contains(node);
        }

        public bool HasEdge(Tuple<string, string> edge)
        {
            return Edges.Contains(edge);
        }

        public void AddNode(string node)
        {
            if (!HasNode(node))
            {
                Nodes.Add(node);
            }
        }

        public void AddEdge(Tuple<string, string> edge)
        {
            if (!HasEdge(edge) && HasNode(edge.Item1) && HasNode(edge.Item2))
            {
                Edges.Add(edge);
            }
        }

        public void RemoveNode(string node)
        {
            if (HasNode(node))
            {
                Nodes.Remove(node);
                Edges.RemoveAll(e => e.Item1 == node || e.Item2 == node);
            }
        }

        public void RemoveEdge(Tuple<string, string> edge)
        {
            if (HasEdge(edge))
            {
                Edges.Remove(edge);
            }
        }

        public void AddRoom(string room)
        {
            Rooms.Add(room);
        }

        public override string ToString()
        {
            string str = $"Graph with\nnodes: {string.Join(", ", Nodes)}\nedges: ";
            str += string.Join(", ", Edges.Select(e => e.ToString()));
            return str;
        }
    }
}
