using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BudgetCollection
{
		public Budget[] items;
}

[System.Serializable]
public class Budget {
	public string guid;
	public string user_guid;
	public string category_guid;
	public int amount;
	public double transaction_total;
	public string external_id;
	public int revision;
	public int start_date;
	public int end_date;
	public int created_at;
	public int updated_at;
	public int status_code;
	public string name;
	public bool is_off_track;
	public bool is_exceeded;
	public double projected_transaction_total;
	public double projected_transaction_total_ratio;
	public string top_level_category_guid;
	public bool is_income;
	public string external_guid;
	public string client_guid;
	public string parent_guid;
	public string metadata;
	public bool is_deleted;
	public int deleted_at;
}

public class BudgetsManager : MonoBehaviour {

  public void Start () {
	}

	public bool HasData() { return _has_data; }
	public List<Budget> GetBudgets() { return _budgets;	}
	public List<Budget> GetSubBudgets(string budget_guid) {	return _sub_budgets[budget_guid];	}
	public void SetBudgets(Budget[] budgets) {
		_budgets = new List<Budget>();
		_sub_budgets = new SortedDictionary<string, List<Budget>>();
		foreach (var b in budgets) {
			var budget = JsonUtility.FromJson<Budget>(JsonUtility.ToJson(b));
			if(budget.parent_guid.Length == 0) {
				_budgets.Add(budget);
			} else {
				string key = budget.parent_guid;
				if(!_sub_budgets.ContainsKey(key)) {
					_sub_budgets.Add(key, new List<Budget>());
				}
				_sub_budgets[key].Add(budget);
			}
		}
  	_has_data = true;
	}

  protected bool _has_data = false;
	protected List<Budget> _budgets; 															 // Top level budgets
	protected SortedDictionary<string, List<Budget>> _sub_budgets; // Sub-budgets based on budget guid

}
