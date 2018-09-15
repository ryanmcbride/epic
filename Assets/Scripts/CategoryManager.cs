using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CategoryCollection
{
		public Category[] items;
}

[System.Serializable]
public class Category {
  public string guid;
  public string parent_guid;
  public string name;
  public string user_guid;
  public bool is_default;
  public string external_id;
  public int revision;
  public int status_code;
  public int created_at;
  public int updated_at;
  public bool is_income;
  public string external_guid;
  public string metadata;
  public bool is_deleted;
  public int deleted_at;
}

public class CategoryManager : MonoBehaviour {

  public void Start () {
	}

	public bool HasData() { return _has_data; }
	public List<Category> GetCategories() { return _categories;	}
	public Category GetCategory(string guid) {
		if(_categories != null) {
			foreach (var category in _categories) {
				if(category.guid == guid) {
					return category;
				}
			}
		}
		return null;
	}

	public void SetCategories(Category[] categories) {
		_categories = new List<Category>();
		foreach (var cat in categories) {
			var category = JsonUtility.FromJson<Category>(JsonUtility.ToJson(cat));
			_categories.Add(category);
		}
  	_has_data = true;
	}

  protected bool _has_data = false;
	protected List<Category> _categories;

}
