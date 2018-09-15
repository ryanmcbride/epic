using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour {

  public AccountsManager accountsManager;
	public AccountListVertical accountListVertical;
	public TransactionManager transactionManager;
	public TransactionListVertical transactionListVertical;
	public BudgetsManager budgetsManager;

	// Use this for initialization
	public void Start () {
		_startSync();

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

#region SYNC
	public enum SyncStatus {
		NOT_STARTED     = 0,
		SYNCING         = 1,
		SYNC_COMPLETE   = 2,
		SYNC_FAIL 			= 3
	}

#region WINDOWS

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LoginDelegate(bool success);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void SyncDelegate(string model);

	[DllImport("moneymobilex_unity")]
	private static extern int connectivityCheck();

	[DllImport("moneymobilex_unity")]
	private static extern void login(string username, string password, [MarshalAs(UnmanagedType.FunctionPtr)]LoginDelegate functionCallback);

	[DllImport("moneymobilex_unity")]
	private static extern IntPtr getModel(string modelJson);

	[DllImport("moneymobilex_unity")]
	private static extern void syncModel(string modelName, [MarshalAs(UnmanagedType.FunctionPtr)]SyncDelegate functionCallback);

	[DllImport("moneymobilex_unity")]
	private static extern void update();

	private void _startSync() {
		var connectivity = connectivityCheck();
    Debug.Log("Connectivity Check: " + connectivity.ToString());
    login("march17", "fdsfds", HandleLogin);
	}

public static void HandleLogin(bool success) {
		Debug.Log("login (" + success.ToString() + ")");

		syncModel("accounts", HandleSync);
		syncModel("budgets", HandleSync);
		syncModel("categories", HandleSync);
		syncModel("members", HandleSync);
		syncModel("transactions", HandleSync);
}

public static void HandleSync(string modelName) {
		IntPtr models = getModel(modelName);
		string model_json = Marshal.PtrToStringAnsi(models);
		Debug.Log("getModel(" + modelName + ") " + model_json);

		//Update Managers
		if (modelName == "accounts") {
      //accountsManage
		} else if (modelName == "budgets") {
		} else if (modelName == "categories") {
		} else if (modelName == "members") {
		} else if (modelName == "transactions") {
		}
}
#endregion WINDOWS

#region MAC

#endregion MAC

#endregion SYNC

	private bool _all_data_retrieved = false;

	// Temp
	public float _account_select_delay = 1.0f;
	private float _account_select_current = 0.0f;
	private int _account_select_index = 0;
}
