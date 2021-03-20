using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RefreshButton : AbstractButton
{	
	public override void OnPointerClick(PointerEventData eventData)
	{
		SceneManager.LoadScene("Map", LoadSceneMode.Single);
	}
}
