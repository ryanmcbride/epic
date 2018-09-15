using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccountCollection
{
    public Account[] items;
}

[System.Serializable]
public class Account
{
    public enum Type {
        ANY = 0,
        CHECKING = 1,
        SAVINGS = 2,
        LOAN = 3,
        CREDIT_CARD = 4,
        INVESTMENT = 5,
        LINE_OF_CREDIT = 6,
        MORTGAGE = 7,
        PROPERTY = 8,
        CASH = 9,
        INSURANCE = 10,
        PREPAID = 11
    }

    public enum Subtype {
        NONE = 0,
        MONEY_MARKET = 1,
        CERTIFICATE_OF_DEPOSIT = 2,
        AUTO = 3,
        STUDENT = 4,
        SMALL_BUSINESS = 5,
        PERSONAL = 6,
        PERSONAL_WITH_COLLATERAL = 7,
        HOME_EQUITY = 8,
        PLAN_401_K = 9,
        PLAN_403_B = 10,
        PLAN_529 = 11,
        IRA = 12, // Traditional IRA
        ROLLOVER_IRA = 13,
        ROTH_IRA = 14,
        TAXABLE = 15,     // For INVESTMENT types
        NON_TAXABLE = 16, // For INVESTMENT types
        BROKERAGE = 17,
        TRUST = 18,
        UNIFORM_GIFTS_TO_MINORS_ACT = 19,
        PLAN_457 = 20,
        PENSION = 21,
        EMPLOYEE_STOCK_OWNERSHIP_PLAN = 22,
        SIMPLIFIED_EMPLOYEE_PENSION = 23,
        SIMPLE_IRA = 24,
        BOAT = 25,
        POWERSPORTS = 26,
        RV = 27,
        HELOC = 28
    }

    public enum PropertyType {
        REAL_ESTATE = 0,
        VEHICLE = 1,
        ART = 2,
        JEWELRY = 3,
        FURNITURE = 4,
        APPLIANCES = 5,
        COMPUTER = 6,
        ELECTRONICS = 7,
        SPORTS_EQUIPMENT = 8,
        MISCELLANEOUS = 9
    }

    public enum SuperType {
        ASSET = 1,
        LIABILITY = 2,
        UNKNOWN = 3
    }

    public Subtype account_subtype;
    public Type account_type;
    public float apr;
    public float apy;
    public float available_balance;
    public float available_credit;
    public float balance;
    public bool is_closed;
    public float credit_limit;
    public string day_payment_is_due;
    public string external_guid;
    public Type feed_account_type;
    public float feed_apr;
    public float feed_apy;
    public string guid;
    public bool has_monthly_transfer_limit;
    public string institution_guid;
    public float interest_rate;
    public bool is_hidden;
    public bool is_manual;
    public bool is_personal;
    public string matures_on;
    public string member_guid;
    public bool member_is_managed_by_user;
    public float minimum_balance;
    public int monthly_transfer_count;
    public string name;
    public string nickname;
    public float original_balance;
    public float payoff_balance;
    public float pending_balance;
    public string property_type;
    public int revision;
    public string started_on;
    public float statement_balance;
    public string user_guid;
    public float minimum_payment;
    public string payment_due_at;

    public SuperType GetSuperType() {
      switch (account_type) {
        case Type.CHECKING:
        case Type.SAVINGS:
        case Type.INVESTMENT:
        case Type.PROPERTY:
        case Type.CASH:
        case Type.PREPAID: return SuperType.ASSET;
        case Type.LOAN:
        case Type.CREDIT_CARD:
        case Type.LINE_OF_CREDIT:
        case Type.MORTGAGE:
        case Type.INSURANCE: return SuperType.LIABILITY;
        default: break;
      }

      return SuperType.UNKNOWN;
    }

    public string GetFormattedBalance() {
      // TODO: Account for `+` and `-`
      return string.Format("{0:C2}", balance);
    }

    public Color32 GetBalanceColor() {
      // TODO: Fix this logic
      if (GetSuperType() == SuperType.LIABILITY) {
        if (balance < 0 ) { return new Color32(235, 52, 52, 255); }
      } else if (GetSuperType() == SuperType.ASSET) {
        if (balance > 0 ) { return new Color32(21, 199, 100, 255); }
      }
      return new Color32(71, 79, 89, 255);
    }
};

public class AccountsManager : MonoBehaviour {

  public void Start () {
  }

  public bool HasData() { return _has_data; }
  public List<Account> GetAccounts() { return _accounts; }
  public void SetAccounts(Account[] accounts) {
		if(_accounts == null)	_accounts = new List<Account>();

    // Debug.Log("Loading Accounts");
    foreach (var a in accounts) {
      var account = JsonUtility.FromJson<Account>(JsonUtility.ToJson(a));
      _accounts.Add(account);
    }
    // Debug.Log("Loaded " + _accounts.Count + " Accounts");
    _accounts.Sort((x, y) => x.account_type.CompareTo(y.account_type));
    _has_data = true;
  }

  protected bool _has_data = false;
  protected List<Account> _accounts;
}
