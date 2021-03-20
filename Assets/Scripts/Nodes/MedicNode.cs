using System.Linq;

public class MedicNode : AbstractNode
{
    public override void OnMouseDown()
	{
		if(GameManager.instance.CurrentNode.Neighbors.Any(val => val.Index == Index) == true)
		{
			GameManager.instance.CurrentNode = GameManager.instance.CreatedNodes.Single(val => val.Index == Index);
		}
	}
}
