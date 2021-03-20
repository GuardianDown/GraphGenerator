using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class AbstractButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	private Image image;
	
	public virtual void Awake()
	{
		image = FindImageChild();
	}
	
	public void OnPointerEnter(PointerEventData eventData)
	{
		image.enabled = true;
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		image.enabled = false;
	}
	
	abstract public void OnPointerClick(PointerEventData eventData);
	
	private Image FindImageChild()
	{
		for(int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			child.TryGetComponent(out Image component);
			if(component != null)
				return component;
		}
		return null;
	}
}
