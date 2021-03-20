using UnityEngine;

public class Edge
{
	public Point Point1 { get; }
	public Point Point2 { get; }
	
	public Edge(Point point1, Point point2)
	{
		Point1 = point1;
		Point2 = point2;
	}
	
	public override bool Equals(object obj)
    {
        if(obj == null) 
			return false;
        if(obj.GetType() != GetType()) 
			return false;
        Edge edge = obj as Edge;
		bool same1 = Point1 == edge.Point1 && Point2 == edge.Point2;
		bool same2 = Point1 == edge.Point2 && Point2 == edge.Point1;
        return same1 || same2;
    }

    public override int GetHashCode()
    {
        return (int)Point1.X ^ (int)Point1.Y ^ (int)Point2.X ^ (int)Point2.Y;
    }
}
