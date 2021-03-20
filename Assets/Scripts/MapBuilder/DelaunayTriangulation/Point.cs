using System.Collections.Generic;

public class Point
{
	public float X { get; }
	public float Y { get; }
	public int Index { get; }
	public Nodes TypeOfNode { get; set; }
	
	public HashSet<Point> Neighbors { get; set; }
	
	public Point(float x, float y, int index)
	{
		X = x;
		Y = y;
		Index = index;
		Neighbors = new HashSet<Point>();
	}
	
	public override bool Equals(object obj)
    {
        if(obj == null) 
			return false;
        if(obj.GetType() != GetType()) 
			return false;
        Point point = obj as Point;
        return X == point.X && Y == point.Y;
    }

    public override int GetHashCode()
    {
        return (int)X ^ (int)Y;
    }
}
