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
	 * SlotKeyView
	 * 
	 * Slot that will be used in all the lists of the system 
	 * 
	 * @author Esteban Gallardo
	 */
	public class SlotKeyView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SLOT_SELECTED = "EVENT_SLOT_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private Transform m_container;
		private string m_key;
		private decimal m_balance;
		private GameObject m_selectedBackground;
		private Dictionary<string, Transform> m_iconsCurrencies = new Dictionary<string, Transform>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			ItemMultiObjectEntry item = (ItemMultiObjectEntry)_list[0];

			m_container = this.gameObject.transform;
			
			m_key = (string)item.Objects[0];
			m_balance = (decimal)item.Objects[1];

			string balanceWallet = Utilities.Trim(m_balance.ToString());

			m_container.Find("Label").GetComponent<Text>().text = BitCoinController.Instance.AddressToLabel(BitCoinController.Instance.GetPublicKey(m_key));
			m_container.Find("Bitcoins").GetComponent<Text>().text = balanceWallet.ToString();
			m_container.Find("BTC").GetComponent<Text>().text = "BTC";
			m_selectedBackground = m_container.Find("Selected").gameObject;
			m_selectedBackground.SetActive(false);

			m_iconsCurrencies.Clear();
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				m_iconsCurrencies.Add(BitCoinController.CURRENCY_CODE[i], m_container.Find("IconsCurrency/" + BitCoinController.CURRENCY_CODE[i]));
			}

			UpdateCurrency();

			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);			
		}


		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
			GameObject.Destroy(this.gameObject);

			return true;
		}

		// -------------------------------------------
		/* 
		 * UpdateCurrency
		 */
		public void UpdateCurrency()
		{			
			m_container.Find("Bitcoins").GetComponent<Text>().text = Utilities.Trim(m_balance.ToString());
			string balanceCurrencyWallet = Utilities.Trim((m_balance * BitCoinController.Instance.CurrenciesExchange[BitCoinController.Instance.CurrentCurrency]).ToString());
			m_container.Find("Price").GetComponent<Text>().text = balanceCurrencyWallet;
			m_container.Find("Currency").GetComponent<Text>().text = BitCoinController.Instance.CurrentCurrency;

			m_selectedBackground.SetActive((m_key == BitCoinController.Instance.CurrentPrivateKey));

			foreach (KeyValuePair<string, Transform> item in m_iconsCurrencies)
			{
				if (item.Key == BitCoinController.Instance.CurrentCurrency)
				{
					item.Value.gameObject.SetActive(true);
				}
				else
				{
					item.Value.gameObject.SetActive(false);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnPointerClick
		 */
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			BasicEventController.Instance.DispatchBasicEvent(EVENT_SLOT_SELECTED, m_key);
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_CURRENCY_CHANGED)
			{
				UpdateCurrency();
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PRIVATE_KEY)
			{
				UpdateCurrency();
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_BALANCE_UPDATED)
			{
				string key = (string)_list[0];
				decimal balance = (decimal)_list[1];
				if (m_key == key)
				{
					m_balance = balance;
					UpdateCurrency();
				}				
			}
		}

	}
}