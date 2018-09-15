using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spending
{
	public string category_guid;
	public double total_amount;
	public double percentage; // 0.0 - 1.0
	
	public string GetFormattedTotalAmount() {
		return string.Format("{0:C2}", total_amount);
	}
};

public class SpendingManager : MonoBehaviour {
	void Start () {
	}

  public bool HasData() { return _has_data; }

	public SortedDictionary<string, Spending> GetSpending() {
		return _spending_by_category;
	}

  public Spending GetSpendingCategory(string category_guid) {
		if (_spending_by_category != null && _spending_by_category.ContainsKey(category_guid)) {
			return _spending_by_category[category_guid];
		}
		return new Spending();
	}

	public void SetTransactions(Transaction[] transactions) {
		_income = new Spending();
		_ordered_spending = new List<Spending>();
		_spending_by_category = new SortedDictionary<string, Spending>();
		AddTransactions(transactions, true);
	}

	public void AddTransactions(Transaction[] transactions, bool final_page) {
		if(_spending_by_category == null)	_spending_by_category = new SortedDictionary<string, Spending>();
		if(_ordered_spending == null) _ordered_spending = new List<Spending>();
		if(_income == null)	_income = new Spending();

		foreach (var trans in transactions) {
			var transaction = JsonUtility.FromJson<Transaction>(JsonUtility.ToJson(trans));

			if(transaction.amount > 0) {
				_income.total_amount += transaction.amount;
			}
			string category_guid = transaction.category_guid;
			if(!_spending_by_category.ContainsKey(category_guid)) {
				var spending = new Spending();
				spending.category_guid = category_guid;
				_spending_by_category.Add(category_guid, spending);
			}
			_spending_by_category[category_guid].total_amount += transaction.amount;
		}

		if (final_page) {
			// Calculate Percentage
			double total_spend = 0.0;
			foreach (var pair in _spending_by_category) {
				total_spend += pair.Value.total_amount;
			}

      if (total_spend > 0.0) {
				foreach (var pair in _spending_by_category) {
					pair.Value.percentage = pair.Value.total_amount / total_spend;
				}
			}
			_income.percentage = 1.0;

			// Build sorted list
			foreach (var pair in _spending_by_category) {
				_ordered_spending.Add(pair.Value);
			}
			_ordered_spending.Sort((x, y) =>  y.total_amount.CompareTo(x.total_amount));
			_has_data = true;

			// Temp
			// Debug.Log("Spending Totals:");
			// foreach (var spending in _ordered_spending) {
			// 	Debug.Log("  Category: " + BubbleBudget.CategoryGuidToTextureName(spending.category_guid) + " Total Spend: " + spending.total_amount.ToString() + " Percentage: " + spending.percentage.ToString());
			// }
			// Debug.Log("  Category: " + BubbleBudget.CategoryGuidToTextureName(_income.category_guid) + " Total Spend: " + _income.total_amount.ToString() + " Percentage: " + _income.percentage.ToString());
		}
	}

  protected bool _has_data = false;
	protected SortedDictionary<string, Spending> _spending_by_category;
	protected List<Spending> _ordered_spending;
	protected Spending _income;
}
