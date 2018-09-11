using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BubbleBudget
{
    private static float CENTER_MASS = 1000.0f;
    private static float CENTER_HEIGHT = 1.3f;
    private static float ALPHA = 150.0f / 255.0f;
    private static float UNIT_SPHERE_VOLUME = 4.19f;
    private static float SPRING_DAMPER = 5.0f;
    private static float INITIAL_TORQUE_Y = 10.0f;
    private static float DRAG = 30.0f;

    private List<GameObject> spheres;
    private List<GameObject> icons;
    private Vector3 spawnerPosition;
    public GameObject center;

    public BubbleBudget(Vector3 parentPosition, BudgetsManager.Budget[] budgets, bool isSubBudget)
    {
        spawnerPosition = parentPosition;
        icons = new List<GameObject>();
        spheres = new List<GameObject>();

        BudgetsManager.Budget budget = budgets[0];
        float max = budget.amount;
        Debug.Log("Budget Name: " + budget.name);

        center = CreateBubble(1.0f, budget);
        if (isSubBudget)
        {
            center.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, ALPHA);
        }
        center.transform.position = new Vector3(spawnerPosition.x, spawnerPosition.y + CENTER_HEIGHT, spawnerPosition.z);

        float previous_magnitude = 0.0f;
        float previous_angle = (float)(Math.PI * 0.5f); // 90 degrees (or up)

        for (int i = 1; i < budgets.Length; ++i)
        {
            budget = budgets[i];

            float volume = UNIT_SPHERE_VOLUME * (budget.amount / max);
            float radius = (float)Math.Pow(0.75f * volume / Math.PI, 0.333333f);
            //float scale = radius;

            Debug.Log("Volume: " + volume + " Radius: " + radius + " Budget amount: " + budget.amount + " Max: " + max);
            GameObject sub = CreateBubble(radius, budget);
            sub.transform.parent = center.transform;

            float magnitude = (1.0f + radius);
            if (i == 1)
            {
                sub.transform.localPosition = Vector3.up * magnitude * 0.5f;
            }
            else
            {
                float a = previous_magnitude;
                float b = previous_magnitude - 1.0f + radius;
                float c = 1.0f + radius;
                float angle_b = (float)Math.Acos((c * c + a * a - b * b) / (2.0 * c * a)) + previous_angle;
                Debug.LogFormat("{0} {1} {2} {3}", a, b, c, angle_b);
                sub.transform.localPosition = new Vector3((float)Math.Cos(angle_b) * magnitude * 0.5f, (float)Math.Sin(angle_b) * magnitude * 0.5f, 0.0f);
                previous_angle = angle_b;
            }
            previous_magnitude = magnitude;

            // Place on unit circle in the xy plane
            //float angle = (float)(UnityEngine.Random.value * Math.PI * 2.0);
            SpringJoint springJoint = sub.AddComponent<SpringJoint>() as SpringJoint;
            springJoint.damper = SPRING_DAMPER;
            springJoint.maxDistance = 0.0f;
            springJoint.minDistance = 0.0f;
            springJoint.connectedBody = center.GetComponent<Rigidbody>();
        }
    }

    public void SetActive(bool active)
    {
        center.SetActive(active);
    }

    private GameObject CreateBubble(float scale, BudgetsManager.Budget budget)
    {
        double percent_spent = budget.transaction_total / budget.amount;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/TranslucentGreen", typeof(Material));
        Color color = Color.green;
        if (percent_spent >= 0.8f)
        {
            color = Color.red;
        }
        else if (percent_spent > 0.6f)
        {
            color = Color.yellow;
        }
        color.a = ALPHA;
        sphere.GetComponent<MeshRenderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);

        sphere.AddComponent<MeshCollider>();

        Rigidbody rigidBody = sphere.AddComponent<Rigidbody>() as Rigidbody;
        rigidBody.isKinematic = false;
        rigidBody.detectCollisions = true;
        rigidBody.useGravity = false;
        //rigidBody.AddTorque(0.0fdddds, INITIAL_TORQUE_Y, 0.0f);
        rigidBody.drag = 100.0f;

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/auto", typeof(Material));
        string texture = CategoryGuidToTextureName(budget.category_guid);
        plane.GetComponent<MeshRenderer>().material.mainTexture = (Texture)Resources.Load("Textures/" + texture, typeof(Texture));
        plane.transform.parent = sphere.transform;
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = new Vector3(0.02f / scale, 0.02f / scale, 0.02f / scale);

        // create 3d text mesh
        GameObject textGameObject = new GameObject();
        TextMesh textMesh = textGameObject.AddComponent<TextMesh>() as TextMesh;
        textMesh.anchor = TextAnchor.MiddleCenter;
        //textMesh.font = font;
        textGameObject.transform.parent = plane.transform;
        textGameObject.transform.localEulerAngles = new Vector3(90.0f, 0.1f, 180.0f);
        textGameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        textGameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 4.0f);
        textGameObject.SetActive(false);

        icons.Add(plane);
        spheres.Add(sphere);

        BudgetData budgetData = sphere.AddComponent<BudgetData>() as BudgetData;
        budgetData.display = string.Format("{0} - ${1} / ${2}", budget.name, budget.transaction_total, budget.amount);
        budgetData.category_guid = budget.category_guid;
        budgetData.guid = budget.guid;

        return sphere;
    }

    // Update is called once per frame
    public void Update(GameObject colliding)
    {
        // Slowly oscillate the central budget up & down and around
        if (center)
        {
            center.transform.Rotate(0, 2.0f /* rpm */ * Time.deltaTime, 0);
        }

        // Billboard the icons
        foreach (GameObject icon in icons)
        {
            icon.transform.LookAt(Camera.main.transform.position, Vector3.up);
            icon.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
            GameObject text = icon.transform.GetChild(0).gameObject;
            text.SetActive(false);
        }

        foreach (GameObject sphere in spheres)
        {
            float scale = sphere == colliding ? 1.7f : 1.5f;

            GameObject textObject = sphere.transform.GetChild(0).transform.GetChild(0).gameObject;
            textObject.SetActive(true);
            textObject.transform.localScale = new Vector3(scale, scale, scale);
            TextMesh mesh = textObject.GetComponent<TextMesh>();// as TextMesh;
            BudgetData budgetData = sphere.GetComponent<BudgetData>() as BudgetData;
            mesh.text = budgetData.display;
        }
    }

    public void Animate(Vector3 start, Vector3 startScale, float t)
    {
        float t2 = 1.0f + (--t) * t * t * t * t;
        center.transform.position = Vector3.Lerp(start, new Vector3(spawnerPosition.x, spawnerPosition.y + CENTER_HEIGHT, spawnerPosition.z), t2);
        center.transform.localScale = Vector3.Lerp(startScale, new Vector3(1.0f, 1.0f, 1.0f), t2);
    }

    private string CategoryGuidToTextureName(string category_guid)
    {
        switch (category_guid)
        {
            case "CAT-7829f71c-2e8c-afa5-2f55-fa3634b89874": return "auto";
            case "CAT-79b02f2f-2adc-88f0-ac2b-4e71ead9cfc8": return "utilities";
            case "CAT-94b11142-e97b-941a-f67f-6e18d246a23f": return "business";
            case "CAT-bf5c9cca-c96b-b50d-440d-38d9adfda5b0": return "education";
            case "CAT-e04e9d1e-e041-c315-2e50-094143ab3f73": return "entertainment";
            case "CAT-d73ee74b-13a4-ac3e-4015-fc4ba9a62b2a": return "finance";
            case "CAT-6c7de3f8-de6c-7061-1dd2-b093044014bf": return "financial";
            case "CAT-bd56d35a-a9a7-6e10-66c1-5b9cc1b6c81a": return "food_and_dining";
            case "CAT-8edf9663-623e-4735-490e-31288f0a70b0": return "gifts";
            case "CAT-52fa4693-c088-afb2-2a99-7bc39bb23a0f": return "health";
            case "CAT-b709172b-4eb7-318e-3b5d-e0f0500b32ac": return "home";
            case "CAT-bf9f3294-4c40-1677-d269-54fbc189faf3": return "income";
            case "CAT-ccd42390-9e8c-3fb6-a5d9-6c31182d9c5c": return "investment";
            case "CAT-0cb1d99d-f558-99e3-2282-b31f359b411a": return "kids";
            case "CAT-e5154228-fe45-790d-a280-f6bf5ae5ac9f": return "beauty";
            case "CAT-7cccbafa-87d7-c9a6-661b-8b3402fe9e78": return "pets";
            case "CAT-aad51b46-d6f7-3da5-fd6e-492328b3023f": return "groceries";
            case "CAT-d00fc539-aa14-009b-4ffb-7e8c7b839954": return "financial";
            case "CAT-bce48142-fea4-ff45-20d9-0a642d44de83": return "transfers";
            case "CAT-d7851c65-3353-e490-1953-fb9235e681e4": return "finance";

        }
        return "income";
    }
}

//[System.Serializable]
//public class BudgetsCollection
//{
//    public Budget[] budgets;
//}

//[System.Serializable]
//public class Budget
//{
//    public float amount;
//    public float transaction_total;
//    public string guid;
//    public string category_guid;
//    public string parent_guid;
//    public string name;
//}

// [System.Serializable]
// public class AccountsCollection
// {
//     public Account[] items;
// }

// [System.Serializable]
// public class Account
// {
//     public string account_subtype;
//     public int account_type;
//     public float apr;
//     public float apy;
//     public float available_balance;
//     public float available_credit;
//     public float balance;
//     public bool is_closed;
//     public float credit_limit;
//     public string day_payment_is_due;
//     public string external_guid;
//     public int feed_account_type;
//     public float feed_apr;
//     public float feed_apy;
//     public string guid;
//     public bool has_monthly_transfer_limit;
//     public string institution_guid;
//     public float interest_rate;
//     public bool is_hidden;
//     public bool is_manual;
//     public bool is_personal;
//     public string matures_on;
//     public string member_guid;
//     public bool member_is_managed_by_user;
//     public float minimum_balance;
//     public int monthly_transfer_count;
//     public string name;
//     public string nickname;
//     public float original_balance;
//     public float payoff_balance;
//     public float pending_balance;
//     public string property_type;
//     public int revision;
//     public string started_on;
//     public float statement_balance;
//     public string user_guid;
//     public float minimum_payment;
//     public string payment_due_at;
// }

// "User data" that gets attached to a bubble
public class BudgetData : MonoBehaviour
{
    public string display;
    public string guid;
    public string category_guid;
}

public class BudgetSpawner : MonoBehaviour
{

//  Windows dll ties.

    // [DllImport("moneymobilex_unity")]
    // private static extern float sanity_check();

    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LoginDelegate(bool success);
    // [DllImport("moneymobilex_unity")]
    // private static extern void login(string username, string password, [MarshalAs(UnmanagedType.FunctionPtr)]LoginDelegate functionCallback);

    // [DllImport("moneymobilex_unity")]
    // private static extern IntPtr getModel(string modelJson);

    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SyncDelegate();
    // [DllImport("moneymobilex_unity")]
    // private static extern void syncModel(string modelName, [MarshalAs(UnmanagedType.FunctionPtr)]SyncDelegate functionCallback);

    // [DllImport("moneymobilex_unity")]
    // private static extern void heartbeat();

    
//  Mac dummy data
    private static float sanity_check() {
        return 0.0f;
    }
    private static void login(string username, string password, LoginDelegate functionCallback) {
        functionCallback(true);
    }
    private static string getModel(string modelJson) {
        return budget_json;
    }
    private static void syncModel(string modelName, SyncDelegate functionCallback) {
        //Do stuff, probably nothing.
        functionCallback();
    }
    private static void heartbeat() {
        //do nothing
    }

    private static BubbleBudget mainBudget;
    private static BubbleBudget subBudget;
    private static List<BudgetsManager.Budget> budgets;
    private static Vector3 spawnerPosition;
    private static string USERNAME = "march17";
    private static string PASSWORD = "anypass";
    private static int nextUpdate = 1;
    private static float lerp = 1.0f;
    private static Vector3 startScale;
    private static Vector3 startPosition;
    private static GameObject colliding;

    public static void SyncCallback()
    {
        // Windows code
        // IntPtr budgets_ptr = getModel("budgets");
        // string budgets_json = Marshal.PtrToStringAnsi(budgets_ptr);

        //Mac code
        string budgets_json = getModel("budgets");
        BudgetsManager manager = (BudgetsManager)GameObject.Find("Managers").GetComponent("BudgetsManager");
        manager.Start();
        budgets = manager.getBudgets();
        Debug.Log("FirstBudgetS Name: " + budgets[0].name + " FirstBudgetS Amount: " + budgets[0].amount);
        Debug.Log("SecondBudgetS Name: " + budgets[1].name + " SecondBudgeS Amount: " + budgets[1].amount);
        //budgets = JsonUtility.FromJson<BudgetsManager.BudgetCollection>(budgets_json);

        Debug.LogFormat("{0}", budgets_json);
        Debug.LogFormat("{0}", "Budget Size:"+budgets.Count);
        mainBudget = new BubbleBudget(spawnerPosition, GetBudgets(""), false /* is sub budget */);
    }

    public static BudgetsManager.Budget[] GetBudgets(string parent_guid)
    {
        BudgetsManager.Budget[] subset = budgets.Where( b => (b.parent_guid == parent_guid || b.guid == parent_guid || (parent_guid == "" && b.parent_guid == null)) && b.name != "Income").ToArray();
        Debug.LogFormat("{0}", "Subset Length: "+subset.Length);
        Array.Sort<BudgetsManager.Budget>(subset, (left, right) => right.amount.CompareTo(left.amount));
        Debug.Log("FirstBudget Name: " + subset[0].name + " FirstBudget Amount: " + subset[0].amount);
        Debug.Log("SecondBudget Name: " + subset[1].name + " SecondBudget Amount: " + subset[1].amount);
        Debug.Log("FirstBudgetB Name: " + budgets[0].name + " FirstBudgetB Amount: " + budgets[0].amount);
        Debug.Log("SecondBudgetB Name: " + budgets[1].name + " SecondBudgetB Amount: " + budgets[1].amount);
        return subset;
    }

    public static void LoginCallback(bool success)
    {
        Debug.LogFormat("Logged in? {0}", success);
        syncModel("budgets", SyncCallback);
    }

    // Use this for initialization
    public void Start()
    {
        Debug.LogFormat("Start!");
        Debug.LogFormat("Called sanity_check() => {0}", sanity_check());
        login(USERNAME, PASSWORD, LoginCallback);
        spawnerPosition = this.gameObject.transform.position;
    }

    // Update is called once per frame
    public void Update()
    {
        if (mainBudget != null) { mainBudget.Update(colliding); }
        if (subBudget != null)
        {
            lerp += Time.deltaTime * 2.0f;
            if (lerp > 1.0f) lerp = 1.0f;

            subBudget.Animate(startPosition, startScale, lerp);
            subBudget.Update(colliding);
        }

        // Heartbeat every 1 second
        if (Time.time >= nextUpdate)
        {
            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            heartbeat();
        }
    }

    public static void GotoMainBudget()
    {
        if (subBudget != null) subBudget.SetActive(false);
        if (mainBudget != null) mainBudget.SetActive(true);
        subBudget = null;
    }

    public static void GotoSubBudget(GameObject obj)
    {
        if (subBudget != null) return; /* already in sub-budget */

        BudgetData budgetData = obj.GetComponent<BudgetData>() as BudgetData;
        mainBudget.SetActive(false);
        lerp = 0.0f;
        startPosition = obj.transform.position;
        startScale = obj.transform.localScale;
        Debug.Log("GotoSubBudget!!");
        subBudget = new BubbleBudget(spawnerPosition, GetBudgets(budgetData.guid), true /* is sub budget */);
    }

    public static void BudgetBlur()
    {
        colliding = null;
    }

    public static void BudgetHover(GameObject obj)
    {
        colliding = obj;
    }

    private static string budget_json = "{\"budgets\":[{\"budget\": {\"id\": 1, \"amount\": 16, \"category_guid\": \"CAT-7cccbafa-87d7-c9a6-661b-8b3402fe9e78\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-1e0e7bb2-0af8-0a69-02cd-8b7350000096\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Pets\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 2, \"amount\": 134, \"category_guid\": \"CAT-8edf9663-623e-4735-490e-31288f0a70b0\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-d3e113a0-325f-7a26-83da-07dbf6dfa545\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Gifts & Donations\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 3, \"amount\": 98, \"category_guid\": \"CAT-bf5c9cca-c96b-b50d-440d-38d9adfda5b0\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-ecded965-1ad6-4e1c-80d8-b586d791f11d\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Education\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 4, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 4, \"amount\": 4, \"category_guid\": \"CAT-94b11142-e97b-941a-f67f-6e18d246a23f\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-5504874a-ddd3-5871-0b3a-8016eb4f1603\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Business Services\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 5, \"amount\": 140, \"category_guid\": \"CAT-79b02f2f-2adc-88f0-ac2b-4e71ead9cfc8\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-6bad8f8c-c4a3-3370-f3e8-eb79f1a02dd5\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Bills & Utilities\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 6, \"amount\": 3, \"category_guid\": \"CAT-d73ee74b-13a4-ac3e-4015-fc4ba9a62b2a\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-63720105-8756-6de4-2803-6216a9ee578c\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Fees & Charges\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 7, \"amount\": 0, \"category_guid\": \"CAT-ddc9b8e0-a9a1-e31e-a467-2c33e553afd9\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-936c1a24-a0db-4013-3020-17f5a933b8a9\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Home Improvement\", \"parent_guid\": \"BGT-f5843ce5-233f-5dc6-42ec-1cae407bf1e0\", \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 8, \"amount\": 183, \"category_guid\": \"CAT-6c7de3f8-de6c-7061-1dd2-b093044014bf\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-2ddfe567-7f06-ff79-d2cf-ee9976f96cac\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Financial\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 9, \"amount\": 0, \"category_guid\": \"CAT-ee48b740-c981-778b-3c02-04540dec0262\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-e6b04675-5d84-6db8-1af6-5be0d4f7eade\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Dentist\", \"parent_guid\": \"BGT-d484079f-af3b-192a-2413-ea91165681d4\", \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 10, \"amount\": 253, \"category_guid\": \"CAT-ea23d844-cbd1-eb10-f6ac-0df9610e59ae\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-c503f94d-1d06-849f-4e03-d4c3ce58ee17\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Travel\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 2, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 11, \"amount\": 0, \"category_guid\": \"CAT-e671e3a1-b7cb-f0a9-ac6e-bc4632245c31\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-6852bcec-c437-946e-34b4-93591cbd1ed9\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Home Supplies\", \"parent_guid\": \"BGT-f5843ce5-233f-5dc6-42ec-1cae407bf1e0\", \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 12, \"amount\": 134, \"category_guid\": \"CAT-aad51b46-d6f7-3da5-fd6e-492328b3023f\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-3a98bb41-610d-fe22-a334-e8953631d083\", \"is_deleted\": null, \"is_exceeded\": false,"+
    " \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Shopping\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 13, \"amount\": 21, \"category_guid\": \"CAT-0cb1d99d-f558-99e3-2282-b31f359b411a\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-7ddce3d1-a0a4-2450-da4a-d1f8720aa90f\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Kids\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 14, \"amount\": 164, \"category_guid\": \"CAT-52fa4693-c088-afb2-2a99-7bc39bb23a0f\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-d484079f-af3b-192a-2413-ea91165681d4\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Health & Fitness\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 15, \"amount\": 304, \"category_guid\": \"CAT-bd56d35a-a9a7-6e10-66c1-5b9cc1b6c81a\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-3f35e6a8-2727-6409-45ab-217b00fa3497\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Food & Dining\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 2, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 16, \"amount\": 25, \"category_guid\": \"CAT-e5154228-fe45-790d-a280-f6bf5ae5ac9f\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-b05144b6-c18e-2d28-e4c0-48ae8ac7239b\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Personal Care\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 17, \"amount\": 227, \"category_guid\": \"CAT-7829f71c-2e8c-afa5-2f55-fa3634b89874\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-afec2eb5-26a3-f816-35ca-f7868c6739bd\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Auto & Transport\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 24.68, \"projected_transaction_total_ratio\": null, \"revision\": 5, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 18, \"amount\": 501, \"category_guid\": \"CAT-b709172b-4eb7-318e-3b5d-e0f0500b32ac\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-f5843ce5-233f-5dc6-42ec-1cae407bf1e0\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Home\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 3, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 19, \"amount\": 25, \"category_guid\": \"CAT-e04e9d1e-e041-c315-2e50-094143ab3f73\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-697b62de-3f40-fa64-7ab9-1e7e201808f4\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": false, \"is_off_track\": false, \"metadata\": null, \"name\": \"Entertainment\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}},{\"budget\": {\"id\": 20, \"amount\": 6200, \"category_guid\": \"CAT-bf9f3294-4c40-1677-d269-54fbc189faf3\", \"client_guid\": null, \"created_at\": 1536183109, \"deleted_at\": null, \"end_date\": null, \"external_guid\": null, \"external_id\": null, \"guid\": \"BGT-1feae2ed-89ac-7aa5-460d-b7e8285b9119\", \"is_deleted\": null, \"is_exceeded\": false, \"is_income\": true, \"is_off_track\": false, \"metadata\": null, \"name\": \"Income\", \"parent_guid\": null, \"projected_spending\": null, \"projected_transaction_total\": 0, \"projected_transaction_total_ratio\": null, \"revision\": 1, \"start_date\": null, \"status_code\": null, \"top_level_category_guid\": null, \"transaction_total\": 0, \"updated_at\": 1536183109, \"user_guid\": null}}]}";
}