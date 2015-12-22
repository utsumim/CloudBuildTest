using UnityEngine;
using System.Collections;

public class PurchaseTransactionInfo {

    /// <summary>購入対象の課金アイテムのID</summary>
    private string targetProductId;
    /// <summary>サーバから発行されたトランザクションID</summary>
    private string targetTransactionId;
    /// <summary>課金の進行状態</summary>
    private BillingManager.ePurchaseTransactionState purchaseState;

    /// <summary>購入対象の課金アイテムのID</summary>
    public string TargetProductId
    {
        get{ return targetProductId; }
    }

    /// <summary>サーバから発行されたトランザクションID</summary>
    public string TargetTransactionId
    {
        get{ return targetTransactionId; }
    }

    /// <summary>課金の進行状態</summary>
    public BillingManager.ePurchaseTransactionState PurchaseState
    {
        get{ return purchaseState; }
        set{ purchaseState = value; }
    }

    public PurchaseTransactionInfo(string productId, string transactionId, BillingManager.ePurchaseTransactionState state)
    {
        targetProductId = productId;
        targetTransactionId = transactionId;
        purchaseState = state;
    }
}
