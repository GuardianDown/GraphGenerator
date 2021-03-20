using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractNode : MonoBehaviour
{
	private Vector3 normalScale;
	private Vector3 bigScale;
	
	public int Index { get; set; }
	public HashSet<AbstractNode> Neighbors { get; set; } = new HashSet<AbstractNode>();
	
	private void Awake()
	{
		normalScale = transform.localScale;
		bigScale = normalScale * 2;
	}
	
	abstract public void OnMouseDown();
	
	private void Update()
	{
		if(this.Index == GameManager.instance.CurrentNode.Index)
			transform.localScale = bigScale;
		else
			transform.localScale = normalScale;
	}
}
