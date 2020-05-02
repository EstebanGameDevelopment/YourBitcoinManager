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
	 * AddElementView
	 * 
	 * Slot that will be used in add a new element
	 * 
	 * @author Esteban Gallardo
	 */
	public class AddElementView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_ADD_ELEMENT_SELECTED = "EVENT_ADD_ELEMENT_SELECTED";

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
			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.sign.add.new.data.document");
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

			UIEventController.Instance.DispatchUIEvent(EVENT_ADD_ELEMENT_SELECTED);
		}

        // -------------------------------------------
        /* 
		 * GetOnClick
		 */
        public ButtonClickedEvent GetOnClick()
        {
            return this.onClick;
        }

        // -------------------------------------------
        /* 
		 * RunOnClick
		 */
        public bool RunOnClick()
        {
            UIEventController.Instance.DispatchUIEvent(EVENT_ADD_ELEMENT_SELECTED);
            return true;
        }
    }
}