using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
			int curClr = -1;
			int cur = target;
			
			bool possible = false;
			
			List<int> path = new List<int>();
			Queue<int> queue = new Queue<int>();
			
			bool[,] visited = new bool[g.VertexCount, n];
			int[,] prev = new int[g.VertexCount, n];
			
			for (int i = 0; i < n; i++)
				visited[start, i] = true;
			
			queue.Enqueue(start);
			while (queue.Count > 0 && !possible)
			{
				var v = queue.Dequeue();

				foreach (var edge in g.OutEdges(v))
				{
					int nbr = edge.To;
					int color = edge.Weight;
					
					if (visited[nbr, color])
						continue;
					
					if (visited[v, color] || CheckColorChange(visited, v, color, c) != -1)
					{
						visited[nbr, color] = true;
						prev[nbr, color] = v;
						
						if (nbr == target)
						{
							curClr = color;
							possible = true;
							break;
						}
						
						queue.Enqueue(nbr);
					}
				}
			}

			if (!possible)
				return (false, new int[0]);

			int pr = prev[cur, curClr];
			path.Add(cur);
			while (cur != start)
			{
				int tempClr = CheckColorChange(visited, pr, curClr, c);
				if (tempClr != -1)
					curClr = tempClr;
				cur = pr;
				path.Add(cur);
				if (curClr != -1)
					pr = prev[cur, curClr];
			}
			path.Reverse();

			return (possible, path.ToArray());
		}

		int CheckColorChange(bool[,] colors, int vertexFrom, int targetColor, DiGraph<int> c)
		{
			for (int i = 0; i < colors.GetLength(1); i++)
				if (i != targetColor && colors[vertexFrom, i] && c.HasEdge(i, targetColor))
					return i;

			return -1;
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
						layeredGraph.AddEdge(edge.From + g.VertexCount * layer, edge.To + g.VertexCount * layer, 1);
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

			// minCost = pathsInfo[minStart].GetDistance(starts[minStart], target + minTarget * g.VertexCount);
			var path = pathsInfo[minStart].GetPath(starts[minStart], target + minTarget * g.VertexCount);
			for (int i = 0; i < path.Length; i++)
				path[i] = path[i] % g.VertexCount;

			return (minCost, path.ToArray());
			// return (minCost, new int[0]);
		}
	}
}
