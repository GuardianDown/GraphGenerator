using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MapBuilder : MonoBehaviour
{
	[SerializeField] private AbstractNode node = null;
	[SerializeField] private AbstractNode battleNode = null;
	[SerializeField] private AbstractNode storeNode = null;
	[SerializeField] private AbstractNode mercenaryNode = null;
	[SerializeField] private AbstractNode medicNode = null;
	[SerializeField] private AbstractNode eventNode = null;
	[SerializeField] private float battleNodeChance = 45.0f;
	[SerializeField] private float storeNodeChance = 10.0f;
	[SerializeField] private float mercenaryNodeChance = 10.0f;
	[SerializeField] private float medicNodeChance = 10.0f;
	[SerializeField] private float eventNodeChance = 25.0f;
	[SerializeField] private float radius = 1;
	[SerializeField] private Vector2 rectSize = new Vector2(16, 7);
	[SerializeField] private Vector2 rectPosition = new Vector2(-8, -3);
	[SerializeField] private int numberOfPaths = 5;
	[SerializeField] private GameObject linePrefab = null;

	private Vector2[,] grid;
	private HashSet<Triangle> triangulation;
	private HashSet<Point> graph;
	private List<Point>[] paths;
	
    private void Awake()
    {
		GameManager.instance.CreatedNodes = new List<Point>();
		PoissonDiskSampler pds = new PoissonDiskSampler(radius, rectPosition, rectSize);
		grid = pds.PoissonDiskSampling();
		DelaunayTriangulator dt = new DelaunayTriangulator(grid, rectPosition, rectSize);
		triangulation = dt.DelaunayTriangulation();
		RemoveBadTriangles();
		CreateGraph();
		AStarSearch();
		SetActivities();
		FirstInitialization();
		DrawLines();
	}
	
	private void RemoveBadTriangles()
	{
		List<Triangle> badTriangles = new List<Triangle>();
		foreach (Triangle triangle in triangulation)
		{
			Point pa = triangle.Vertices[0];
			Point pb = triangle.Vertices[1];
			Point pc = triangle.Vertices[2];
			float a = Mathf.Sqrt((pb.X - pa.X) * (pb.X - pa.X) + (pb.Y - pa.Y) * (pb.Y - pa.Y));
			float b = Mathf.Sqrt((pc.X - pb.X) * (pc.X - pb.X) + (pc.Y - pb.Y) * (pc.Y - pb.Y));
			float c = Mathf.Sqrt((pa.X - pc.X) * (pa.X - pc.X) + (pa.Y - pc.Y) * (pa.Y - pc.Y));
			float alpha = Mathf.Acos((b * b + c * c - a * a) / (2 * b * c)) * Mathf.Rad2Deg;
			float beta = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c)) * Mathf.Rad2Deg;
			float gamma = Mathf.Acos((b * b + a * a - c * c) / (2 * b * a)) * Mathf.Rad2Deg;
			if (alpha > 130 || beta > 130 || gamma > 130)
				badTriangles.Add(triangle);
		}
		triangulation.ExceptWith(badTriangles);
	}
	
	private void CreateGraph()
	{
		graph = new HashSet<Point>();
		foreach (Triangle triangle in triangulation)
		{
			if(triangle.Vertices[0].X < triangle.Vertices[1].X)
				triangle.Vertices[0].Neighbors.Add(triangle.Vertices[1]);
			else if(triangle.Vertices[1].X < triangle.Vertices[0].X)
				triangle.Vertices[1].Neighbors.Add(triangle.Vertices[0]);
			if(triangle.Vertices[0].X < triangle.Vertices[2].X)
				triangle.Vertices[0].Neighbors.Add(triangle.Vertices[2]);
			else if(triangle.Vertices[2].X < triangle.Vertices[0].X)
				triangle.Vertices[2].Neighbors.Add(triangle.Vertices[0]);
			if(triangle.Vertices[1].X < triangle.Vertices[2].X)
				triangle.Vertices[1].Neighbors.Add(triangle.Vertices[2]);
			else if(triangle.Vertices[2].X < triangle.Vertices[1].X)
				triangle.Vertices[2].Neighbors.Add(triangle.Vertices[1]);
			graph.Add(triangle.Vertices[0]);
			graph.Add(triangle.Vertices[1]);
			graph.Add(triangle.Vertices[2]);
		}
		Point start = new Point(rectPosition.x - 1, rectPosition.y + rectSize.y / 2, -1);
		Point end = new Point(rectPosition.x + rectSize.x + 1, rectPosition.y + rectSize.y / 2, grid.Length);
		graph.Add(start);
		graph.Add(end);
		int counterStart = 0;
		int counterEnd = 0;
		for(int i = 0; i < grid.GetLength(1); i++)
		{
			Point linkedWithStart = graph.SingleOrDefault(val => val.Index == i);
			if(linkedWithStart != null)
			{
				counterStart++;
				graph.Single(val => val == start).Neighbors.Add(linkedWithStart);
				graph.Single(val => val == linkedWithStart).Neighbors.Add(start);
			}
			Point linkedWithEnd = graph.SingleOrDefault(val => val.Index == grid.Length - i - 1);
			if(linkedWithEnd != null)
			{
				counterEnd++;
				graph.Single(val => val == end).Neighbors.Add(linkedWithEnd);
				graph.Single(val => val == linkedWithEnd).Neighbors.Add(end);
			}
		}
		if(counterStart == 0 || counterEnd == 0)
			SceneManager.LoadScene("Map", LoadSceneMode.Single);
	}
	
	private void AStarSearch()
	{
		paths = new List<Point>[numberOfPaths];
		Point[] excludedPoints = new Point[numberOfPaths];
		Point start = graph.Single(val => val.Index == -1);
		Point end = graph.Single(val => val.Index == grid.Length);
		for(int i = 0; i < numberOfPaths; i++)
		{
			paths[i] = new List<Point>();
			PriorityQueue<Point> frontier = new PriorityQueue<Point>();
			frontier.Enqueue(start, 0);
			Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
			cameFrom[start] = start;
			Dictionary<Point, float> costSoFar = new Dictionary<Point, float>();
			costSoFar[start] = 0;
			while(frontier.Count > 0)
			{
				Point current = frontier.Dequeue();
				if(current == end)
					break;
				foreach(Point neighbor in current.Neighbors)
				{
					float newCost = costSoFar[current] + Vector2.Distance(new Vector2(current.X, current.Y), new Vector2(neighbor.X, neighbor.Y));
					if((!costSoFar.ContainsKey(neighbor) || costSoFar[neighbor] > newCost) && !excludedPoints.Contains(neighbor))
					{
						costSoFar[neighbor] = newCost;
						float priority = newCost +  Vector2.Distance(new Vector2(neighbor.X, neighbor.Y), new Vector2(end.X, end.Y));
						frontier.Enqueue(neighbor, newCost);
						cameFrom[neighbor] = current;
					}
				}
			}
			if (!cameFrom.ContainsKey(end))
				break;
			Point cameFromPoint = end;
			while(cameFromPoint != start)
			{
				paths[i].Add(cameFromPoint);
				cameFromPoint = cameFrom[cameFromPoint];
			}
			paths[i].Add(start);
			paths[i].Reverse();
			excludedPoints[i] = paths[i][Random.Range(2, paths[i].Count - 2)];
		}
	}
	
	private void SetActivities()
	{
		List<Point> dist = new List<Point>();
		foreach(List<Point> path in paths)
			if(path != null && path.Count() > 0)
				dist.AddRange(path);
		float pool = dist.Distinct().Count() - 2;
		float onePer = pool / 100;
		float battlePool = onePer * battleNodeChance;
		float storePool = onePer * storeNodeChance;
		float medicPool = onePer * medicNodeChance;
		float mercenaryPool = onePer * mercenaryNodeChance;
		float eventPool = onePer * eventNodeChance;
		foreach(List<Point> path in paths)
		{
			if(path != null && path.Count > 0)
			{
				foreach(Point point in path)
				{
					if(point.Index == -1 || point.Index == grid.Length)
						continue;
					point.TypeOfNode = graph.Single(val => val.Index == point.Index).TypeOfNode;
				}
				foreach(Point point in path)
				{
					if(point.Index == -1 || point.Index == grid.Length)
						continue;
					if(point.TypeOfNode == Nodes.None)
					{
						float chanceOfNode = Random.Range(1.0f, pool);
						if(chanceOfNode <= battlePool)
						{
							battlePool--;
							point.TypeOfNode = Nodes.Battle;
							graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Battle;
						}
						else if(chanceOfNode > battlePool  && chanceOfNode <= battlePool + eventPool)
						{
							eventPool--;
							point.TypeOfNode = Nodes.Event;
							graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Event;
						}
						else if(path.Count(val => val.TypeOfNode == Nodes.Mercenary) == 0 && 
						chanceOfNode > battlePool + eventPool &&
						chanceOfNode <= battlePool + eventPool + mercenaryPool)
						{
							mercenaryPool--;
							point.TypeOfNode = Nodes.Mercenary;
							graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Mercenary;
						}
						else if(path.Count(val => val.TypeOfNode == Nodes.Medic) == 0 &&
						chanceOfNode > battlePool + eventPool + mercenaryPool &&
						chanceOfNode <= battlePool + eventPool + mercenaryPool + medicPool)
						{
							medicPool--;
							point.TypeOfNode = Nodes.Medic;
							graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Medic;
						}
						else if(path.Count(val => val.TypeOfNode == Nodes.Store) == 0 &&
						chanceOfNode > battlePool + eventPool + mercenaryPool + medicPool)
						{
							point.TypeOfNode = Nodes.Store;
							graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Store;
						}
						else
						{
							if(battlePool > 0)
							{
								battlePool--;
								point.TypeOfNode = Nodes.Battle;
								graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Battle;
							}
							else
							{
								eventPool--;
								point.TypeOfNode = Nodes.Event;
								graph.Single(val => val.Index == point.Index).TypeOfNode = Nodes.Event;
							}
							
						}
						pool--;
					}
				}
			}
		}
	}
	
	private void FirstInitialization()
	{
		foreach(List<Point> path in paths)
			if(path != null && path.Count() > 0)
				for(int i = 0; i < path.Count(); i++)
				{
					if(GameManager.instance.CreatedNodes.Count(val => val.Index == path[i].Index) == 0)
					{
						AbstractNode curNode = InstantiateNode(path[i]);
						curNode.Index = path[i].Index;
						path[i].Neighbors = new HashSet<Point>();
						GameManager.instance.CreatedNodes.Add(path[i]);
					}
					if(i != 0)
						GameManager.instance.CreatedNodes.Single(val => val.Index == path[i - 1].Index).Neighbors.Add(GameManager.instance.CreatedNodes.Single(val => val.Index == path[i].Index));
				}	
		GameManager.instance.CurrentNode = GameManager.instance.CreatedNodes.Single(val => val.Index == -1);
	}
	
	private AbstractNode InstantiateNode(Point p)
	{
		switch(p.TypeOfNode)
		{
			case Nodes.None:
				return Instantiate(node, new Vector2(p.X, p.Y), new Quaternion(0, 0, 0, 0)).GetComponent<AbstractNode>();
			case Nodes.Battle:
				return Instantiate(battleNode, new Vector2(p.X, p.Y), new Quaternion(0, 0, 0, 0)).GetComponent<AbstractNode>();
			case Nodes.Event:
				return Instantiate(eventNode, new Vector2(p.X, p.Y), new Quaternion(0, 0, 0, 0)).GetComponent<AbstractNode>();
			case Nodes.Mercenary:
				return Instantiate(mercenaryNode, new Vector2(p.X, p.Y), new Quaternion(0, 0, 0, 0)).GetComponent<AbstractNode>();
			case Nodes.Store:
				return Instantiate(storeNode, new Vector2(p.X, p.Y), new Quaternion(0, 0, 0, 0)).GetComponent<AbstractNode>();
			case Nodes.Medic:
				return Instantiate(medicNode, new Vector2(p.X, p.Y), new Quaternion(0, 0, 0, 0)).GetComponent<AbstractNode>();
			default:
				return null;
		}
	}
	
	private void DrawLines()
	{
		foreach(Point p in GameManager.instance.CreatedNodes)
			foreach(Point neighbor in p.Neighbors)
            {
				GameObject line = Instantiate(linePrefab);
				LineRenderer lr = line.GetComponent<LineRenderer>();
				lr.SetPosition(0, new Vector3(p.X, p.Y, 0));
				lr.SetPosition(1, new Vector3(neighbor.X, neighbor.Y, 0));
            }
	}
}
