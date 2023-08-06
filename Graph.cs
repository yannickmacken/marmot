using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

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
            List<Tuple<string, string>> newEdges = new List<Tuple<string, string>>();
            foreach (var edge in Edges)
            {
                var sortedEdge = edge.Item1.CompareTo(edge.Item2) < 0 ? edge : new Tuple<string, string>(edge.Item2, edge.Item1);
                newEdges.Add(sortedEdge);
            }
            Edges = newEdges;

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

        public List<Graph> MapOnto(Graph other)
        {
            if (Nodes.Count == other.Nodes.Count && Edges.Count <= other.Edges.Count)
            {
                var finalMappings = new List<Graph>();
                var mappings = new List<List<string>>();
                var permutations = GetPermutations(other.Nodes, other.Nodes.Count);

                foreach (var permlist in permutations)
                {
                    var mapdic = new Dictionary<string, string>();
                    for (int i = 0; i < permlist.Count; i++)
                    {
                        mapdic[Nodes[i]] = permlist[i];
                    }

                    var match = true;
                    foreach (var edge in Edges)
                    {
                        if (!other.HasEdge(new Tuple<string, string>(mapdic[edge.Item1], mapdic[edge.Item2])))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        mappings.Add(permlist);
                    }
                }

                foreach (var mapping in mappings)
                {
                    var roomOrder = mapping.Select(x => other.Nodes.IndexOf(x)).ToList();
                    var tempRooms = other.Rooms.Where((x, i) => roomOrder.Contains(i)).ToList();
                    finalMappings.Add(new Graph(Nodes, Edges, tempRooms, Areas));
                }

                return finalMappings;
            }

            return null;
        }

        public static Graph operator +(Graph a, Graph b)
        {
            var newNodes = a.Nodes.Concat(b.Nodes).ToList();
            var newEdges = a.Edges.Concat(b.Edges).ToList();
            return new Graph(newNodes, newEdges);
        }

        public static bool operator ==(Graph a, Graph b)
        {
            return a.Nodes.SequenceEqual(b.Nodes) && a.Edges.SequenceEqual(b.Edges);
        }

        public static bool operator !=(Graph a, Graph b)
        {
            return !(a == b);
        }

        public static Graph operator -(Graph a, Graph b)
        {
            var newGraph = new Graph(a.Nodes, a.Edges);
            foreach (var node in b.Nodes)
            {
                newGraph.RemoveNode(node);
            }
            foreach (var edge in b.Edges)
            {
                newGraph.RemoveEdge(edge);
            }
            return newGraph;
        }

        public override bool Equals(object obj)
        {
            return obj is Graph graph &&
                   this == graph;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (Nodes != null ? Nodes.GetHashCode() : 0);
            hash = hash * 23 + (Edges != null ? Edges.GetHashCode() : 0);
            return hash;
        }

        private IEnumerable<List<T>> GetPermutations<T>(List<T> list, int length)
        {
            if (length == 1) return list.Select(t => new List<T> { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                    (t1, t2) => t1.Concat(new List<T> { t2 }).ToList());
        }

        public override string ToString()
        {
            string str = $"Graph with\nnodes: {string.Join(", ", Nodes)}\nedges: ";
            str += string.Join(", ", Edges.Select(e => e.ToString()));
            return str;
        }
    }
}
