using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static OVRSkeleton;

// TODO organize this class
//[CustomEditor(typeof(GestureCollection))]
public class GestureCreator : EditorWindow
{
    string gestureName = "New Gesture";
    string assetPath = "Assets/Art/Prefabs/Gestures";
    float detectionThreshold = 0.05f;
    [SerializeField] List<Vector3> fingerData = new List<Vector3>();
    SkeletonType skeletonType = SkeletonType.None;
    HandPose handPose = HandPose.Default;

    OVRSkeleton skeleton = null;
    SerializedProperty fingerDataSerialized = null;
    SerializedObject so = null;

    bool disableRec = true;
    Vector2 scrollPos = Vector2.zero;

    [MenuItem("Tools/Rehab Framework/Gesture Creator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GestureCreator));
    }

    private void OnEnable()
    {
        so = new SerializedObject(this);
        fingerDataSerialized = so.FindProperty("fingerData");
    }

    private void Update()
    {
        disableRec = skeleton == null || !skeleton.IsDataHighConfidence;
    }


    void OnGUI()
    {
        GUILayout.Label("Create new hand gesture", EditorStyles.boldLabel);

        EditorGUILayout.Separator();

        gestureName = EditorGUILayout.TextField("Gesture Name", gestureName);
        assetPath = EditorGUILayout.TextField("Asset Path", assetPath);
        EditorGUILayout.HelpBox("Note that if an asset with matching name already exists at location, it will be overriden.", MessageType.Warning);

        EditorGUILayout.Separator();

        EditorGUILayout.HelpBox("Define the threshold for gesture detection. The threshold decides how close the finger positions must be to the recorded gesture to trigger the action. Recommended values is 0.05. You may increase it for higher tolerance or decrease for more precise detection.", MessageType.Info);
        detectionThreshold = EditorGUILayout.FloatField("Detection Threshold", detectionThreshold);

        EditorGUILayout.Separator();

        skeletonType = (SkeletonType)EditorGUILayout.EnumPopup("Recorded Hand", skeletonType);

        handPose = (HandPose)EditorGUILayout.EnumPopup("Choose a hand pose", handPose);

        EditorGUILayout.Separator();

        var msg = "To record the gesture, position the gesture performing hand within the Oculus cameras field of view and press Record Hand Data. Tip: Lay the headset on the table, enter the play mode and use your other hand to record the gesture when it's clearly visible in the scene mode. You can review the gesture later.";

        EditorGUILayout.HelpBox(msg, MessageType.Info);

        FindHandSkeletonInScene();

        EditorGUI.BeginDisabledGroup(disableRec);
        if (GUILayout.Button("Record Hand Pose"))
        {
            foreach (OVRBone bone in skeleton.Bones)
            {
                fingerData.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
            }
            
        }
        EditorGUI.EndDisabledGroup();

        #region HAND POSE DATA
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);


        so = new SerializedObject(this);
        fingerDataSerialized = so.FindProperty("fingerData");
        EditorGUILayout.PropertyField(fingerDataSerialized, true);
        so.ApplyModifiedProperties();


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        #endregion

        Repaint();

        EditorGUILayout.Separator();

        if (GUILayout.Button("Save Gesture"))
        {
            SaveGesture();
            Reset();
        }

        if (GUILayout.Button("Reset"))
        {
            Reset();
        }
    }

    private bool FindHandSkeletonInScene()
    {
        foreach (OVRSkeleton sk in FindObjectsOfType<OVRSkeleton>())
        {
            if (sk.GetSkeletonType().Equals(skeletonType))
            {
                skeleton = sk;
                return true;
            }
        }

        return false;
    }

    // Create new instance of Gesture and save it in assets at specified path
    private void SaveGesture()
    {
        Gesture gesture = CreateInstance<Gesture>();
        gesture.name = gestureName;
        gesture.bonePositions = fingerData;
        gesture.skeletonType = skeletonType;
        gesture.detectionThreshold = detectionThreshold;
        gesture.handPose = handPose;
        AssetDatabase.CreateAsset(gesture, assetPath + "/" + gestureName +".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    private void Reset()
    {
        so = new SerializedObject(this);
        fingerData = new List<Vector3>();
        fingerDataSerialized = so.FindProperty("fingerData");
        skeletonType = SkeletonType.None;
        gestureName = "New Gesture";
    }
}
