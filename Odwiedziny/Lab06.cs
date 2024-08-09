using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

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
			bool reached = false;
			int curColor = -1;
			(int v, int color)[,] prev = new (int, int)[g.VertexCount, n];
			bool[,] visited = new bool[g.VertexCount, n];
			Stack< (int v, int color)> stack = new Stack<(int v, int color)>();

			for (int i = 0; i < n; i++)
			{
				visited[start, i] = true;
				prev[start, i] = (-1, -1);
			}

			foreach (var edge in g.OutEdges(start))
			{
				stack.Push((edge.To, edge.Weight));
				prev[edge.To, edge.Weight] = (start, edge.Weight);
				visited[edge.To, edge.Weight] = true;

				if (edge.To == target)
					reached = true;
			}

			while (stack.Count > 0 && !reached)
			{
				var v = stack.Pop();

				foreach (var edge in g.OutEdges(v.v))
				{
					if (!visited[edge.To, edge.Weight] && (edge.Weight == v.color || c.HasEdge(v.color, edge.Weight)))
					{
						visited[edge.To, edge.Weight] = true;
						prev[edge.To, edge.Weight] = (v.v, v.color);
						stack.Push((edge.To, edge.Weight));

						if (edge.To == target)
						{
							curColor = edge.Weight;
							reached = true;
							break;
						}
					}
				}
			}

			if (!reached)
				return (false, new int[0]);

			List<int> path = new List<int>();
			int cur = target;
			while (cur != -1)
			{
				path.Add(cur);
				(cur, curColor) = prev[cur, curColor];
			}

			path.Reverse();

			return (reached, path.ToArray());

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
			DiGraph<int> layeredGraph = new DiGraph<int>(g.VertexCount * n + 2);

			int start = layeredGraph.VertexCount - 2;
			int end = layeredGraph.VertexCount - 1;
			foreach (var edge in g.BFS().SearchAll())
			{
				for (int layer = 0; layer < n; layer++)
				{
					if (edge.Weight == layer)
					{
						layeredGraph.AddEdge(edge.From + layer * g.VertexCount, edge.To + layer * g.VertexCount, 1);
					}
					else if (c.HasEdge(layer, edge.Weight))
					{
						layeredGraph.AddEdge(edge.From + layer * g.VertexCount, edge.To + edge.Weight * g.VertexCount,
							1 + c.GetEdgeWeight(layer, edge.Weight));
					}
				}
			}

			for (int i = 0; i < n; i++)
			{
				layeredGraph.AddEdge(target + i * g.VertexCount, end, 0);
				foreach (var s in starts)
					layeredGraph.AddEdge(start, s + i * g.VertexCount, 0);
			}

			var pathInfo = Paths.Dijkstra(layeredGraph, start);
			
			if (!pathInfo.Reachable(start, end))
				return (null, new int[0]);

			var cost = pathInfo.GetDistance(start, end);
			var path = pathInfo.GetPath(start, end);
			List<int> resPath = new List<int>();
			for (int i = 1; i < path.Length - 1; i++)
			{
				resPath.Add(path[i] % g.VertexCount);
			}

			return (cost, resPath.ToArray());
		}
	}
}
