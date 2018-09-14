using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

  public AccountsManager accountsManager;
	public AccountListVertical accountListVertical;
	public TransactionManager transactionManager;
	public TransactionListVertical transactionListVertical;
	public BudgetsManager budgetsManager;

	// Use this for initialization
	public void Start () {
		if (accountsManager == null) {
			accountsManager = (AccountsManager)FindObjectOfType(typeof(AccountsManager));
			if (accountsManager == null) {
				accountsManager = new AccountsManager();
			}
		}
		if (accountListVertical == null) {
    	accountListVertical = (AccountListVertical)FindObjectOfType(typeof(AccountListVertical));
		}

    if (transactionManager == null) {
			transactionManager = (TransactionManager)FindObjectOfType(typeof(TransactionManager));
			if (transactionManager == null) {
				transactionManager = new TransactionManager();
			}
		}
		if (transactionListVertical == null) {
    	transactionListVertical = (TransactionListVertical)FindObjectOfType(typeof(TransactionListVertical));
		}

    if (budgetsManager == null) {
			budgetsManager = (BudgetsManager)FindObjectOfType(typeof(BudgetsManager));
			if (budgetsManager == null) {
				budgetsManager = new BudgetsManager();
			}
		}
	}
	
	// Update is called once per frame
	public void Update () {
		var accounts = accountsManager.GetAccounts();

		if (!_all_data_retrieved && accountsManager.HasData() && transactionManager.HasData() && budgetsManager.HasData()) {
			_all_data_retrieved = true;
			// Poplulate data to the displayed elements in the scene
			if (accountListVertical != null) {
        accountListVertical.SetAccounts(accounts);
			}

		} else {
			// Data Retrieved and pupulated to what elements exist in the scene, now set up any interaction

      // Temp: Select through accounts in order and send data to the related views in the scene
			_account_select_current += Time.deltaTime;
			if (_account_select_current  > _account_select_delay) {
				_account_select_current -= _account_select_delay;
				var account = accounts[_account_select_index];
				// Debug.Log("Select Account " + account.name + " (" + _account_select_index + ") + " + account.guid);
				if (accountListVertical != null) {
					accountListVertical.SelectAccount(_account_select_index);
				}
				if (transactionListVertical != null) {
					var transactions = transactionManager.GetTransactions(account.guid);
					transactionListVertical.SetTransactions(transactions);
				}
				_account_select_index = ++_account_select_index %  accounts.Count;
			}
		}
	}

	private bool _all_data_retrieved = false;

	// Temp
	public float _account_select_delay = 1.0f;
	private float _account_select_current = 0.0f;
	private int _account_select_index = 0;
}
