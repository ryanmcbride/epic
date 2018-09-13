using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransactionRow : MonoBehaviour {

  public GameObject background;
  public TextMeshPro dateText;
  public TextMeshPro payeeText;
  public TextMeshPro categoryText;
  public TextMeshPro accountText;
  public TextMeshPro balanceText;

	// Use this for initialization
	public void Start () {
		// Verify we have all required elements
		Debug.Assert(background != null, "Missing Background Component");
		Debug.Assert(dateText != null, "Missing Date Component");
		Debug.Assert(payeeText != null, "Missing Payee Component");
		Debug.Assert(categoryText != null, "Missing Category Component");
		Debug.Assert(accountText != null, "Missing Account Component");
		Debug.Assert(balanceText != null, "Missing Balance Component");
	}

	public void SetTransaction(Transaction transaction) {
		_transaction = transaction;
		dateText.text = transaction.created_at.ToString(); // TODO: Convert to date
		payeeText.text = transaction.merchant_guid; // TODO: Look up Payee Name
		categoryText.text = transaction.transaction_type.ToString();
		accountText.text = transaction.account_guid; // TODO: Look up Account Name
		balanceText.text = transaction.GetFormattedBalance();
		balanceText.color = transaction.GetBalanceColor();
	}

	protected Transaction _transaction;
}
