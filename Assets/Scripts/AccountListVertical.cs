using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountListVertical : MonoBehaviour {

  public AccountRow accountRowPrefab;
	public float rowYHeight = 2.0f;
	public float rowYSpacer = 0.5f;
	public float rowXOffset = 0.25f;
	public float rowAnimationDelay = 0.1f;
	public float rowAnimationTime = 0.1f;
  public iTween.EaseType rowAnimationEaseType = iTween.EaseType.easeOutCubic;

	// Use this for initialization
	public void Start () {
		// Verify we have all required elements
		Debug.Assert(accountRowPrefab != null, "Missing Account Row Prefab");
	}

	public void Update () {
		if(_accounts_manager == null) {
			_accounts_manager = (AccountsManager)FindObjectOfType(typeof(AccountsManager));
			return;
		}

		// Wait until the Manager has Loaded Data
		if(_dirty && _accounts_manager.HasData()) {
			_dirty = false;
			SetAccounts(_accounts_manager.GetAccounts());
		}
	}

	public void SetAccounts(List<Account> accounts) {
		if(_accounts == null) _accounts = new List<Account>();
		if(_account_rows == null) _account_rows = new List<AccountRow>();

		foreach (var row in _account_rows) {
			Object.Destroy(row.gameObject);
		}
		_accounts = accounts;
		_account_rows = new List<AccountRow>(accounts.Count);
		
    float row_offset = rowYHeight + rowYSpacer;
    for (int ii = 0; ii < _accounts.Count; ii++) {
			if (ii == 0) {
				var row = Object.Instantiate(accountRowPrefab, transform.position, transform.rotation, transform);
				row.transform.localPosition = new Vector3(0.0f, -row_offset, 0.0f);
				row.SetAccount(_accounts[ii]);
				_account_rows.Add(row);
			} else {
				var xForm = _account_rows[ii - 1].transform;
				var row = Object.Instantiate(accountRowPrefab, xForm.position, xForm.rotation, xForm);
				row.transform.localPosition = new Vector3(-rowXOffset, 0.0f, 0.0f);
				iTween.MoveBy(row.gameObject,
											iTween.Hash("x", rowXOffset * xForm.lossyScale.x,
																	"y", -row_offset * xForm.lossyScale.y,
																	"time", rowAnimationTime,
																	"easetype", rowAnimationEaseType,
																	"delay", rowAnimationDelay * ii));
				row.SetAccount(_accounts[ii]);
				_account_rows.Add(row);
			}
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
