using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransactionCollection
{
  public Transaction[] items;
}

[System.Serializable]
public class Transaction
{
	public enum Type
	{
		CREDIT = 1,
		DEBIT  = 2
	}

	public enum Status {
		POSTED  = 1,
		PENDING = 2
	}
	public enum CategorizationSource
	{
		CATEGORIZATION_SOURCE_AUTOCAT             = 0,
		CATEGORIZATION_SOURCE_USER                = 1,
		CATEGORIZATION_SOURCE_FEED                = 2,
		CATEGORIZATION_SOURCE_LEVENSHTEIN         = 3,
		CATEGORIZATION_SOURCE_PARSER              = 4,
		CATEGORIZATION_SOURCE_TRAINER             = 5,
		CATEGORIZATION_SOURCE_SPHINX              = 6,
		CATEGORIZATION_SOURCE_NAIVE_BAYES         = 7,
		CATEGORIZATION_SOURCE_TERMINATION         = 8,
		CATEGORIZATION_SOURCE_KEYWORD_CATEGORIZER = 9,
		CATEGORIZATION_SOURCE_CATCHER             = 10
	}

	public enum DescriptionSource {
		DESCRIPTION_SOURCE_SCRUBBER    = 0,
		DESCRIPTION_SOURCE_USER        = 1,
		DESCRIPTION_SOURCE_FEED        = 2,
		DESCRIPTION_SOURCE_LEVENSHTEIN = 3,
		//4 is not currently used
		DESCRIPTION_SOURCE_TRAINER     = 5
	}


	public string guid;
	public string account_guid;
	public string description;
	public int date;
	public double amount;
	public string memo;
	public string user_guid;
	public Status status;
	public bool is_manual;
	public string category_guid;
	public string parent_guid;
	public string external_id;
	public Type transaction_type;
	public bool has_been_viewed;
	public bool is_personal;
	public bool is_flagged;
	public int revision;
	public int created_at;
	public int updated_at;
	public int status_code;
	public string top_level_category_guid;
	public CategorizationSource categorized_by;
	public bool has_been_split;
	public DescriptionSource described_by;
	public bool is_deleted;
	public int deleted_at;
	public bool is_hidden;
	public string feed_description;
	public int merchant_category_code;
	public bool is_income;
	public bool is_expense;
	public bool is_fee;
	public bool is_demo;
	public string job_guid;
	public bool is_direct_deposit;
	public bool is_bill_pay;
	public string external_guid;
	public string attachment_guid;
	public string feed_attachment_guid;
	public double latitude;
	public double longitude;
	public int transacted_at;
	public double feed_amount;
	public string feed_memo;
	public int feed_posted_at;
	public int feed_transacted_at;
	public Type feed_transaction_type;
	public Status feed_status;
	public string feed_category_guid;
	public double feed_latitude;
	public double feed_longitude;
	public int posted_at;
	public bool is_potential_bill_pay;
	public bool is_payroll_advance;
	public bool is_overdraft_fee;
	public string vendor_guid;
	public string check_number_string;
	public string feed_check_number_string;
	public string metadata;
	public string member_guid;
	public string discovered_account_guid;
	public string check_number;
	public string feed_check_number;
	public string merchant_guid;


	public string GetFormattedAmount() {
		// TODO: Account for `+` and `-`
		return string.Format("{0:C2}", amount);
	}

	public Color32 GetBalanceColor() {
		// TODO: Fix this logic
		if (transaction_type == Type.CREDIT) {
			return new Color32(21, 199, 100, 255);
		}
		return new Color32(71, 79, 89, 255);
	}

};

public class TransactionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		_transactions = new List<Transaction>();
		_transactions_by_account = new SortedDictionary<string, List<Transaction>>();
	}

  public bool HasData() { return _has_data; }

  public List<Transaction> GetTransactions() {
		return _transactions;
	}
	public List<Transaction> GetTransactions(string account_guid) {
		if(_transactions_by_account.ContainsKey(account_guid)) {
			return _transactions_by_account[account_guid];	
		} else {
			return new List<Transaction>();
		}
	}

	public void SetTransactions(Transaction[] transactions) {
		_transactions = new List<Transaction>(transactions.Length);
		_transactions_by_account = new SortedDictionary<string, List<Transaction>>();
		AddTransactions(transactions, true);
	}
	
	public void AddTransactions(Transaction[] transactions, bool final_page) {
		foreach (var trans in transactions) {
			var transaction = JsonUtility.FromJson<Transaction>(JsonUtility.ToJson(trans));
			string key = transaction.account_guid;
			if(!_transactions_by_account.ContainsKey(key)) {
				_transactions_by_account.Add(key, new List<Transaction>());
			}
			_transactions_by_account[key].Add(transaction); // Add to list by account GUID, this makes lookup easier at the cost of memory
			_transactions.Add(transaction);									// Add to full transaction list
		}

		if (final_page) {
			// Sort Lists
			foreach (var pair in _transactions_by_account) {
				pair.Value.Sort((x, y) =>  y.date.CompareTo(x.date));
			}
			_transactions.Sort((x, y) =>  y.date.CompareTo(x.date));
			_has_data = true;
		}
	}

  protected bool _has_data = false;
	protected List<Transaction> _transactions; // Full list of transactions
	protected SortedDictionary<string, List<Transaction>> _transactions_by_account;
}
