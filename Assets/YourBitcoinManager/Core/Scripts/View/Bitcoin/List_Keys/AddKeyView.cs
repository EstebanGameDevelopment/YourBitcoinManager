using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourCommonTools;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * AddKeyView
	 * 
	 * Slot that will be used in add a new key
	 * 
	 * @author Esteban Gallardo
	 */
	public class AddKeyView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_ADD_KEY_SELECTED = "EVENT_ADD_KEY_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private Transform m_container;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_container = this.gameObject.transform;
			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.list.add.new.key");
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			GameObject.Destroy(this.gameObject);

			return true;
		}

		// -------------------------------------------
		/* 
		 * OnPointerClick
		 */
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);

			UIEventController.Instance.DispatchUIEvent(EVENT_ADD_KEY_SELECTED);

		}

	}
}