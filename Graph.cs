using System;
using System.Collections.Generic;
using System.Linq;

namespace Marmot
{
	internal class Graph
	{
		public List<string> Nodes { get; set; }
		public List<Tuple<string, string>> Edges { get; set; }
		public List<Tuple<List<int>, List<int>>> Rooms { get; set; }
		public List<double> Areas { get; set; }

		public Graph(
			List<string> nodes = null,
			List<Tuple<string, string>> edges = null,
			List<Tuple<List<int>, List<int>>> rooms = null,
			List<double> areas = null)
		{
			// Add some error checking: max 7 rooms, edges should match nodes..

			Nodes = nodes ?? new List<string>();
			Edges = edges ?? new List<Tuple<string, string>>();
			Rooms = rooms ?? new List<Tuple<List<int>, List<int>>>();
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

		private bool HasNode(string node)
		{
			return Nodes.Contains(node);
		}

		private bool HasEdge(Tuple<string, string> edge)
		{
			return Edges.Contains(edge);
		}

		private void AddNode(string node)
		{
			if (!HasNode(node))
			{
				Nodes.Add(node);
			}
		}

		private void AddEdge(Tuple<string, string> edge)
		{
			if (!HasEdge(edge) && HasNode(edge.Item1) && HasNode(edge.Item2))
			{
				Edges.Add(edge);
			}
		}

		private void RemoveNode(string node)
		{
			if (HasNode(node))
			{
				Nodes.Remove(node);
				Edges.RemoveAll(e => e.Item1 == node || e.Item2 == node);
			}
		}

		private void RemoveEdge(Tuple<string, string> edge)
		{
			if (HasEdge(edge))
			{
				Edges.Remove(edge);
			}
		}

		private void AddRoom(Tuple<List<int>, List<int>> room)
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

		private Graph Clone()
		{
			var clone = new Graph(
				new List<string>(this.Nodes),
				new List<Tuple<string, string>>(this.Edges),
				this.Rooms.Select(r => new Tuple<List<int>, List<int>>(new List<int>(r.Item1), new List<int>(r.Item2))).ToList(),
				new List<double>(this.Areas)
			);
			return clone;
		}

		public Graph RotateGraph()
		{
			var rotatedGraph = this.Clone();
			for (int i = 0; i < rotatedGraph.Rooms.Count; i++)
			{
				rotatedGraph.Rooms[i] = new Tuple<List<int>, List<int>>(rotatedGraph.Rooms[i].Item2, rotatedGraph.Rooms[i].Item1);
			}
			return rotatedGraph;
		}

		public List<Graph> MirrorGraph()
		{
			var xmMappedGraph = this.Clone();
			var ymMappedGraph = this.Clone();
			var xymMappedGraph = this.Clone();

			int mxWidth = this.Rooms.Max(room => room.Item1.Last());
			int myWidth = this.Rooms.Max(room => room.Item2.Last());

			for (int i = 0; i < this.Rooms.Count; i++)
			{
				xmMappedGraph.Rooms[i] = new Tuple<List<int>, List<int>>(
					xmMappedGraph.Rooms[i].Item1.Select(j => mxWidth - j).Reverse().ToList(),
					new List<int>(xmMappedGraph.Rooms[i].Item2));

				ymMappedGraph.Rooms[i] = new Tuple<List<int>, List<int>>(
					new List<int>(ymMappedGraph.Rooms[i].Item1),
					ymMappedGraph.Rooms[i].Item2.Select(j => myWidth - j).Reverse().ToList());

				xymMappedGraph.Rooms[i] = new Tuple<List<int>, List<int>>(
					xymMappedGraph.Rooms[i].Item1.Select(j => mxWidth - j).Reverse().ToList(),
					xymMappedGraph.Rooms[i].Item2.Select(j => myWidth - j).Reverse().ToList());
			}

			return new List<Graph> { xmMappedGraph, ymMappedGraph, xymMappedGraph };
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
