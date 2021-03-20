using System.Collections.Generic;
using UnityEngine;
using System;

public class Triangle
{
	private Vector2 center;
	private float raduis;
	
    public Point[] Vertices { get; } = new Point[3];
	public Triangle(Point point1, Point point2, Point point3)
	{
		if(IsClockwise(point1, point2, point3))
		{
			Vertices[0] = point1;
			Vertices[1] = point3;
			Vertices[2] = point2;
		}
		else
		{
			Vertices[0] = point1;
			Vertices[1] = point2;
			Vertices[2] = point3;
		}
		CalculateCircumcircle();
	}
	
	private void CalculateCircumcircle()
	{
		Point p0 = Vertices[0];
        Point p1 = Vertices[1];
        Point p2 = Vertices[2];
        float dA = p0.X * p0.X + p0.Y * p0.Y;
        float dB = p1.X * p1.X + p1.Y * p1.Y;
        float dC = p2.X * p2.X + p2.Y * p2.Y;
        float aux1 = (dA * (p2.Y - p1.Y) + dB * (p0.Y - p2.Y) + dC * (p1.Y - p0.Y));
        float aux2 = -(dA * (p2.X - p1.X) + dB * (p0.X - p2.X) + dC * (p1.X - p0.X));
        float div = (2 * (p0.X * (p2.Y - p1.Y) + p1.X * (p0.Y - p2.Y) + p2.X * (p1.Y - p0.Y)));
		center = new Vector2(aux1 / div, aux2 / div);
        raduis = (p0.X - center.x) * (p0.X - center.x) + (p0.Y - center.y) * (p0.Y - center.y);
	}
	
	private bool IsClockwise(Point point1, Point point2, Point point3)
	{
		return (point2.X - point1.X) * (point3.Y - point1.Y) - (point3.X - point1.X) * (point2.Y - point1.Y) < 0;
	}
	
	public bool IsPointInsideCircumcircle(Point point)
	{
		float radiusToPoint = (point.X - center.x) * (point.X - center.x) + (point.Y - center.y) * (point.Y - center.y);
		return radiusToPoint < raduis;
	}
}
