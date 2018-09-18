using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MemberCollection
{
		public Member[] items;
}

[System.Serializable]
public class Member {
  public enum ConnectionStatus {
    CREATED      = 0,  // The member is new and has not aggregated yet
    PREVENTED    = 1,  // MX preventing aggregation until credentials are updated
    DENIED       = 2,  // Invalid Credentials
    CHALLENGED   = 3,  // MFA presented
    REJECTED     = 4,  // MFA answered incorrectly
    LOCKED       = 5,  // Institution is preventing authentication, user must contact FI
    CONNECTED    = 6,  // Successfully authenticated and aggregating data
    IMPEDED      = 7,  // User attention is needed in online banking (marketing message, terms and conditions etc.)
    RECONNECTED  = 8,  // The connection has been migrated, we will not aggregate until user interacts with UI
    DEGRADED     = 9,  // Aggregation has failed for a short period with a minimum number of attempts
    DISCONNECTED = 10, // Aggregation has failed for an extended period of time with a minimum number of attempts
    DISCONTINUED = 11, // The connection to this institution is no longer available
    CLOSED       = 12, // The user, MX, the client or partner have marked the account as closed
    DELAYED      = 13, // Aggregation has taken longer than expected without being CONNECTED
    FAILED       = 14, // Aggregation failed without being CONNECTED
    UPDATED      = 15, // The connection has been updated and has not yet been CONNECTED
    PAUSED       = 16, // Aggregation has been momentarily paused, but the member is still CONNECTED
    IMPORTED     = 17  // Member was imported from another provider using the MDX Migration API
  }

  public string guid;
  public string external_guid;
  public string user_guid;
  public string institution_guid;
  public string current_aggregator_guid;
  public string most_recent_job_guid;
  public string name;
  public bool aggregation_disabled_by_api;
  public bool is_demo;
  public bool is_manual;
  public bool is_user_created;
  public bool is_deleted;
  public int created_at;
  public int updated_at;
  public int deleted_at;
  public int revision;
  public int failed_logins_count;
  public int exception_count;
  public int accounts_count;
  public int status_code;
  public int aggregated_at;
  public bool can_steal_accounts;
  public string finicity_customer_id;
  public string jxchange_customer_id;
  public int next_background_aggregation_at;
  public string finicity_account_ids;
  public string client_guid;
  public string cash_edge_customer_id;
  public string cash_edge_account_ids;
  public int successfully_aggregated_at;
  public bool was_migrated;
  public string metadata;
  public int successfully_aggregated_transactions_at;
  public int successfully_aggregated_holdings_at;
  public string institution_code;
  public bool needs_updated_credentials;
  public ConnectionStatus connection_status;
  public string finicity_institution_login_id;
  public string quovo_user_id;
  public string quovo_brokerage_id;
  public string by_all_accounts_fp_id;
  public string by_all_accounts_account_ids;
}

public class MembersManager : MonoBehaviour {

  public void Start () {
	}

	public bool HasData() { return _has_data; }
	public List<Member> GetMembers() { return _members;	}
	public Member GetMember(string guid) {
		if(_members != null) {
			foreach (var member in _members) {
				if(member.guid == guid) {
					return member;
				}
			}
		}
		return null;
	}

  public void SetMembers(Member[] members) {
    _members = new List<Member>();
    AddMembers(members, true);
  }
	public void AddMembers(Member[] members, bool final_page) {
		if (_members == null) _members = new List<Member>();
		foreach (var mem in members) {
			var member = JsonUtility.FromJson<Member>(JsonUtility.ToJson(mem));
			_members.Add(member);
		}
    if (final_page) {
  	  _has_data = true;
    }
	}

  protected bool _has_data = false;
	protected List<Member> _members = new List<Member>();
}
