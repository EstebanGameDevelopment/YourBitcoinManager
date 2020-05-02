using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;

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
		private ItemMultiObjectEntry m_item;

		private int m_typeItem = -1;
		private bool m_isImage = false;

		public ItemMultiObjectEntry Item
		{
			get { return m_item;  }
		}
		
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_item = (ItemMultiObjectEntry)_list[0];

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

					if (FileSystemController.IsFileImage(filename))
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
			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			UIEventController.Instance.UIEvent -= OnBasicEvent;
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
			UIEventController.Instance.DispatchUIEvent(EVENT_SLOT_ELEMENT_SELECTED, m_item);
		}

		// -------------------------------------------
		/* 
		 * OnDelete
		 */
		public void OnDelete()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_SLOT_DELETED_SELECTED, m_item);			
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_SLOT_ELEMENT_SELECTED)
			{
				m_selectedBackground.SetActive(m_item == (ItemMultiObjectEntry)_list[0]);
				if (m_selectedBackground.activeSelf)
				{
					switch (m_typeItem)
					{
						case TYPE_STRING:
                            List<object> listKeyParams = new List<object>();
                            listKeyParams.Add(LanguageController.Instance.GetText("screen.bitcoin.sign.write.text.you.want.signed"));
                            listKeyParams.Add((string)m_item.Objects[2]);
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnterTextView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, listKeyParams.ToArray());
                            break;

						case TYPE_FILE:
							if (m_isImage)
							{
								Texture2D loadedTexture = ImageUtils.LoadTexture2D((string)m_item.Objects[2], 600);
								byte[] dataImage = loadedTexture.EncodeToJPG(75);
                                List<object> listFileParams = new List<object>();
                                listFileParams.Add((long)-1);
                                listFileParams.Add(dataImage);
                                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenSingleImageView.SCREEN_IMAGE, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, listFileParams.ToArray());
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
            UIEventController.Instance.DispatchUIEvent(EVENT_SLOT_ELEMENT_SELECTED, m_item);
            return true;
        }
    }
}