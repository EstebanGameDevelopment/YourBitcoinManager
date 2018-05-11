using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * SlotAddressView
	 * 
	 * Slot that will be used to display an address with its label
	 * 
	 * @author Esteban Gallardo
	 */
	public class SlotAddressView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SLOT_ADDRESS_SELECTED = "EVENT_SLOT_ADDRESS_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private Transform m_container;
		private string m_address;
		private string m_label;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			ItemMultiObjectEntry item = (ItemMultiObjectEntry)_list[0];

			m_container = this.gameObject.transform;
			
			m_address = (string)item.Objects[0];
			m_label = (string)item.Objects[1];

			m_container.Find("Label").GetComponent<Text>().text = m_label;
			m_container.Find("Address").GetComponent<Text>().text = m_address;
		}


		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			GameObject.DestroyObject(this.gameObject);
			return true;
		}


		// -------------------------------------------
		/* 
		 * OnPointerClick
		 */
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			BasicEventController.Instance.DispatchBasicEvent(EVENT_SLOT_ADDRESS_SELECTED, m_address);
		}
	}
}