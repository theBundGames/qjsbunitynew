﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Reflection;

public class JSComponentGenerator
{
    class Info
    {
        public string signature;
        public string group;

        public Info(string s, string g)
        {
            signature = s;
            group = g;
        }
        string VariableName
        {
            get { return "id" + methodName; }
        }
        public string methodName
        {
            get
            {
                int i = signature.IndexOf('(');
                return signature.Substring(0, i);
            }
        }
        string argList
        {
            get
            {
                cg.args a = new cg.args();
                a.Add(this.VariableName);

                if (signature.IndexOf("()") >= 0)
                    return a.Format(cg.args.ArgsFormat.OnlyList);

                int i = signature.IndexOf('(');
                var content = signature.Substring(i + 1, signature.Length - i - 2); // string in ()
                string[] ps = content.Split(',');
                foreach (var p in ps)
                {
                    a.Add(p.Substring(p.LastIndexOf(' ') + 1));
                }
                return a.Format(cg.args.ArgsFormat.OnlyList);
            }
        }

        public string FunctionDeclaration
        {
            get
            {
                return new StringBuilder().AppendFormat(@"    void {0}
    {{
        callIfExist({1});
    }}
"
                    , this.signature
                    , this.argList)

                    .ToString();
            }
        }
        public string VariableDeclaration
        {
            get { return "    int " + this.VariableName + ";\r\n"; }
        }
        public string GetInfoVariableInit
        {
            get { return new StringBuilder().AppendFormat("        {0} = JSApi.getObjFunction(jsObjID, \"{1}\");\r\n", this.VariableName, this.methodName).ToString(); }
        }
    }

    static Info[] infos = new Info[]
    {
        // already in JSComponent
//      new Info("Awake()"),
//      new Info("Start()"),
//      new Info("OnDestroy()"),

        // Performance killer
        new Info("Update()", "Update"),
        new Info("LateUpdate()", "Update"),
 
        new Info("FixedUpdate()", "FixedUpdate_OnGUI"),
        new Info("OnGUI()", "FixedUpdate_OnGUI"),

        new Info("OnDisable()", "Enable_Visible"),
        new Info("OnEnable()", "Enable_Visible"),
        new Info("OnBecameInvisible()", "Enable_Visible"),
        new Info("OnBecameVisible()", "Enable_Visible"),

        new Info("OnTransformChildrenChanged()", "TransChange"),
        new Info("OnTransformParentChanged()", "TransChange"),

        new Info("OnApplicationFocus(bool focusStatus)", "Application"),
        new Info("OnApplicationPause(bool pauseStatus)", "Application"),
        new Info("OnApplicationQuit()", "Application"),
        new Info("OnAudioFilterRead(float[] data, int channels)", "Application"),
        new Info("OnLevelWasLoaded(int level)", "Application"),

        new Info("OnAnimatorIK(int layerIndex)", "AnimatorIK_Move_JointBreak"),
        new Info("OnAnimatorMove()", "AnimatorIK_Move_JointBreak"),
        new Info("OnJointBreak(float breakForce)", "AnimatorIK_Move_JointBreak"),

        new Info("OnParticleCollision(GameObject other)", "Physics"),
        new Info("OnCollisionEnter(Collision collisionInfo)", "Physics"),
        new Info("OnCollisionEnter2D(Collision2D coll)", "Physics"),
        new Info("OnCollisionExit(Collision collisionInfo)", "Physics"),
        new Info("OnCollisionExit2D(Collision2D coll)", "Physics"),
        new Info("OnCollisionStay(Collision collisionInfo)", "Physics"),
        new Info("OnCollisionStay2D(Collision2D coll)", "Physics"),
        new Info("OnTriggerEnter(Collider other)", "Physics"),
        new Info("OnTriggerEnter2D(Collider2D other)", "Physics"),
        new Info("OnTriggerExit(Collider other)", "Physics"),
        new Info("OnTriggerExit2D(Collider2D other)", "Physics"),
        new Info("OnTriggerStay(Collider other)", "Physics"),
        new Info("OnTriggerStay2D(Collider2D other)", "Physics"),
        new Info("OnControllerColliderHit(ControllerColliderHit hit)", "Physics"),

        new Info("OnConnectedToServer()", "Server"),
        new Info("OnDisconnectedFromServer(NetworkDisconnection info)", "Server"),
        new Info("OnFailedToConnect(NetworkConnectionError error)", "Server"),
        new Info("OnFailedToConnectToMasterServer(NetworkConnectionError info)", "Server"),
        new Info("OnMasterServerEvent(MasterServerEvent msEvent)", "Server"),
        new Info("OnNetworkInstantiate(NetworkMessageInfo info)", "Server"),
        new Info("OnPlayerConnected(NetworkPlayer player)", "Server"),
        new Info("OnPlayerDisconnected(NetworkPlayer player)", "Server"),
        new Info("OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)", "Server"),
        new Info("OnServerInitialized()", "Server"),

        new Info("OnMouseDown()", "Mouse"),
        new Info("OnMouseDrag()", "Mouse"),
        new Info("OnMouseEnter()", "Mouse"),
        new Info("OnMouseExit()", "Mouse"),
        new Info("OnMouseOver()", "Mouse"),
        new Info("OnMouseUp()", "Mouse"),
        new Info("OnMouseUpAsButton()", "Mouse"),

        new Info("OnPostRender()", "Render"),
        new Info("OnPreCull()", "Render"),
        new Info("OnPreRender()", "Render"),
        new Info("OnRenderImage(RenderTexture src, RenderTexture dest)", "Render"),
        new Info("OnRenderObject()", "Render"),
        new Info("OnWillRenderObject()", "Render"),


        // Editor only
        //
        // Reset
        // OnDrawGizmos
        // OnDrawGizmosSelected
        // OnValidate
    };

    public static JSComponent CreateJSComponentInstance(GameObject go, MonoBehaviour behav)
    {
//         JSComponent ret = (JSComponent)go.AddComponent<JSComponent>();
//         return ret;

        Type type = behav.GetType();
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public 
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance
                        // | BindingFlags.Static
                        // | BindingFlags.DeclaredOnly
                        );
        Dictionary<string, bool> dict = new Dictionary<string, bool>();
        foreach (var method in methods)
        {
            if (!dict.ContainsKey(method.Name))
                dict.Add(method.Name, true);
        }
        string className = "JSComponent";
        for (var i = 0; i < infos.Length; /*i++*/)
        {
            if (dict.ContainsKey(infos[i].methodName))
            {
                className += "_" + infos[i].group;
                var j = i + 1;
                while (j < infos.Length && infos[j].group == infos[i].group)
                {
                    j++;
                }
                i = j;
            }
            else
            {
                i++;
            }
        }
        Type jsComponentType = JSDataExchangeMgr.GetTypeByName(className, null);
        if (jsComponentType == null)
        {
            Debug.LogError(type.Name + " JSComponent not found!");
        }
        JSComponent ret = (JSComponent)go.AddComponent(jsComponentType);
        return ret;
    }

    //[MenuItem("JSB/Gen JSComopnents", false, 1000)]
    public static void GenJSComponents()
    {
        //
        // 0 suffix
        // 1 variables declare
        // 2 variables init
        // 3 functions
        //
        string fileFormat = @"//
// Automatically generated by JSComponentGenerator.
//
using UnityEngine;

public class JSComponent{0} : JSComponent
{{
{1}
    protected override void initMemberFunction()
    {{
        base.initMemberFunction();
{2}    }}

{3}
}}";
        // group ->  List<Info>
        var dict = new Dictionary<string, List<Info>>();
        for (var i = 0; i < infos.Length; i++)
        {
            Info info = infos[i];
            List<Info> lst;
            if (!dict.TryGetValue(info.group, out lst))
            {
                lst = dict[info.group] = new List<Info>();
            }
            lst.Add(info);
        }

        // index -> group
        int ind = 0;
        var dict2 = new Dictionary<int, string>();
        foreach (var d in dict)
        {
            dict2[ind++] = d.Key;
        }

        // arr: 0,1,2,...N
        int N = dict.Count;
        int[] arr = new int[N];
        for (var i = 0; i < N; i++)
        {
            arr[i] = i;
        }
        List<int[]>[] arrLstCombination = new List<int[]>[N];

        int C = 0;
        for (var i = 0; i < N; i++)
        {
            arrLstCombination[i] = Algorithms.PermutationAndCombination<int>.GetCombination(arr, i + 1);
            C += arrLstCombination[i].Count;
        }

        bool bContinue = EditorUtility.DisplayDialog("WARNING",
            @"Total files: " + C,
            "Continue",
            "Cancel");

        if (!bContinue)
        {
            Debug.Log("Operation canceled.");
            return;
        }

        for (var i = 0; i < N; i++)
        {
            List<int[]> l = arrLstCombination[i];
            for (var j = 0; j < l.Count; j++)
            {
                // gen a .cs file!

                int[] a = l[j];
                var suffix = string.Empty;
                StringBuilder sbVariableDeclaration = new StringBuilder();
                StringBuilder sbVariableInit = new StringBuilder();
                StringBuilder sbFunctions = new StringBuilder();
                StringBuilder sbFile = new StringBuilder();
                for (var k = 0; k < a.Length; k++)
                {
                    var group = dict2[a[k]];
                    suffix += "_" + group;
                    List<Info> lstInfo = dict[group];
                    foreach (var li in lstInfo)
                    {
                        sbVariableDeclaration.Append(li.VariableDeclaration);
                        sbVariableInit.Append(li.GetInfoVariableInit);
                        sbFunctions.Append(li.FunctionDeclaration);
                    }
                }
                sbFile.AppendFormat(fileFormat, suffix, sbVariableDeclaration, sbVariableInit, sbFunctions);

                string fileName = Application.dataPath + "/JSBinding/Source/JSComponent/Generated/JSComponent" + suffix + ".cs";
                var w = new StreamWriter(fileName, false/* append */, Encoding.UTF8);
                w.Write(sbFile.ToString());
                w.Close();
            }
        }

        AssetDatabase.Refresh();
    }
}
