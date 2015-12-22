using UnityEngine;
using System.Collections.Generic;
using Prime31;



namespace Prime31
{
    public class IABUIManager : MonoBehaviourGUI
    {
#if UNITY_ANDROID
        void OnGUI()
        {
            beginColumn();

            if( GUILayout.Button( "Initialize IAB" ) )
            {
                var key = "your public key from the Android developer portal here";
                key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmqCOeIeSNVBmkWR+NZV7v6EYpyrkBGRTJrKwDSNynJKKzpE//LSEYcDYX0/0QguZocWr/fgblmYhwHHaSzHNYPDskZbms+7NLAimnxwc4//N5JP9nuWsDQuEAn+5BoU+7NdFDGXf2ZY8qA941qLwBJlMlstomHk9fIuEBRs8Heukm5gaXYS+AxgXf7ZrT4uxUWqO2rK+DeUMiy+LFVePkb8KdkvjCR4SDNWL2Iv0l3WukxsLsB/lsiC/zAxYlKyUoaGUTZudycdjZ2Z1K0cRt9H1uKWs6mcKaDkYVSmUJ6tbO5fwJaXTi4rsw0ej7Kqt9fqd4wCWFGQY0blX9hKP2QIDAQAB";
                GoogleIAB.init( key );
            }


            if( GUILayout.Button( "Query Inventory" ) )
            {
                // enter all the available skus from the Play Developer Console in this array so that item information can be fetched for them
                var skus = new string[] { "com.prime31.testproduct", "android.test.purchased", "com.prime31.managedproduct", "com.prime31.testsubscription" };
                GoogleIAB.queryInventory( skus );
            }


            endColumn( true );


            if( GUILayout.Button( "Purchase Real Product" ) )
            {
                GoogleIAB.purchaseProduct( "jp.co.craftegg.nanairorungirls.gem.0004", "payload that gets stored and returned" );
            }


            if( GUILayout.Button( "Consume Real Purchase" ) )
            {
                GoogleIAB.consumeProduct( "jp.co.craftegg.nanairorungirls.gem.0004" );
            }


            if( GUILayout.Button( "Enable High Details Logs" ) )
            {
                GoogleIAB.enableLogging( true );
            }


            if( GUILayout.Button( "Consume Multiple Purchases" ) )
            {
                var skus = new string[] {
                    "jp.co.craftegg.nanairorungirls.gem.0001",
                    "jp.co.craftegg.nanairorungirls.gem.0002",
                    "jp.co.craftegg.nanairorungirls.gem.0003",
                    "jp.co.craftegg.nanairorungirls.gem.0004",
                    "jp.co.craftegg.nanairorungirls.gem.0005",
                    "jp.co.craftegg.nanairorungirls.gem.0006",
                };
                GoogleIAB.consumeProducts( skus );
            }

            endColumn();
        }
#endif
    }

}
