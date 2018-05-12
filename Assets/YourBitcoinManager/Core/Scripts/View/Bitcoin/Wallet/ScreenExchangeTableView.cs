using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * ScreenExchangeTableView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenExchangeTableView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_EXCHANGE_TABLE = "SCREEN_EXCHANGE_TABLE";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			decimal bitcoinsUser = (decimal)_list[0];
			string tableBitcoinsExchange = (string)_list[1];

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.table.exchange.title");
			m_container.Find("MoneyEarned").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.table.exchange.money.earned");

			JSONNode jsonExchangeTable = JSON.Parse(tableBitcoinsExchange);
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				string currencyCode = BitCoinController.CURRENCY_CODE[i];
				if (currencyCode != BitCoinController.CODE_BITCOIN)
				{
					float exchangeValue = float.Parse(jsonExchangeTable[currencyCode]["sell"]);
					string symbolValue = jsonExchangeTable[currencyCode]["symbol"];
					GameObject node = m_container.Find(currencyCode).gameObject;
					node.transform.Find("ConversionRate").GetComponent<Text>().text = "x" + exchangeValue;
					string finalStringValue = (bitcoinsUser * (decimal)exchangeValue).ToString();
					if (finalStringValue.IndexOf('.') != -1)
					{
						int decimalNumber = finalStringValue.Length - finalStringValue.IndexOf('.');
						if (decimalNumber > 2)
						{
							decimalNumber = 2;
						}
						finalStringValue = finalStringValue.Substring(0, finalStringValue.IndexOf('.') + decimalNumber);
					}
					node.transform.Find("Balance").GetComponent<Text>().text = finalStringValue + " " + symbolValue;
				}
			}

			m_container.Find("PricesUpdated").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.table.exchange.prices.updated.15");

			m_container.Find("Button_OK").GetComponent<Button>().onClick.AddListener(OkPressed);

			BasicEventController.Instance.DispatchBasicEvent(ScreenInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			BasicEventController.Instance.DispatchBasicEvent(ScreenController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OkPressed
		 */
		private void OkPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OkPressed();
			}
		}
	}
}