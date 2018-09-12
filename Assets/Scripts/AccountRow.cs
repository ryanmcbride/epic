using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AccountRow : MonoBehaviour {

  public TextMeshPro institutionName;
  public TextMeshPro accountName;
  public TextMeshPro accountType;
  public TextMeshPro balance;

	// Use this for initialization
	public void Start () {
		// Verify we have all required elements
		Debug.Assert(institutionName != null, "Missing Institution Name Component");
		Debug.Assert(accountName != null, "Missing Account Name Component");
		Debug.Assert(accountType != null, "Missing Account Type Component");
		Debug.Assert(balance != null, "Missing Balance Component");
	}

	public void SetAccount(Account account) {
		_account = account;
		institutionName.text = account.institution_guid; //TODO: Institution Name?
		accountName.text = account.name;
		accountType.text = account.account_type.ToString();
		balance.text = account.GetFormattedBalance();
		balance.color = account.GetBalanceColor();
	}

	protected Account _account;
}
