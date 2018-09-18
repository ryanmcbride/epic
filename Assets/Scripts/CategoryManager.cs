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
	public Category GetTopCategory(string guid) {
		if(_categories != null) {
			var category = GetCategory(guid);
			if(category != null && category.parent_guid.Length > 0) {
				//Pull Parent guid (recursve)
				return GetTopCategory(category.parent_guid);
			}
			return category;
		}
		return null;
	}

  public void SetCategories(Category[] categories) { 
		_categories = new List<Category>();
		AddCategories(categories, true); 
	}

	public void AddCategories(Category[] categories, bool final_page) {
		if(_categories == null) _categories = new List<Category>();
		foreach (var cat in categories) {
			var category = JsonUtility.FromJson<Category>(JsonUtility.ToJson(cat));
			_categories.Add(category);
		}
		if (final_page) {
			// 'Other' Category
			var otherCategory = new Category();
			otherCategory.guid = "other";
			otherCategory.name = "Other";
			_categories.Add(otherCategory);
			_has_data = true;
		}
	}

	public static Color32 GetCategoryColor(string categoryName) {
    switch (categoryName.ToLower())	{
			case "auto & transport":  return new Color32( 75, 157, 188, 255);
			case "bills & utilities": return new Color32(239, 139,  44, 255);
			case "business services": return new Color32(179, 222, 140, 255);
			case "education":				  return new Color32(248, 171,  58, 255);
			case "entertainment": 		return new Color32(171,  91, 137, 255);
			case "fees & charges":    return new Color32(255, 150, 150, 255);
			case "financial":				  return new Color32(107, 205, 219, 255);
			case "food & dining":     return new Color32( 88, 172, 123, 255);
			case "gifts & donations": return new Color32( 52, 122, 165, 255);
			case "health & fitness":  return new Color32( 92,  68, 110, 255);
			case "home":				    	return new Color32(255, 216,  77, 255);
			case "income":				    return new Color32( 19,  63,  73, 255);
			case "investments":				return new Color32(255, 112, 112, 255);
			case "kids":				  		return new Color32(130, 209, 150, 255);
			case "pets":				  		return new Color32(133,  80, 123, 255);
			case "personal care": 		return new Color32( 51, 139, 122, 255);
			case "shopping":				  return new Color32(207,  95, 132, 255);
			case "taxes":			    	  return new Color32( 50,  88, 141, 255);
			case "travel":				  	return new Color32(227, 116,  52, 255);
			case "uncategorized":     return new Color32(250,  85,  85, 255);
			case "transfer":     			return new Color32( 20,  99, 200, 255);
			case "other":          	  return new Color32(149, 156, 166, 255);
			default:
				Debug.Log("Category Color Not Found: " + categoryName);
				return new Color32(149, 156, 166, 255); //`other` color
		}
	}

  protected bool _has_data = false;
	protected List<Category> _categories = new List<Category>();

}
