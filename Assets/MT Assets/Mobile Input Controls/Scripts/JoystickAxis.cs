#if UNITY_EDITOR
    using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MTAssets.MobileInputControls
{
    /*
     This class is responsible for the functioning of the "Joystick Axis" component, and all its functions.
    */
    /*
     * The Mobile Input Controls was developed by Marcos Tomaz in 2019.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    public class JoystickAxis : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private float timeStartMoveAxisToCenter;

        [HideInInspector]
        public GameObject joystickBackgroundGameObject;
        [HideInInspector]
        public Image joystickBackgroundImage;
        [HideInInspector]
        public GameObject joystickAxisGameObject;
        [HideInInspector]
        public Image joystickAxisImage;
        [HideInInspector]
        public float joystickBackgroundSize = 150f;
        [HideInInspector]
        public float joystickAxisSize = 80f;
        [HideInInspector]
        public float axisResistance = 3f;
        [HideInInspector]
        public float timeToMoveAxisToCenter = 0.15f;
        [HideInInspector]
        public Vector2 inputVector;
        [HideInInspector]
        public int currentFingerId = -1;
        [HideInInspector]
        public bool pressed = false;

#if UNITY_EDITOR
        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(JoystickAxis))]
        public class CustomInspector : UnityEditor.Editor
        {
            public Rect GetInspectorWindowSize()
            {
                //Returns the current size of inspector window
                return EditorGUILayout.GetControlRect(true, 0f);
            }

            public override void OnInspectorGUI()
            {
                //Start the undo event support, draw default inspector and monitor of changes
                DrawDefaultInspector();
                JoystickAxis script = (JoystickAxis)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");

                //Support reminder
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Remember to read the Mobile Input Controls documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);

                //Verify that base assets exists
                Sprite circle = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Mobile Input Controls/Sprites/Circle.png", typeof(Sprite));
                if (circle == null)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Unable to run this component. The base assets required for installing this component were not found in the folder \n\n\"Assets/MT Assets/Mobile Input Controls/Sprites\"\n\nPlease try to re-install Mobile Input Controls to resolve this issue.", MessageType.Error);
                    GUILayout.Space(10);
                    return;
                }

                //Verify if exists the components, if not exists, create them
                bool createdSomething = false;
                if (GetChildWithName(script.gameObject, "Joystick Background") == null)
                {
                    //Create the joystick background
                    script.joystickBackgroundGameObject = new GameObject("Joystick Background");
                    script.joystickBackgroundGameObject.transform.SetParent(script.gameObject.transform);
                    RectTransform rectTransformBg = script.joystickBackgroundGameObject.AddComponent<RectTransform>();
                    rectTransformBg.sizeDelta = new Vector2(150, 150);
                    rectTransformBg.anchorMin = new Vector2(1, 0);
                    rectTransformBg.anchorMax = new Vector2(1, 0);
                    rectTransformBg.pivot = new Vector2(1, 0);
                    rectTransformBg.anchoredPosition = new Vector2(0, 0);
                    script.joystickBackgroundImage = script.joystickBackgroundGameObject.AddComponent<Image>();
                    script.joystickBackgroundImage.sprite = circle;

                    createdSomething = true;
                }
                if (GetChildWithName(script.joystickBackgroundGameObject, "Joystick Axis") == null)
                {
                    //Create the joystick axis
                    script.joystickAxisGameObject = new GameObject("Joystick Axis");
                    script.joystickAxisGameObject.transform.SetParent(script.joystickBackgroundGameObject.transform);
                    RectTransform rectTransformAxis = script.joystickAxisGameObject.AddComponent<RectTransform>();
                    rectTransformAxis.sizeDelta = new Vector2(80, 80);
                    rectTransformAxis.anchoredPosition = new Vector2(0, 0);
                    script.joystickAxisImage = script.joystickAxisGameObject.AddComponent<Image>();
                    script.joystickAxisImage.sprite = circle;

                    createdSomething = true;
                }
                if(createdSomething == true)
                {
                    //Show warning
                    EditorUtility.DisplayDialog("All right!", "The component was successfully configured! Save the changes to your scene by pressing \"Ctrl + S\" or \"Cmd + S\" to view the changes.", "Ok!");
                }

                //Validate the components
                if(script.joystickAxisGameObject == null || script.joystickAxisImage == null ||
                    script.joystickBackgroundGameObject == null || script.joystickBackgroundImage == null)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Unable to run this component. Some of the components are configured incorrectly or nonexistent. Please delete all Gameobject affiliated with this Gameobject, this problem will be solved.", MessageType.Error);
                    GUILayout.Space(10);
                    return;
                }

                //Now that all components are validated, execution continues. ----------
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Please do not add child objects to this joystick GameObject. To change the size of the joystick, use the variables below. You can change the sprites by modifying the Images that are affiliated with this object. Also, modify this GameObject only, avoid changing the positions and anchoring of the GameObjects children. Such care is needed to keep everything working properly!", MessageType.Info);
                GUILayout.Space(10);

                //Start of settings
                EditorGUILayout.LabelField("Settings For Joystick Axis", EditorStyles.boldLabel);
                GUILayout.Space(10);

                script.axisResistance = EditorGUILayout.Slider(new GUIContent("Axis Resistance",
                                "The resistance of the axis in relation to the movement of the player's finger. The higher the value here, the farther from the center, the axis can move."),
                                script.axisResistance, 1f, 5f);

                script.timeToMoveAxisToCenter = EditorGUILayout.Slider(new GUIContent("Time To Centralize",
                                "The time it will take for the axis to return to the center of the Joystick when the player releases his finger."),
                                script.timeToMoveAxisToCenter, 0.001f, 1f);

                script.joystickBackgroundSize = EditorGUILayout.Slider(new GUIContent("Background Size",
                                "Joystick background size."),
                                script.joystickBackgroundSize, 80f, 290f);
                script.joystickBackgroundGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(script.joystickBackgroundSize, script.joystickBackgroundSize);
                script.GetComponent<RectTransform>().sizeDelta = new Vector2(script.joystickBackgroundSize, script.joystickBackgroundSize);

                script.joystickAxisSize = EditorGUILayout.Slider(new GUIContent("Axis Size",
                                "Joystick axis size."),
                                script.joystickAxisSize, 20f, 200f);
                script.joystickAxisGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(script.joystickAxisSize, script.joystickAxisSize);

                GUILayout.Space(10);

                //Start of combiner settings
                EditorGUILayout.LabelField("Joystick Debug", EditorStyles.boldLabel);
                GUILayout.Space(10);

                script.inputVector.x = EditorGUILayout.FloatField(new GUIContent("Input Vector X",
                    "This is the current X-axis (Vertical) value of this Joystick."),
                    script.inputVector.x);

                script.inputVector.y = EditorGUILayout.FloatField(new GUIContent("Input Vector Y",
                    "This is the current Y-axis (Vertical) value of this Joystick."),
                    script.inputVector.y);

                script.currentFingerId = EditorGUILayout.IntField(new GUIContent("Current Finger ID",
                  "ID of the finger on this Joystick Axis"),
                  script.currentFingerId);

                script.pressed = EditorGUILayout.Toggle(new GUIContent("Joystick Pressed",
                    "Is the Joystick pressed now?"),
                    script.pressed);

                //Final space
                GUILayout.Space(10);

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {

                }
            }
        }
        #endregion

        public static GameObject GetChildWithName(GameObject obj, string name)
        {
            Transform trans = obj.transform;
            Transform childTrans = trans.Find(name);
            if (childTrans != null)
            {
                return childTrans.gameObject;
            }
            else
            {
                return null;
            }
        }
#endif

        private void Update()
        {
            //Move the axis to center, if input is zero
            if (inputVector == Vector2.zero && joystickAxisImage != null)
            {
                joystickAxisImage.rectTransform.anchoredPosition = Vector2.Lerp(joystickAxisImage.rectTransform.anchoredPosition, Vector2.zero, (Time.time - timeStartMoveAxisToCenter) / timeToMoveAxisToCenter);
            }
        }

        public virtual void OnDrag(PointerEventData pointerEventData)
        {
            //On drag the axis
            pressed = true;
            currentFingerId = pointerEventData.pointerId;
            Vector2 currentPosition;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackgroundImage.rectTransform, pointerEventData.position, pointerEventData.pressEventCamera, out currentPosition))
            {
                //Calculate current position
                currentPosition.x = (currentPosition.x / joystickBackgroundImage.rectTransform.sizeDelta.x);
                currentPosition.y = (currentPosition.y / joystickBackgroundImage.rectTransform.sizeDelta.y);

                //Calculate input
                inputVector = new Vector2(currentPosition.x * 2 + 1, currentPosition.y * 2 - 1);
                inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

                //Move the axis image
                joystickAxisImage.rectTransform.anchoredPosition = new Vector2(inputVector.x * (joystickBackgroundImage.rectTransform.sizeDelta.x / axisResistance), inputVector.y * (joystickBackgroundImage.rectTransform.sizeDelta.y / axisResistance));
            }
        }

        public virtual void OnPointerDown(PointerEventData pointerEventData)
        {
            //On pointer down
            currentFingerId = pointerEventData.pointerId;
            pressed = true;
            OnDrag(pointerEventData);
        }

        public virtual void OnPointerUp(PointerEventData pointerEventData)
        {
            //On pointer up
            pressed = false;
            currentFingerId = -1;
            inputVector = Vector2.zero;
            timeStartMoveAxisToCenter = Time.time;
        }
    }
}