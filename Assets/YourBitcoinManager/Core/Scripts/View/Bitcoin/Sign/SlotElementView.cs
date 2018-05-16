using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourBitcoinController;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * SlotElementView
	 * 
	 * Slot that will be used in all the lists of the system 
	 * 
	 * @author Esteban Gallardo
	 */
	public class SlotElementView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SLOT_ELEMENT_SELECTED = "EVENT_SLOT_ELEMENT_SELECTED";
		public const string EVENT_SLOT_DELETED_SELECTED = "EVENT_SLOT_DELETED_SELECTED";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const int TYPE_STRING	= 0;
		public const int TYPE_FILE		= 1;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private Transform m_container;
		private GameObject m_selectedBackground;
		private ItemMultiObjects m_item;

		private int m_typeItem = -1;
		private bool m_isImage = false;

		public ItemMultiObjects Item
		{
			get { return m_item;  }
		}
		
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_item = (ItemMultiObjects)_list[0];

			m_container = this.gameObject.transform;

			m_typeItem = (int)m_item.Objects[1];

			m_container.Find("Icons/DATA").gameObject.SetActive(false);
			m_container.Find("Icons/IMAGE").gameObject.SetActive(false);
			m_container.Find("Icons/FILE").gameObject.SetActive(false);

			switch (m_typeItem)
			{
				case TYPE_STRING:
					m_container.Find("Icons/DATA").gameObject.SetActive(true);
					m_container.Find("Label").GetComponent<Text>().text = LanguageController.Instance.GetText("message.type.data.string");
					m_container.Find("Description").GetComponent<Text>().text = (string)m_item.Objects[2];
					break;
					
				case TYPE_FILE:
					m_container.Find("Label").GetComponent<Text>().text = LanguageController.Instance.GetText("message.type.data.file");
					string fullPathFilename = (string)m_item.Objects[2];
					string filename = fullPathFilename.Substring(fullPathFilename.LastIndexOf('\\') + 1, fullPathFilename.Length - (fullPathFilename.LastIndexOf('\\') + 1));
					m_container.Find("Description").GetComponent<Text>().text = filename;

					if (FileSystemManagerController.IsFileImage(filename))
					{
						m_isImage = true;
						m_container.Find("Icons/FILE").gameObject.SetActive(true);
					}
					else
					{
						m_container.Find("Icons/IMAGE").gameObject.SetActive(true);
					}
					break;
			}
			m_selectedBackground = m_container.Find("Selected").gameObject;
			m_selectedBackground.SetActive(false);

			m_container.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDelete);
			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
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
			BasicEventController.Instance.DispatchBasicEvent(EVENT_SLOT_ELEMENT_SELECTED, m_item);
		}

		// -------------------------------------------
		/* 
		 * OnDelete
		 */
		public void OnDelete()
		{
			BasicEventController.Instance.DispatchBasicEvent(EVENT_SLOT_DELETED_SELECTED, m_item);			
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_SLOT_ELEMENT_SELECTED)
			{
				m_selectedBackground.SetActive(m_item == (ItemMultiObjects)_list[0]);
				if (m_selectedBackground.activeSelf)
				{
					switch (m_typeItem)
					{
						case TYPE_STRING:
							ScreenController.Instance.CreateNewScreen(ScreenEnterTextView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.bitcoin.sign.write.text.you.want.signed"), (string)m_item.Objects[2]);
							break;

						case TYPE_FILE:
							if (m_isImage)
							{
								Texture2D loadedTexture = ImageUtilities.LoadTexture2D((string)m_item.Objects[2], 600);
								byte[] dataImage = loadedTexture.EncodeToJPG(75);
								ScreenController.Instance.CreateNewScreen(ScreenSingleImageView.SCREEN_IMAGE, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, (long)-1, dataImage);
							}
							break;
					}
				}
			}
			if (_nameEvent == ScreenEnterTextView.EVENT_SCREENENTERETEXT_MODIFICATION)
			{
				if (m_selectedBackground.activeSelf && (m_typeItem == TYPE_STRING))
				{
					m_item.Objects[2] = (string)_list[0];
					m_container.Find("Description").GetComponent<Text>().text = (string)m_item.Objects[2];
				}					
			}
		}

	}
}