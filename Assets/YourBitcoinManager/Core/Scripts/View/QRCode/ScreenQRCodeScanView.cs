using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenQRCodeScanView
	 * 
	 * Screen to make a scan of a QR code
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenQRCodeScanView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SCANQRCODE";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private GameObject m_acceptQRCode;
		private Text m_result;
#if ENABLE_QRCODE
		private QRCodeDecodeController m_qrController;
#endif

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.scanqrcode.title");
			m_container.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.scanqrcode.instructions");

			m_result = m_container.Find("Result").GetComponent<Text>();
			m_result.text = "";

			m_acceptQRCode = m_container.Find("AcceptQRCode").gameObject;
			m_acceptQRCode.GetComponent<Button>().onClick.AddListener(ConfirmQRCodeScanned);
			m_acceptQRCode.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.scanqrcode.confirm.code");
			m_acceptQRCode.SetActive(false);

			m_container.Find("Exit").GetComponent<Button>().onClick.AddListener(ExitPressed);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);

#if ENABLE_QRCODE
			m_qrController = GameObject.FindObjectOfType<QRCodeDecodeController>();
			m_qrController.onQRScanFinished += new QRCodeDecodeController.QRScanFinished(QRScanFinished);
#endif
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnBasicEvent;
#if ENABLE_QRCODE
			m_qrController.onQRScanFinished -= QRScanFinished;
			m_qrController = null;
#endif
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			GameObject.Destroy(this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * QRScanFinished
		 */
		private void QRScanFinished(string _dataText)
		{
			m_result.text = _dataText;
			m_acceptQRCode.SetActive(true);
		}

		// -------------------------------------------
		/* 
		 * ConfirmQRCodeScanned
		 */
		private void ConfirmQRCodeScanned()
		{
			BitcoinEventController.Instance.DispatchBitcoinEvent(BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY, m_result.text);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * ExitPressed
		 */
		private void ExitPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				ExitPressed();
			}
		}
	}
}