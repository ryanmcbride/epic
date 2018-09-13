using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransactionListVertical : MonoBehaviour {

  public TransactionRow transactionRowPrefab;

	// Use this for initialization
	public void Start () {
		_transactions = new List<Transaction>();
		_transaction_rows = new List<TransactionRow>();

		// Verify we have all required elements
		Debug.Assert(transactionRowPrefab != null, "Missing Transaction Row Prefab");
	}

	public void SetTransactions(List<Transaction> transactions) {
		foreach (var transaction_rows in _transaction_rows) {
			Object.Destroy(transaction_rows);
		}
		_transactions = transactions;
		_transaction_rows = new List<TransactionRow>(transactions.Count);
		
		float row_height = 2.0f; // transactionRowPrefab.transform.position.height;
    float row_offset = row_height + 0.5f;
    for (int ii = 0; ii < _transactions.Count; ii++) {
			var rot = transform.rotation;
				var transaction_row = Object.Instantiate(transactionRowPrefab, transform.position, rot, transform);
				transaction_row.transform.localPosition = new Vector3(0.0f, -row_offset * (ii + 1), 0.0f);
				// TODO: Animate rows into place
				transaction_row.SetTransaction(_transactions[ii]);
				_transaction_rows.Add(transaction_row);
		}
	}

	protected List<Transaction> _transactions;
	protected List<TransactionRow> _transaction_rows;
}
