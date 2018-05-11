using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NBitcoin;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBitcoinReceiveView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinReceiveView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_RECEIVE";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private string m_publicKey = "";
		private string m_pathQRCodeImage = "";

		private QRCodeEncodeController m_qrController;
		private RawImage m_qrCodeImage;
		private QRCodeEncodeController.CodeMode m_codeformat;
		private bool m_sendQRCodeImageByEmail = false;

		private Dictionary<string, Transform> m_iconsCurrencies = new Dictionary<string, Transform>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_publicKey = BitCoinController.Instance.CurrentPublicKey;

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			Transform accountInfo = m_container.Find("AccountInfo");
			string labelInfo = BitCoinController.Instance.AddressToLabel(BitCoinController.Instance.CurrentPublicKey);
			if (labelInfo != BitCoinController.Instance.CurrentPublicKey)
			{
				accountInfo.Find("Label").GetComponent<Text>().text = labelInfo;
			}
			else
			{
				accountInfo.Find("Label").GetComponent<Text>().text = "";
			}
			decimal bitcoinBalance = BitCoinController.Instance.PrivateKeys[BitCoinController.Instance.CurrentPrivateKey];
			string bitcoinTrimmed = Utilities.Trim(bitcoinBalance.ToString());
			accountInfo.Find("Bitcoin").GetComponent<Text>().text = bitcoinTrimmed + " BTC";
			string currencyTrimmed = Utilities.Trim((bitcoinBalance * BitCoinController.Instance.CurrenciesExchange[BitCoinController.Instance.CurrentCurrency]).ToString());
			accountInfo.Find("Exchange").GetComponent<Text>().text = currencyTrimmed + " " + BitCoinController.Instance.CurrentCurrency;

			m_iconsCurrencies.Clear();
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				m_iconsCurrencies.Add(BitCoinController.CURRENCY_CODE[i], accountInfo.Find("IconsCurrency/" + BitCoinController.CURRENCY_CODE[i]));
			}

			UpdateCurrency();


			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.public.address.to.be.paid");

			m_container.Find("PublicKey").GetComponent<InputField>().text = m_publicKey;

			m_container.Find("SendPublicAddress").GetComponent<Button>().onClick.AddListener(OnSendPublicAddress);
			m_container.Find("SendPublicAddress/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.public.address");

			m_container.Find("SendQRCode").GetComponent<Button>().onClick.AddListener(OnSendQRCode);
			m_container.Find("SendQRCode/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.qr.code.address");

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();

#if ENABLE_QRCODE
			m_qrController = GameObject.FindObjectOfType<QRCodeEncodeController>();
			m_qrController.onQREncodeFinished += QREncodeFinished;
			m_qrCodeImage = m_container.Find("QRCode").GetComponent<RawImage>();
			EncodeQRPublicKey();
#else
			m_container.Find("QRCode").gameObject.SetActive(false);
			m_container.Find("SendQRCode").gameObject.SetActive(false);
#endif
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
		 * UpdateCurrency
		 */
		public void UpdateCurrency()
		{
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
		 * EncodeQRPublicKey
		 */
		private void EncodeQRPublicKey()
		{			
			int errorlog = m_qrController.Encode(m_publicKey, QRCodeEncodeController.CodeMode.QR_CODE);
			if (errorlog == -13)
			{
				Debug.LogError("Must contain 12 digits,the 13th digit is automatically added !");
			}
			else if (errorlog == -8)
			{
				Debug.LogError("Must contain 7 digits,the 8th digit is automatically added !");

			}
			else if (errorlog == -128)
			{
				Debug.LogError("Contents length should be between 1 and 80 characters !");

			}
			else if (errorlog == -1)
			{
				Debug.LogError("Please select one code type !");
			}
			else if (errorlog == 0)
			{
				Debug.Log("Encode successfully!");
			}
		}

		// -------------------------------------------
		/* 
		 * QREncodeFinished
		 */
		private void QREncodeFinished(Texture2D tex)
		{
			if (tex != null && tex != null)
			{
				int width = tex.width;
				int height = tex.height;
				float aspect = width * 1.0f / height;
				m_qrCodeImage.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 170.0f / aspect);
				m_qrCodeImage.texture = tex;
				m_pathQRCodeImage = SaveTextureToFile(tex, "PublicAddress_"+ m_publicKey.Substring(0,5) + ".png");
			}
		}

		// -------------------------------------------
		/* 
		 * SaveTextureToFile
		 */
		private string SaveTextureToFile(Texture2D _texture, string _fileName)
		{
			byte[] bytes = _texture.EncodeToPNG();
			string finalPath = Application.dataPath + "/" + _fileName;
			FileStream file = File.Create(finalPath);
			BinaryWriter binary = new BinaryWriter(file);
			binary.Write(bytes);
			file.Close();
			return finalPath;
		}
		// -------------------------------------------
		/* 
		 * OnBackButton
		 */
		private void OnBackButton()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnSendPublicAddress
		 */
		private void OnSendPublicAddress()
		{
			m_sendQRCodeImageByEmail = false;
			ScreenController.Instance.CreateNewScreen(ScreenEnterEmailView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.enter.email.address"));
		}

		// -------------------------------------------
		/* 
		 * OnSendQRCode
		 */
		private void OnSendQRCode()
		{
			m_sendQRCodeImageByEmail = true;
			ScreenController.Instance.CreateNewScreen(ScreenEnterEmailView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.enter.email.address"));
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenEnterEmailView.EVENT_SCREENENTEREMAIL_CONFIRMATION)
			{
				if (!m_sendQRCodeImageByEmail)
				{
					Application.OpenURL("mailto:" + (string)_list[0] + "?subject=" + LanguageController.Instance.GetText("message.public.address") + "&body=" + LanguageController.Instance.GetText("screen.bitcoin.message.email.send.public.key") + ":" + m_publicKey);
				}
				else
				{
					Application.OpenURL("mailto:" + (string)_list[0] + "?subject=" + LanguageController.Instance.GetText("message.public.address") + "&body=" + LanguageController.Instance.GetText("screen.bitcoin.message.email.send.qrcode.image.key") + ". Your QR Image is located at="+ m_pathQRCodeImage);
				}				
			}
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnBackButton();
			}
		}
	}
}