using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccountsCollection
{
    public Account[] accounts;
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
    _accounts = new List<Account>();
    _FetchData();
  }

  public bool HasData() { return _has_data; }

  public List<Account> GetAccounts() { return _accounts; }

  protected void _FetchData() {
    //Debug.Log("Loading Accounts");
    var collection = JsonUtility.FromJson<AccountsCollection>(_accounts_json);
    foreach (var a in collection.accounts) {
      var account = JsonUtility.FromJson<Account>(JsonUtility.ToJson(a));
      _accounts.Add(account);
    }
    //Debug.Log("Loaded " + _accounts.Count + " Accounts");
    _accounts.Sort((x, y) => x.account_type.CompareTo(y.account_type));
    _has_data = true;
  }

  protected bool _has_data = false;
  protected List<Account> _accounts;

  private string _accounts_json = "{\"accounts\":[{\"id\":26,\"account_type\":4,\"available_balance\":1000,\"balance\":411.37,\"created_at\":1536178435,\"external_guid\":\"account-6d3a80a9-ae48-4db1-ae83-5ecf65d4312b\",\"feed_account_type\":1,\"guid\":\"ACT-167442ed-2697-14dd-94e9-8e01ffc43c70\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-843fd519-e5b0-708f-b7e8-608660badcfc\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-ec3ba60b-1d92-b5c4-b323-42e9a10e787b\",\"member_is_managed_by_user\":false,\"name\":\"Gringotts Checking\",\"revision\":20,\"updated_at\":1536178438,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":22,\"account_type\":2,\"balance\":21874.7,\"created_at\":1536178435,\"external_guid\":\"EXT-b6a951dd-50ce-bef0-ef85-89a890d578ba-DEM\",\"feed_account_type\":2,\"guid\":\"ACT-0316068a-3982-100d-c73c-07e2f794295f\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-82f1023d-abdb-4ea2-566b-a90c5a7bbd65\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-73b1da26-05c1-cb4b-409f-9b3253067d32\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"Savings\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":24,\"account_type\":9,\"balance\":1218.27,\"created_at\":1536178435,\"guid\":\"ACT-6cceb233-2ec1-1579-b813-5c63626e1fd3\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-MANUAL-cb5c-1d48-741c-b30f4ddd1730\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":true,\"is_personal\":true,\"member_guid\":\"MBR-5435ab2f-e94a-db6a-39fa-dc4fc6ad74d8\",\"member_is_managed_by_user\":true,\"name\":\"test\",\"revision\":11,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":21,\"account_type\":1,\"balance\":5187.24,\"created_at\":1536178435,\"external_guid\":\"EXT-9eefe4fb-6782-8424-9bd0-ba7528fc40b8-DEM\",\"feed_account_type\":1,\"guid\":\"ACT-9b3a2b2d-c377-251e-0520-40b07e00d446\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-82f1023d-abdb-4ea2-566b-a90c5a7bbd65\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-73b1da26-05c1-cb4b-409f-9b3253067d32\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"Checking\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":16,\"account_type\":9,\"balance\":556.44,\"created_at\":1536178435,\"guid\":\"ACT-928d8702-3508-f218-f018-108f2d60cd3e\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-MANUAL-cb5c-1d48-741c-b30f4ddd1730\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":true,\"is_personal\":true,\"member_guid\":\"MBR-5435ab2f-e94a-db6a-39fa-dc4fc6ad74d8\",\"member_is_managed_by_user\":true,\"name\":\"merrrrge\",\"revision\":2,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":9,\"account_type\":7,\"balance\":162477.84,\"created_at\":1536178435,\"external_guid\":\"EXT-59fc0c12-f3bb-9b8e-018d-598af1d82584-DEM\",\"feed_account_type\":7,\"guid\":\"ACT-99a6911e-1bd4-063e-9160-eb3926b53422\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-2e1fb5f8-aff1-ba99-f7ad-3f467fa02734\",\"interest_rate\":3.8,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-2b2d3f4c-c7b9-f2af-6612-ad65c68a702f\",\"member_is_managed_by_user\":true,\"minimum_payment\":1388,\"name\":\"Mortgage\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":15,\"account_type\":1,\"balance\":104.33,\"created_at\":1536178435,\"external_guid\":\"EXT-36964687-8390-e9ea-a050-a8156947d838-DEM\",\"feed_account_type\":1,\"guid\":\"ACT-9bc82e63-179d-3d30-012b-c7b12ca6384c\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-6073ad01-da9e-f6ba-dfdf-5f1500d8e867\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-7cdd87bf-8e3f-6e14-217e-52ab9caae61a\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"Checking\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":13,\"account_type\":3,\"balance\":16482.97,\"created_at\":1536178435,\"external_guid\":\"EXT-e83cab64-ca31-2f15-e090-fe0d797f7c4e-DEM\",\"feed_account_type\":3,\"guid\":\"ACT-4920666f-baeb-dec7-050c-4953f7e430a6\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-d4dde375-895e-44b3-5380-fabdc4e10949\",\"interest_rate\":5.5,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-2ca1640e-bf28-cbf3-2266-999d5e72331c\",\"member_is_managed_by_user\":true,\"minimum_payment\":350,\"name\":\"Honda Pilot\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":19,\"account_type\":2,\"available_balance\":5775,\"balance\":5775,\"created_at\":1536178435,\"external_guid\":\"EXT-6f7c7769-91d3-86a4-58e0-c90fd16ca627-DEM\",\"feed_account_type\":2,\"guid\":\"ACT-978e699f-cc26-3b1d-9af5-d2bb60c071cd\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-e68b4969-d270-6c6c-970b-dccdc75ca586\",\"interest_rate\":0.1,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-83ecf745-40c1-3629-d544-d29f4d5dbea8\",\"member_is_managed_by_user\":true,\"name\":\"Vacation Savings\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":25,\"account_type\":2,\"available_balance\":1000,\"balance\":2147.26,\"created_at\":1536178435,\"external_guid\":\"account-73093544-9d98-4b10-ac3e-5396172ae230\",\"feed_account_type\":2,\"guid\":\"ACT-28a606a5-b009-92b9-f8b2-90a2ef6b197e\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-843fd519-e5b0-708f-b7e8-608660badcfc\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-ec3ba60b-1d92-b5c4-b323-42e9a10e787b\",\"member_is_managed_by_user\":false,\"name\":\"Gringotts Savings\",\"revision\":22,\"updated_at\":1536178438,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":10,\"account_type\":9,\"balance\":44562.78,\"created_at\":1536178435,\"guid\":\"ACT-91d5a98b-b517-7e70-f695-59b1340ea300\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-MANUAL-cb5c-1d48-741c-b30f4ddd1730\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":true,\"is_personal\":true,\"member_guid\":\"MBR-5435ab2f-e94a-db6a-39fa-dc4fc6ad74d8\",\"member_is_managed_by_user\":true,\"name\":\"test\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":12,\"account_type\":4,\"balance\":2817.22,\"created_at\":1536178435,\"external_guid\":\"EXT-b83cb178-14a9-65d1-5243-ca03583e08bb-DEM\",\"feed_account_type\":4,\"guid\":\"ACT-0d9c2f3d-8e76-92d5-32e1-cf4910e81a70\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-acc3b72a-1165-9642-b41d-1e15f62d75fa\",\"interest_rate\":24.99,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-f567cfd7-b9a3-9d9c-6cc2-945176e628a9\",\"member_is_managed_by_user\":true,\"minimum_payment\":120,\"name\":\"Capital One\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":11,\"account_type\":4,\"balance\":2418.37,\"created_at\":1536178435,\"external_guid\":\"EXT-fb6dab14-6b71-a67a-5806-50be33163a59-DEM\",\"feed_account_type\":4,\"guid\":\"ACT-bdb234b0-1c99-743b-0377-12d7337312a8\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-d59961c9-c6ab-4378-7d70-0edcecb2124a\",\"interest_rate\":14.99,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-18f21a57-bc97-d632-68aa-ddef35284af8\",\"member_is_managed_by_user\":true,\"minimum_payment\":90,\"name\":\"Visa Platinum\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":18,\"account_type\":1,\"balance\":187.34,\"created_at\":1536178435,\"external_guid\":\"EXT-1a2cfdcc-585c-ab8c-783e-1b9b5ac4b8cc-DEM\",\"feed_account_type\":1,\"guid\":\"ACT-b2017bb5-831e-551d-c62c-5fb10194ceb1\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-f78cd062-8ec2-5bc2-cd48-ebb8bf032b2d\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-6f7d8d9e-a18e-eaee-aff5-a4f358d3321d\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"PayPal\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":8,\"account_type\":2,\"available_balance\":1500,\"balance\":1500,\"created_at\":1536178435,\"external_guid\":\"EXT-f4856f6d-2014-0774-d4c4-62821a7e2b99-DEM\",\"feed_account_type\":2,\"guid\":\"ACT-3a5a5d3d-66a0-8aae-007b-ecd6d320fc52\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-7568ea81-24ba-1949-d231-f6427632aacd\",\"interest_rate\":2.15,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-44e4ea4e-1208-d5ec-086f-9207cb20501b\",\"member_is_managed_by_user\":true,\"name\":\"Certificate of Deposit\",\"revision\":2,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":7,\"account_type\":5,\"balance\":32989.76,\"created_at\":1536178435,\"external_guid\":\"EXT-727c92ba-ff00-a06d-2cb7-499c80129507-DEM\",\"feed_account_type\":5,\"guid\":\"ACT-1d6a2036-b52c-423f-f0f4-398c077c25c5\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-c95a758b-22e2-8b88-2d71-617ba9f7a9bd\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-5644ed58-7843-edc2-6a90-0a00717ef780\",\"member_is_managed_by_user\":true,\"name\":\"Fidelity Personal Retirement Annuity\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":17,\"account_type\":5,\"balance\":66098.73,\"created_at\":1536178435,\"external_guid\":\"EXT-c47bf19e-643b-98bd-700c-95a09ac1d265-DEM\",\"feed_account_type\":5,\"guid\":\"ACT-d5bc20c9-1790-1942-73d8-319c333298cf\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-c64d4803-0871-5770-31fa-9cd47e509710\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-e996e121-6d62-55c0-98ef-b3ff13ce5f4a\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"TD Ameritrade 8294\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":2,\"account_type\":5,\"balance\":2376.43,\"created_at\":1536178435,\"external_guid\":\"EXT-c0e4e6f4-1706-7c3d-307b-30ea3f2fd1d8-DEM\",\"feed_account_type\":5,\"guid\":\"ACT-e952930b-b27d-cd57-88d7-49ea28c53154\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-c95a758b-22e2-8b88-2d71-617ba9f7a9bd\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-5644ed58-7843-edc2-6a90-0a00717ef780\",\"member_is_managed_by_user\":true,\"name\":\"Joint WROS\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":1,\"account_type\":5,\"balance\":27947,\"created_at\":1536178435,\"external_guid\":\"EXT-c9752a7a-5172-daea-e9d8-5110bea5d46e-DEM\",\"feed_account_type\":5,\"guid\":\"ACT-aa95a316-5023-85dc-e423-cacd5ce3f638\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-c95a758b-22e2-8b88-2d71-617ba9f7a9bd\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-5644ed58-7843-edc2-6a90-0a00717ef780\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"INDIVIDUAL 3518\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":3,\"account_type\":2,\"balance\":2714.26,\"created_at\":1536178435,\"external_guid\":\"EXT-028ac1db-c784-076c-5733-60ef9aecf6db-DEM\",\"feed_account_type\":2,\"guid\":\"ACT-474e951d-91f3-23d4-81ac-0aeb91871f28\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-180aa360-942f-2cf9-0465-01cd53676696\",\"interest_rate\":29.99,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-4c692e71-df27-63e0-441c-34a0052e1e08\",\"member_is_managed_by_user\":true,\"minimum_payment\":125,\"name\":\"Chase Savings\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":5,\"account_type\":5,\"balance\":253578,\"created_at\":1536178435,\"external_guid\":\"EXT-a0597b20-5012-1dd2-04fa-e05bc163745b-DEM\",\"feed_account_type\":5,\"guid\":\"ACT-c34c6af0-6c84-56ce-40f4-cc38416e7851\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-5a4d4843-0876-5028-ca60-7167b390caf7\",\"interest_rate\":0,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-2b88fbf2-0065-149f-1bf2-6d460673180f\",\"member_is_managed_by_user\":true,\"minimum_payment\":0,\"name\":\"401k\",\"revision\":3,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":23,\"account_type\":10,\"balance\":78501,\"created_at\":1536178435,\"external_guid\":\"EXT-0f96c40e-b739-6a3b-5a8a-cb3b810b0d4f-DEM\",\"feed_account_type\":10,\"guid\":\"ACT-f8e78286-a8b8-e795-bac0-08f188e67110\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-c95a758b-22e2-8b88-2d71-617ba9f7a9bd\",\"interest_rate\":4,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-5644ed58-7843-edc2-6a90-0a00717ef780\",\"member_is_managed_by_user\":true,\"name\":\"Fixed Annuity\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":14,\"account_type\":3,\"balance\":45184.54,\"created_at\":1536178435,\"external_guid\":\"EXT-d5628ddc-6d7f-960e-02d2-254d5dbd2979-DEM\",\"feed_account_type\":3,\"guid\":\"ACT-44a44982-2449-3a54-afd0-5ea7053e3931\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-3d7e81b0-861e-5540-1b92-3ace0a10bee2\",\"interest_rate\":8.5,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-d9ddd0f2-b799-6330-f906-1c5e73d066e5\",\"member_is_managed_by_user\":true,\"minimum_payment\":450,\"name\":\"Student Loan\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":20,\"account_type\":4,\"balance\":1798.65,\"created_at\":1536178435,\"external_guid\":\"EXT-c4d7fcca-e8ce-6454-a452-faf7b5d3abb4-DEM\",\"feed_account_type\":4,\"guid\":\"ACT-61a264fe-1391-70a4-f6b9-f6718cc6d102\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-82f1023d-abdb-4ea2-566b-a90c5a7bbd65\",\"interest_rate\":7.99,\"is_closed\":false,\"is_hidden\":false,\"is_manual\":false,\"is_personal\":true,\"member_guid\":\"MBR-73b1da26-05c1-cb4b-409f-9b3253067d32\",\"member_is_managed_by_user\":true,\"minimum_payment\":65,\"name\":\"Credit Card\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":6,\"account_type\":8,\"balance\":26000,\"created_at\":1536178435,\"external_guid\":\"EXT-0778656d-79e6-5be6-4f2a-a03ecc0f19b5-DEM\",\"feed_account_type\":8,\"guid\":\"ACT-3814ca6a-0775-aed7-cace-3833fc55369c\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-MANUAL-cb5c-1d48-741c-b30f4ddd1730\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":true,\"is_personal\":true,\"member_guid\":\"MBR-56078650-ae55-102c-c8aa-8eb959a12cf7\",\"member_is_managed_by_user\":true,\"name\":\"Honda Pilot\",\"revision\":1,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"},{\"id\":4,\"account_type\":8,\"balance\":275000,\"created_at\":1536178435,\"external_guid\":\"EXT-cf009fd3-87c9-3d95-f320-b4483a8a33c8-DEM\",\"feed_account_type\":8,\"guid\":\"ACT-3051fc92-c80b-ab51-88be-28a380eee680\",\"has_monthly_transfer_limit\":false,\"institution_guid\":\"INS-MANUAL-cb5c-1d48-741c-b30f4ddd1730\",\"is_closed\":false,\"is_hidden\":false,\"is_manual\":true,\"is_personal\":true,\"member_guid\":\"MBR-56078650-ae55-102c-c8aa-8eb959a12cf7\",\"member_is_managed_by_user\":true,\"name\":\"Home\",\"revision\":2,\"updated_at\":1536178435,\"user_guid\":\"USR-054a3472-209c-ae50-6b9a-87685371a51d\"}]}";

}
