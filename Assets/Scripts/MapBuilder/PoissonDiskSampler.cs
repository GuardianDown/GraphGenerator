using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSampler
{
	private float radius;
	private Vector2 rectSize;
	private Vector2 rectPosition;
	private Vector2[,] grid;
	private Rect rect;
	private float cellSize;
	private List<Vector2> activeSamples;
	private float radius2;
	
	private const int attempts = 30;
	
	public PoissonDiskSampler(float radius, Vector2 rectPosition, Vector2 rectSize)
	{
		this.radius = radius;
		this.rectPosition = rectPosition;
		this.rectSize = rectSize;
		rect = new Rect(rectPosition, rectSize);
		radius2 = radius * radius;
		cellSize = radius / Mathf.Sqrt(2);
		grid = new Vector2[Mathf.CeilToInt(rectSize.x / cellSize), Mathf.CeilToInt(rectSize.y / cellSize)];
		activeSamples = new List<Vector2>();
	}
	
	public Vector2[,] PoissonDiskSampling()
	{
		Vector2 firstSample = new Vector2(Random.Range(rectPosition.x, rectPosition.x + rectSize.x), Random.Range(rectPosition.y, rectPosition.y + rectSize.y));
		AddSample(firstSample);
		while(activeSamples.Count > 0)
		{
			int randomIndex = Random.Range(0, activeSamples.Count);
			Vector2 sample = activeSamples[randomIndex];
			bool found = false;
			for(int i = 0; i < attempts; i++)
			{
				float r = Mathf.Sqrt(Random.value * ((2 * radius) * (2 * radius) - (radius * radius)) + (radius * radius));
				float angle = Mathf.Deg2Rad * Random.Range(0, 360);
				float x = sample.x + r * Mathf.Cos(angle);
				float y = sample.y + r * Mathf.Sin(angle);
				Vector2 candidate = new Vector2(x, y);
				if(IsValidSample(candidate))
				{
					found = true;
					AddSample(candidate);
					break;
				}
			}
			if(!found)
				activeSamples.RemoveAt(randomIndex);
		}
		return grid;
	}
	
	private void AddSample(Vector2 sample)
	{
		activeSamples.Add(sample);
		grid[(int)((sample.x - rectPosition.x) / cellSize), (int)((sample.y - rectPosition.y) / cellSize)] = sample;
	}
	
	private bool IsValidSample(Vector2 sample)
	{
		if(!rect.Contains(sample))
			return false;
		Vector2Int gridPosition = new Vector2Int((int)((sample.x - rectPosition.x) / cellSize), (int)((sample.y - rectPosition.y) / cellSize));
		int iMin = Mathf.Max(0, gridPosition.x - 2);
		int iMax = Mathf.Min(gridPosition.x + 2, grid.GetLength(0) - 1);
		int jMin = Mathf.Max(0, gridPosition.y - 2);
		int jMax = Mathf.Min(gridPosition.y + 2, grid.GetLength(1) - 1);
		for(int i = iMin; i <= iMax; i++)
		{
			for(int j = jMin; j <= jMax; j++)
			{
				Vector2 neighbor = grid[i, j];
				if(neighbor != Vector2.zero)
					if(Vector2.Distance(neighbor, sample) < radius)
						return false;
			}
		}
		return true;
	}
}
