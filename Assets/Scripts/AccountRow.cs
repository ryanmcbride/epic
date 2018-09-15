using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AccountRow : MonoBehaviour {

	public GameObject background;
  public TextMeshPro accountName;
  public TextMeshPro accountType;
  public TextMeshPro balance;

	// Use this for initialization
	public void Start () {
		// Verify we have all required elements
		Debug.Assert(background != null, "Missing Background Component");
		Debug.Assert(accountName != null, "Missing Account Name Component");
		Debug.Assert(accountType != null, "Missing Account Type Component");
		Debug.Assert(balance != null, "Missing Balance Component");

		// Fetch the Material from the Renderer of the Background GameObject
		_background_material = background.GetComponent<Renderer>().material;
		Debug.Assert(_background_material != null, "Missing Background Material");
	}

	public void SetAccount(Account account) {
		_account = account;
		accountName.text = account.name;
		accountType.text = account.account_type.ToString();
		balance.text = account.GetFormattedBalance();
		balance.color = account.GetBalanceColor();
	}

	public void SetSelected(bool selected) {
		if (selected) {
			var color = new Color32(128, 191, 255, 255);
			_background_material.color = color;
			_background_material.SetColor("_EmissionColor", color);
		} else {
			_background_material.color = Color.white;
			_background_material.SetColor("_EmissionColor", Color.white);
		}
	}

	protected Account _account;
  protected Material _background_material;
}
