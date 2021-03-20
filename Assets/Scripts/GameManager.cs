using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;
	
	public Point CurrentNode { get; set; }
	
	public List<Point> CreatedNodes { get; set; } = new List<Point>();
	
    private void Awake()
	{
		if(instance == null)
			instance = this;
		else
			Destroy(gameObject);
		
		DontDestroyOnLoad(gameObject);
	}
}
