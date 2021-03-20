using System.Linq;

public class StartEndNode : AbstractNode
{
    public override void OnMouseDown()
	{
		GameManager.instance.CurrentNode = GameManager.instance.CreatedNodes.Single(val => val.Index == Index);
	}
}
