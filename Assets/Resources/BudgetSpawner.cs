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

    public BubbleBudget(Vector3 parentPosition, Budget[] budgets, bool isSubBudget)
    {
        spawnerPosition = parentPosition;
        icons = new List<GameObject>();
        spheres = new List<GameObject>();

        Budget budget = budgets[0];
        float max = budget.amount;

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

    private GameObject CreateBubble(float scale, Budget budget)
    {
        float percent_spent = budget.transaction_total / budget.amount;

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

[System.Serializable]
public class BudgetsCollection
{
    public Budget[] items;
}

[System.Serializable]
public class Budget
{
    public float amount;
    public float transaction_total;
    public string guid;
    public string category_guid;
    public string parent_guid;
    public string name;
}

// "User data" that gets attached to a bubble
public class BudgetData : MonoBehaviour
{
    public string display;
    public string guid;
    public string category_guid;
}

public class BudgetSpawner : MonoBehaviour
{

    [DllImport("moneymobilex_unity")]
    private static extern float sanity_check();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LoginDelegate(bool success);
    [DllImport("moneymobilex_unity")]
    private static extern void login(string username, string password, [MarshalAs(UnmanagedType.FunctionPtr)]LoginDelegate functionCallback);

    [DllImport("moneymobilex_unity")]
    private static extern IntPtr getModel(string modelJson);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SyncDelegate();
    [DllImport("moneymobilex_unity")]
    private static extern void syncModel(string modelName, [MarshalAs(UnmanagedType.FunctionPtr)]SyncDelegate functionCallback);

    [DllImport("moneymobilex_unity")]
    private static extern void heartbeat();

    private static BubbleBudget mainBudget;
    private static BubbleBudget subBudget;
    private static BudgetsCollection budgets;
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
        IntPtr budgets_ptr = getModel("budgets");
        string budgets_json = Marshal.PtrToStringAnsi(budgets_ptr);
        budgets = JsonUtility.FromJson<BudgetsCollection>(budgets_json);

        Debug.LogFormat("{0}", budgets_json);
        mainBudget = new BubbleBudget(spawnerPosition, GetBudgets(""), false /* is sub budget */);
    }

    public static Budget[] GetBudgets(string parent_guid)
    {
        Budget[] subset = budgets.items.Where(b => (b.parent_guid == parent_guid || b.guid == parent_guid) && b.name != "Income").ToArray();
        Array.Sort<Budget>(subset, (left, right) => right.amount.CompareTo(left.amount));
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
}