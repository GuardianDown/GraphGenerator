using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DelaunayTriangulator
{
	private HashSet<Triangle> triangulation = null;
	private Vector2[,] grid = null;
	[SerializeField] private Vector2 rectSize = new Vector2(16, 7);
	[SerializeField] private Vector2 rectPosition = new Vector2(-8, -3);
	
	public DelaunayTriangulator(Vector2[,] grid, Vector2 rectPosition, Vector2 rectSize)
	{
		this.grid = grid;
		this.rectPosition = rectPosition;
		this.rectSize = rectSize;
	}
	
	public HashSet<Triangle> DelaunayTriangulation()
	{
		Point point1 = new Point(rectPosition.x - 10000, rectPosition.y - 10000, -1);
		Point point2 = new Point(rectPosition.x - 10000, rectPosition.y + rectSize.y + 10000, -1);
		Point point3 = new Point(rectPosition.x + rectSize.x + 10000, rectPosition.y + rectSize.y + 10000, -1);
		Point point4 = new Point(rectPosition.x + rectSize.x + 10000, rectPosition.y - 10000, -1);
		Triangle triangle1 = new Triangle(point1, point2, point3);
		Triangle triangle2 = new Triangle(point1, point3, point4);
		triangulation = new HashSet<Triangle>();
		triangulation.Add(triangle1);
		triangulation.Add(triangle2);
		for(int i = 0; i < grid.GetLength(0); i++)
			for(int j = 0; j < grid.GetLength(1); j++)
			{
				if(grid[i, j].x != 0 && grid[i, j].y != 0)
				{
					Point point = new Point(grid[i, j].x, grid[i, j].y, i * grid.GetLength(1) + j);
					HashSet<Triangle> badTriangles = new HashSet<Triangle>(triangulation.Where(triangle => triangle.IsPointInsideCircumcircle(point)));
					List<Edge> boundary = SearchTheBoundaries(badTriangles);
					RemoveBadTriangles(badTriangles);
					foreach(Edge edge in boundary)
						triangulation.Add(new Triangle(point, edge.Point1, edge.Point2));
				}
			}
		triangulation.RemoveWhere(triangle => triangle.Vertices.Contains(point1) || 
											  triangle.Vertices.Contains(point2) || 
											  triangle.Vertices.Contains(point3) || 
											  triangle.Vertices.Contains(point4));
		return triangulation;
	}
	
	private List<Edge> SearchTheBoundaries(HashSet<Triangle> badTriangles)
	{
		List<Edge> edges = new List<Edge>();
		foreach(Triangle triangle in badTriangles)
		{
			if((triangle.Vertices[0].X == triangle.Vertices[1].X && triangle.Vertices[0].X == triangle.Vertices[2].X) ||
				(triangle.Vertices[0].Y == triangle.Vertices[1].Y && triangle.Vertices[0].Y == triangle.Vertices[2].Y))
				{
					edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[2]));
					Debug.Log("zero");
				}
			else
			{
				edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
				edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
				edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
			}
		}
		return edges.GroupBy(edge => edge).Where(edgeGroup => edgeGroup.Count() == 1).Select(edge => edge.First()).ToList();
	}
	
	private void RemoveBadTriangles(HashSet<Triangle> badTriangles)
	{
		foreach(Triangle triangle in badTriangles)
			triangulation.Remove(triangle);
	}
}
