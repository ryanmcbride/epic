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
		foreach (var account_rows in _account_rows) {
			Object.Destroy(account_rows);
		}
		_accounts = accounts;
		_account_rows = new List<AccountRow>(accounts.Count);
		float row_height = 2.0f; // accountRowPrefab.transform.position.height;
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

  protected bool _dirty = true;
  protected AccountsManager _accounts_manager;
	protected List<Account> _accounts;
	protected List<AccountRow> _account_rows;
}
