using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransactionRow : MonoBehaviour {

  public GameObject background;
  public TextMeshPro dateText;
  public TextMeshPro descriptionText;
  public TextMeshPro categoryText;
  public TextMeshPro balanceText;

	// Use this for initialization
	public void Start () {
		// Verify we have all required elements
		Debug.Assert(background != null, "Missing Background Component");
		Debug.Assert(dateText != null, "Missing Date Component");
		Debug.Assert(descriptionText != null, "Missing Desciption Component");
		Debug.Assert(categoryText != null, "Missing Category Component");
		Debug.Assert(balanceText != null, "Missing Balance Component");
	}

	public void SetTransaction(Transaction transaction) {
		_transaction = transaction;
		if(_transaction.status == Transaction.Status.PENDING) {
			dateText.text = "Pending";
			// TODO: `Pending` should italicize fonts and use muted colors for all rows
		} else {
			var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(transaction.date);
			dateText.text = date.ToString("MM/dd/y");
		}
		descriptionText.text = transaction.description;
		categoryText.text = transaction.transaction_type.ToString();
		balanceText.text = transaction.GetFormattedAmount();
		balanceText.color = transaction.GetBalanceColor();
	}

	protected Transaction _transaction;
}
