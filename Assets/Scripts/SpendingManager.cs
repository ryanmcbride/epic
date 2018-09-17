using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartsAndGraphs3D;

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
		_pieCharts = FindObjectsOfType<PieChart>();
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
		if (_categoryManager == null) {
      _categoryManager = FindObjectOfType<CategoryManager>();
    }

		if(_spending_by_category == null)	_spending_by_category = new SortedDictionary<string, Spending>();
		if(_ordered_spending == null) _ordered_spending = new List<Spending>();
		if(_income == null)	_income = new Spending();

		foreach (var trans in transactions) {
			var transaction = JsonUtility.FromJson<Transaction>(JsonUtility.ToJson(trans));

			if(transaction.amount > 0) {
				_income.total_amount += transaction.amount;
			}
			var category = _categoryManager.GetTopCategory(transaction.category_guid);
			var category_guid = category != null ? category.guid : transaction.category_guid;
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
			Spending otherSpending = new Spending();
			otherSpending.category_guid = "other";
			foreach (var pair in _spending_by_category) {
				if(pair.Value.percentage >= 0.03) {
					_ordered_spending.Add(pair.Value);
				} else {
					// Accumulate those under 3% here
					otherSpending.total_amount += pair.Value.total_amount;
					otherSpending.percentage += pair.Value.percentage;
				}
			}
			_ordered_spending.Add(otherSpending);

			_ordered_spending.Sort((x, y) =>  y.total_amount.CompareTo(x.total_amount));
			_has_data = true;

			// Temp
			Debug.Log("Spending Totals:");
			foreach (var spending in _ordered_spending) {
				var category = _categoryManager.GetCategory(spending.category_guid);
				Debug.Log("    Percentage: " + spending.percentage.ToString("P") + "\tCategory: " + category.name + "\tTotal Spend: " + spending.total_amount.ToString());
			}

			_sendDataToPieCharts();
		}
	}

	protected void _sendDataToPieCharts() {
    foreach (var pieChart in _pieCharts) {
			//Debug.Log("Sending Spending Data to Pie Chart: " + pieChart.transform.name);
			foreach (var spending in _ordered_spending) {
				var category = _categoryManager.GetCategory(spending.category_guid);
				var color = CategoryManager.GetCategoryColor(category.name);
				var part = new Part();
				part.Text = category.name + " ";
				part.Value = (float)(spending.percentage);
				pieChart.PartColors.Add(color);
				pieChart.Parts.Add(part);
			}
		}
	}

 	private CategoryManager _categoryManager;
  protected bool _has_data = false;
	protected SortedDictionary<string, Spending> _spending_by_category = new SortedDictionary<string, Spending>();
	protected List<Spending> _ordered_spending = new List<Spending>();
	protected Spending _income = new Spending();
	protected PieChart[] _pieCharts;
}
