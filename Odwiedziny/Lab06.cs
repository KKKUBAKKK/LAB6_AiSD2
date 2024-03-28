using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Schema;

namespace ASD
{
	public class Lab06 : MarshalByRefObject
	{
		/// <summary>Etap 1</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="start">Wierzchołek startowy (wejście z lasu).</param>
		/// <returns>Pierwszy element pary to informacja, czy rozwiązanie istnieje. Drugi element pary, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (bool possible, int[] path) Stage1(int n, DiGraph<int> c, Graph<int> g, int target, int start)
		{
			// Works, but: Test  8:  computation interrupted (time limit 3 time units exceeded)
			// Result is correct, but it takes 7s
			// VERSION WITH A LAYERED GRAPH
			DiGraph<int> layeredGraph = new DiGraph<int>(g.VertexCount * n);
			bool isReachable = false;
			
			for (int layer = 0; layer < n; layer++)
			{
				foreach (var edge in g.BFS().SearchAll())
				{
					if (layer == edge.Weight || c.HasEdge(layer, edge.Weight))
					{
						layeredGraph.AddEdge(edge.From + g.VertexCount * layer, edge.To + g.VertexCount * edge.Weight, edge.Weight);
					}
				}
			}

			List<int> path = new List<int>();
			int prev = start;
			path.Add(start);
			foreach (var e in layeredGraph.DFS().SearchFrom(start))
			{
				if (e.From != prev)
				{
					int ind = path.LastIndexOf(e.From);
					path.RemoveRange(ind + 1, path.Count - ind - 1);
				}
				
				prev = e.To;
				path.Add(e.To);
				
				if (e.To % g.VertexCount == target)
				{
					isReachable = true;
					break;
				}
			}
			
			if (!isReachable)
				return (false, new int[0]);

			for (int i = 0; i < path.Count; i++)
				path[i] = path[i] % g.VertexCount;
			
			return (true, path.ToArray());
		}

		/// <summary>Drugi etap</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="starts">Wierzchołki startowe (wejścia z lasu).</param>
		/// <returns>Pierwszy element pary to koszt najlepszego rozwiązania lub null, gdy rozwiązanie nie istnieje. Drugi element pary, tak jak w etapie 1, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (int? cost, int[] path) Stage2(int n, DiGraph<int> c, Graph<int> g, int target, int[] starts)
		{
			DiGraph<int> layeredGraph = new DiGraph<int>(g.VertexCount * n);
			bool isReachable = false;
			
			for (int layer = 0; layer < n; layer++)
			{
				foreach (var edge in g.BFS().SearchAll())
				{
					if (layer == edge.Weight)
					{
						layeredGraph.AddEdge(edge.From + g.VertexCount * layer, edge.To + g.VertexCount * edge.Weight, 1);
					}
					else if (c.HasEdge(layer, edge.Weight))
					{
						layeredGraph.AddEdge(edge.From + g.VertexCount * layer, edge.To + g.VertexCount * edge.Weight, c.GetEdgeWeight(layer, edge.Weight) + 1);
					}
				}
			}

			PathsInfo<int>[] pathsInfo = new PathsInfo<int>[starts.Length];
			int minCost = Int32.MaxValue;
			int minStart = -1;
			int minTarget = -1;
			for (int i = 0; i < starts.Length; i++)
			{
				pathsInfo[i] = Paths.Dijkstra(layeredGraph, starts[i]);

				for (int j = 0; j < n; j++)
				{
					if (!pathsInfo[i].Reachable(starts[i], target + j * g.VertexCount))
						continue;
					
					int cost = pathsInfo[i].GetDistance(starts[i], target + j * g.VertexCount);
					if (cost < minCost)
					{
						minCost = cost;
						minStart = i;
						minTarget = j;
					}
				}
			}

			if (minCost == Int32.MaxValue)
				return (null, new int[0]);

			var path = pathsInfo[minStart].GetPath(starts[minStart], target + minTarget * g.VertexCount);
			for (int i = 0; i < path.Length; i++)
				path[i] = path[i] % g.VertexCount;

			return (minCost, path.ToArray());
			// return (minCost, new int[0]);
		}
	}
}
