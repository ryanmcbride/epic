using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountListVertical : MonoBehaviour {

  public AccountRow accountRowPrefab;

	// Use this for initialization
	public void Start () {
		_accounts = new List<Account>();
		_account_rows = new List<AccountRow>();
		_accounts_manager = (AccountsManager)FindObjectOfType(typeof(AccountsManager));
		
		// Verify we have all required elements
		Debug.Assert(accountRowPrefab != null, "Missing Account Row Prefab");
	}

	public void Update () {
		// Wait until the Manager has Loaded Data
		if(_dirty && _accounts_manager.getAccounts().Count > 0) {
			_dirty = false;
			SetAccounts(_accounts_manager.getAccounts());
		}
	}

	public void SetAccounts(List<Account> accounts) {
		foreach (var row in _account_rows) {
			Object.Destroy(row.gameObject);
		}
		_accounts = accounts;
		_account_rows = new List<AccountRow>(accounts.Count);
		float row_height = 2.0f; // accountRowPrefab.transform.boundingBox.height;
    float row_offset = row_height + 0.5f;
    for (int ii = 0; ii < _accounts.Count; ii++) {
			var rot = transform.rotation;
				var account_row = Object.Instantiate(accountRowPrefab, transform.position, rot, transform);
				account_row.transform.localPosition = new Vector3(0.0f, -row_offset * (ii + 1), 0.0f);
				// TODO: Animate rows into place
				account_row.SetAccount(_accounts[ii]);
				_account_rows.Add(account_row);
		}
	}

	public void SelectAccount(int index) {
		if(_accounts.Count == 0) { return; }

		var index_to_select = index % _account_rows.Count;
		for (int ii = 0; ii < _account_rows.Count; ii++) {
		  bool is_selected = ii == index_to_select;
			_account_rows[ii].SetSelected(is_selected);
		}
	}

  protected bool _dirty = true;
  protected AccountsManager _accounts_manager;
	protected List<Account> _accounts;
	protected List<AccountRow> _account_rows;
}
