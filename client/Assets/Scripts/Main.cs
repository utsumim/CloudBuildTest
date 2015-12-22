using UnityEngine;
using System.Collections;
using Prime31;
using System.Collections.Generic;

public class Main : MonoBehaviour {

#if UNITY_IOS

    // Use this for initialization
    void Start () {
        BillingManager.Instance.SetApi (PurchaseStartApi, GiveItemApi);
        BillingManager.Instance.SetPurchaseCallback (OnPurchaseSucceeded, OnPurchaseFailed, OnPurchaseCancelled);
        BillingManager.Instance.SetGrantCompletedCallback (OnGrantCompleted);
		BillingManager.Instance.ProductIdList = new string[] {
			"jp.co.craftegg.kusari.soul.00001",
			"jp.co.craftegg.kusari.soul.00002",
			"jp.co.craftegg.kusari.soul.00003",
			"jp.co.craftegg.kusari.soul.00004",
			"jp.co.craftegg.kusari.soul.00005",
			"jp.co.craftegg.kusari.soul.00006",
			"jp.co.craftegg.kusari.soul.00007",
		};
    }

    // Update is called once per frame
    void Update () {

    }

    void OnGUI()
    {
        if (GUI.Button (new Rect (150, 0, 100, 100), "purchase")) {
            string targetProductId = BillingManager.Instance.ProductIdList [Random.Range (0, BillingManager.Instance.ProductIdList.Length - 1)];
            BillingManager.Instance.PurchaseProductAuto (targetProductId);
        }
    }

    private void OnPurchaseSucceeded(StoreKitTransaction transaction)
    {
        Debug.Log (
            "transactionId : " + transaction.productIdentifier +
            ", receipt : " + transaction.base64EncodedTransactionReceipt
        );
    }

    private void OnPurchaseFailed(string error)
    {
        Debug.Log (error);
    }

    private void OnPurchaseCancelled(string error)
    {
        Debug.Log (error);
    }

    private void OnGrantCompleted()
    {
        Debug.Log ("Grant Complete");
    }

    private IEnumerator PurchaseStartApi()
    {
        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.RequestThrow;

        yield return new WaitForSeconds (1);
        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.ResponseWait;

        int count = 0;
        while (count < 5) {
            yield return new WaitForSeconds (1);
            count++;
        }

        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.End;
    }

    private IEnumerator GiveItemApi()
    {
        BillingManager.Instance.GrantItemApiState = BillingManager.eApiState.RequestThrow;

        yield return new WaitForSeconds (1);
        BillingManager.Instance.GrantItemApiState = BillingManager.eApiState.ResponseWait;

        int count = 0;
        while (count < 5) {
            yield return new WaitForSeconds (1);
            count++;
        }

        BillingManager.Instance.GrantItemApiState = BillingManager.eApiState.End;
    }

#elif UNITY_ANDROID

    private void Awake()
    {
        BillingManager.Instance.SetApplicationPublickKey ("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmqCOeIeSNVBmkWR+NZV7v6EYpyrkBGRTJrKwDSNynJKKzpE//LSEYcDYX0/0QguZocWr/fgblmYhwHHaSzHNYPDskZbms+7NLAimnxwc4//N5JP9nuWsDQuEAn+5BoU+7NdFDGXf2ZY8qA941qLwBJlMlstomHk9fIuEBRs8Heukm5gaXYS+AxgXf7ZrT4uxUWqO2rK+DeUMiy+LFVePkb8KdkvjCR4SDNWL2Iv0l3WukxsLsB/lsiC/zAxYlKyUoaGUTZudycdjZ2Z1K0cRt9H1uKWs6mcKaDkYVSmUJ6tbO5fwJaXTi4rsw0ej7Kqt9fqd4wCWFGQY0blX9hKP2QIDAQAB");
        BillingManager.Instance.SetApi (PurchaseStartApi, GiveItemApi);
        BillingManager.Instance.SetPurchaseCallback (OnPurchaseSucceeded, OnPurchaseFailed, null);
        BillingManager.Instance.SetQueryInventoryCallback (null, null, NotExistsUnconsumedData);
        BillingManager.Instance.SetConsumeCallback (OnConsumeSucceeded, OnConsumeFailed);

        string[] skus = new string[] {
            "jp.co.craftegg.nanairorungirls.gem.0001",
            "jp.co.craftegg.nanairorungirls.gem.0002",
            "jp.co.craftegg.nanairorungirls.gem.0003",
            "jp.co.craftegg.nanairorungirls.gem.0004",
            "jp.co.craftegg.nanairorungirls.gem.0005",
            "jp.co.craftegg.nanairorungirls.gem.0006",
        };
        BillingManager.Instance.ProductIdList = skus;
    }

    void OnGUI()
    {
        if (GUI.Button (new Rect (0, 0, 100, 100), "init")) {
            BillingManager.Instance.InitGoogleIAB ();
        }

        if (GUI.Button (new Rect (150, 0, 100, 100), "purchase")) {
			BillingManager.Instance.PurchaseProductAuto ("jp.co.craftegg.nanairorungirls.gem.0003");
        }

        if (GUI.Button (new Rect (300, 0, 100, 100), "consume")) {
			BillingManager.Instance.ConsumeProduct ("jp.co.craftegg.nanairorungirls.gem.0003");
        }

        if (GUI.Button (new Rect (450, 0, 100, 100), "inventory")) {
            BillingManager.Instance.QueryInventory(BillingManager.Instance.ProductIdList);
        }
    }

    private IEnumerator PurchaseStartApi()
    {
        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.RequestThrow;

        yield return new WaitForSeconds (1);
        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.ResponseWait;

        int count = 0;
        while (count < 5) {
            yield return new WaitForSeconds (1);
            count++;
        }

        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.End;
    }

    private IEnumerator GiveItemApi()
    {
        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.RequestThrow;

        yield return new WaitForSeconds (1);
        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.ResponseWait;

        int count = 0;
        while (count < 5) {
            yield return new WaitForSeconds (1);
            count++;
        }

        BillingManager.Instance.PurchaseStartApiState = BillingManager.eApiState.End;
    }

    /// <summary>
    /// 購入成功時の処理
    /// </summary>
    /// <param name="purchase">Purchase.</param>
    private void OnPurchaseSucceeded(GooglePurchase purchase)
    {
        Debug.Log (
            "<<OnPurchaseSucceeded>> " +
            "productId : " + purchase.productId + 
            ",packageName : " + purchase.packageName +
            ",originalJson : " + purchase.originalJson + 
            ",developerPayload : " + purchase.developerPayload
        );
    }

    /// <summary>
    /// 購入失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    /// <param name="response">Response.</param>
    private void OnPurchaseFailed(string error, int response)
    {
        Debug.Log (
            "<<OnPurchaseFailed>> " +
            "error : " + error
        );
    }

    /// <summary>
    /// 消費リクエスト成功時の処理
    /// </summary>
    /// <param name="purchase">Purchase.</param>
    private void OnConsumeSucceeded(GooglePurchase purchase)
    {
        Debug.Log (
            "<<OnConsumeSucceeded>> " + 
            "productId : " + purchase.productId
        );
    }

    /// <summary>
    /// 消費リクエスト失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnConsumeFailed(string error)
    {
        Debug.Log (
            "<<OnConsumeFailed>> " + 
            "error : " + error
        );
    }

    private void NotExistsUnconsumedData()
    {
        Debug.Log (
            "<<NotExistsUnconsumedData>>"
        );
    }

#endif

}
