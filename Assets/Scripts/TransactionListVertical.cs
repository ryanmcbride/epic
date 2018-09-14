using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransactionListVertical : MonoBehaviour {

  public TransactionRow transactionRowPrefab;

	public float rowYHeight = 2.0f;
	public float rowYSpacer = 0.5f;
	public float rowXOffset = 0.25f;
	public float rowAnimationDelay = 0.1f;
	public float rowAnimationTime = 0.1f;
  public iTween.EaseType rowAnimationEaseType = iTween.EaseType.easeOutCubic;

	// Use this for initialization
	public void Start () {
		_transactions = new List<Transaction>();
		_transaction_rows = new List<TransactionRow>();

		// Verify we have all required elements
		Debug.Assert(transactionRowPrefab != null, "Missing Transaction Row Prefab");
	}

	public void SetTransactions(List<Transaction> transactions) {
		foreach (var row in _transaction_rows) {
			Object.Destroy(row.gameObject);
		}
		_transactions = transactions;
		_transaction_rows = new List<TransactionRow>(transactions.Count);

    float row_offset = rowYHeight + rowYSpacer;
    for (int ii = 0; ii < _transactions.Count; ii++) {
			if (ii == 0) {
				var row = Object.Instantiate(transactionRowPrefab, transform.position, transform.rotation, transform);
				row.transform.localPosition = new Vector3(0.0f, -row_offset, 0.0f);
				row.SetTransaction(_transactions[ii]);
				_transaction_rows.Add(row);
			} else {
				var xForm = _transaction_rows[ii - 1].transform;
				var row = Object.Instantiate(transactionRowPrefab, xForm.position, xForm.rotation, xForm);
				row.transform.localPosition = new Vector3(-rowXOffset, 0.0f, 0.0f);
				iTween.MoveBy(row.gameObject,
											iTween.Hash("x", rowXOffset * xForm.lossyScale.x,
																	"y", -row_offset * xForm.lossyScale.y,
																	"time", rowAnimationTime,
																	"easetype", rowAnimationEaseType,
																	"delay", rowAnimationDelay * ii));
				row.SetTransaction(_transactions[ii]);
				_transaction_rows.Add(row);
			}
		}
	}

	protected List<Transaction> _transactions;
	protected List<TransactionRow> _transaction_rows;
}
