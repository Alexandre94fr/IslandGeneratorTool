using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace IslandGenerator
{
    public class IslandGeneratorEditorWindow : EditorWindow
    {
        struct FieldInformations
        {
            public string Type;
            public string Name;
            public object Value;
            public object DeclaringObject;
            public FieldInfo FieldInfo;

            public FieldInformations(string p_type, string p_name, object p_value, object p_declaringObject, FieldInfo p_fieldInfo) : this()
            {
                Type = p_type;
                Name = p_name;
                Value = p_value;
                DeclaringObject = p_declaringObject;
                FieldInfo = p_fieldInfo;
            }
        }

        #region -= Variables =-

        #region - Generator's parameters -

        [EditorHeader("Generator")]
        public int VariableFieldOffsetInPixel = 5;

        [EditorHeader("Scriptable object")]
        public string IslandDefaultName = "MyIsland";
        public string ScriptableObjectNamePrefix = "SO_";
        public bool CanOverrideExistingScriptableObject = false;

        [EditorHeader("File browering")]
        public string FileWindowDefaultStartPosition = "Assets";
        public string FileWindowDefaultScriptableObjectPath = "IslandData";

        [EditorHeader("After creation")]
        public bool DoesSelectScriptableObjectAfterCreation = true;
        #endregion

        int _previousVariableFieldOffsetInPixel;

        // ScriptableObject and Generator's parameters saved fields
        BindingFlags _classBindingFlags;
        List<FieldInformations> _classFieldsInformations = new();
        int _classLongestFieldNameWidth;

        BindingFlags _scriptableObjectBindingFlags;
        List<FieldInformations> _scriptableObjectFieldsInformations = new();
        int _scriptableObjectLongestFieldNameWidth;


        Dictionary<Type, Func<FieldInformations, object>> _typeToDrawDictionnary;
        
        // Dictionary to store ReorderableLists and avoid recreating them for each frame
        static Dictionary<string, ReorderableList> _reorderableLists = new();

        Vector2 _scrollViewSize = new();
        #endregion

        #region -= Methods =-

        [MenuItem("Tools/Island generator")]
        public static EditorWindow ShowWindow()
        {
            EditorWindow islandGeneratorEditorWindow = GetWindow<IslandGeneratorEditorWindow>("Island generator");

            islandGeneratorEditorWindow.titleContent = new GUIContent("Island generator", EditorGUIUtility.IconContent("LightmapParameters On Icon").image);

            return islandGeneratorEditorWindow;
        }

        void OnEnable()
        {
            _previousVariableFieldOffsetInPixel = VariableFieldOffsetInPixel;

            SetClassVariables();
        }

        void SetClassVariables()
        {
            // Getting and saving class' fields informations
            _classBindingFlags = BindingFlags.Instance | BindingFlags.Public;

            _classFieldsInformations = GetClassFields(this, _classBindingFlags, out _classLongestFieldNameWidth);

            // Getting and saving ScriptableObject's fields informations
            IslandData islandGenerator = CreateInstance<IslandData>();
            _scriptableObjectBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            _scriptableObjectFieldsInformations = GetClassFields(islandGenerator, _scriptableObjectBindingFlags, out _scriptableObjectLongestFieldNameWidth);

            // Setting type to draw methods dictionnary
            _typeToDrawDictionnary = new() {

                { typeof(string), DrawStringField },
                { typeof(bool), DrawBoolField },
                { typeof(int), DrawIntField },
                { typeof(float), DrawFloatField },
                { typeof(Vector2), DrawVector2RangeField },
                { typeof(Vector2Int), DrawVector2IntField },
                { typeof(Vector3), DrawVector3Field },
                { typeof(Vector3Int), DrawVector3IntField },
                { typeof(List<Vector2>), DrawListVector2RangeField },
                { typeof(ScriptableObject), DrawScriptableObjectField },
            };
        }

        List<FieldInformations> GetClassFields<ClassType>(ClassType p_classInstance, BindingFlags p_bindingFlags, out int p_longestFieldNameWidth)
        {
            List<FieldInformations> fieldsInformations = new();
            p_longestFieldNameWidth = 0;

            FieldInfo[] fields = typeof(ClassType).GetFields(p_bindingFlags);

            // If you are here because there is a NullException :
            // Appends when you keep the EditorWindow openned, change the code, and come back in Unity
            // The error append because Unity haven't initialize those variables (EditorStyles's variables) yet

            // In that case just close, and re-open the Window 
            GUIStyle labelStyle = new(EditorStyles.label);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo fieldInformations = fields[i];

                #region Handling field value

                object fieldValue = fieldInformations.GetValue(p_classInstance);

                // If field fieldValue is null we initialize it
                if (fieldValue == null)
                {
                    ConstructorInfo[] constructors = fieldInformations.FieldType.GetConstructors();

                    // Check if there is a parameterless constructor
                    bool hasParameterlessConstructor = constructors.Any(c => c.GetParameters().Length == 0);

                    if (hasParameterlessConstructor)
                    {
                        // To be able to initialize the correct type of fieldValue we can't use 'new()', so we will create an Instance with the correct type
                        fieldValue = Activator.CreateInstance(fieldInformations.FieldType);
                        fieldInformations.SetValue(p_classInstance, fieldValue);
                    }
                    else
                    {
                        Debug.LogError($"ERROR ! Can't initialize automaticly the {fieldInformations.Name}'s fieldValue. The fieldValue stay 'null'.");
                    }
                }
                #endregion

                fieldsInformations.Add(new FieldInformations(
                     fieldInformations.FieldType.ToString(),
                     fieldInformations.Name,
                     fieldValue,
                     p_classInstance,
                     fieldInformations
                ));

                // Getting the width of the field's name
                int fieldWidth = (int)labelStyle.CalcSize(new GUIContent(fieldInformations.Name)).x;

                if (fieldWidth > p_longestFieldNameWidth)
                    p_longestFieldNameWidth = fieldWidth + VariableFieldOffsetInPixel;
            }

            return fieldsInformations;
        }

        #region - Draw field methods -

        #region Language types

        object DrawStringField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.TextField(p_fieldInformations.Name, p_fieldInformations.Value.ToString());
        }

        object DrawBoolField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.Toggle(p_fieldInformations.Name, (bool)p_fieldInformations.Value);
        }

        object DrawIntField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.IntField(p_fieldInformations.Name, Convert.ToInt32(p_fieldInformations.Value));
        }

        object DrawFloatField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.FloatField(p_fieldInformations.Name, Convert.ToSingle(p_fieldInformations.Value));
        }
        #endregion

        #region Vectors

        #region Not list

        object DrawVector2Field(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.Vector2Field(p_fieldInformations.Name, (Vector2)p_fieldInformations.Value);
        }

        object DrawVector2IntField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.Vector2IntField(p_fieldInformations.Name, (Vector2Int)p_fieldInformations.Value);
        }

        object DrawVector3Field(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.Vector3Field(p_fieldInformations.Name, (Vector3)p_fieldInformations.Value);
        }

        object DrawVector3IntField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.Vector3IntField(p_fieldInformations.Name, (Vector3Int)p_fieldInformations.Value);
        }

        #region Attributes

        object DrawVector2RangeField(FieldInformations p_fieldInformations)
        {
            Vector2RangeAttribute rangeAttribute = p_fieldInformations.FieldInfo.GetCustomAttribute<Vector2RangeAttribute>();

            // If the field does not have the Vector2RangeAttribute, we use the DrawVector2Field method
            if (rangeAttribute == null)
                return DrawVector2Field(p_fieldInformations);

            EditorGUILayout.LabelField(p_fieldInformations.Name);

            Vector2 fieldValue = (Vector2)p_fieldInformations.Value;

            #region Value modification with the Vector2Field

            // Add a Vector2Field to manually input values
            EditorGUI.BeginChangeCheck();

            Vector2 inputValue = EditorGUILayout.Vector2Field("", fieldValue);

            if (EditorGUI.EndChangeCheck())
            {
                inputValue.x = Mathf.Clamp(inputValue.x, rangeAttribute.Min.x, rangeAttribute.Max.x);
                inputValue.y = Mathf.Clamp(inputValue.y, rangeAttribute.Min.y, rangeAttribute.Max.y);
                p_fieldInformations.Value = inputValue;

                // Since the 'DrawListVector2RangeField' method is going to call us and the type it's going to pass us is a List<Vector2>,
                // we can't use the 'SetValue' method, but don't worry the 'DrawListVector2RangeField' method will save the value for us.
                if (p_fieldInformations.FieldInfo.FieldType == typeof(Vector2))
                    p_fieldInformations.FieldInfo.SetValue(p_fieldInformations.DeclaringObject, inputValue);
            }
            #endregion

            #region Drawing of the graph

            // Drawing the square background of the graph
            float graphSize = Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS;

            Rect graphRect = GUILayoutUtility.GetRect(graphSize, graphSize, GUILayout.Width(graphSize), GUILayout.Height(graphSize));

            GUI.Box(graphRect, "");

            // Draw axes
            Handles.color = Vector2RangeDrawer.GRAPH_LINE_COLOR;

            // X-axis
            Handles.DrawLine(
                new Vector3(graphRect.x + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS * (1 - Vector2RangeDrawer.GRAPH_LINES_LENGHT_FACTOR), graphRect.y + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS / 2),
                new Vector3(graphRect.x + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS * Vector2RangeDrawer.GRAPH_LINES_LENGHT_FACTOR, graphRect.y + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS / 2)
            );

            // Y-axis
            Handles.DrawLine(
                new Vector3(graphRect.x + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS / 2, graphRect.y + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS * (1 - Vector2RangeDrawer.GRAPH_LINES_LENGHT_FACTOR)),
                new Vector3(graphRect.x + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS / 2, graphRect.y + Vector2RangeDrawer.GRAPH_SIZE_IN_PIXELS * Vector2RangeDrawer.GRAPH_LINES_LENGHT_FACTOR)
            );


            // Normalize the Vector2 coordinates relative to the graph
            Vector2 normalizedPosition = new(
                Mathf.InverseLerp(rangeAttribute.Min.x, rangeAttribute.Max.x, fieldValue.x),
                Mathf.InverseLerp(rangeAttribute.Min.y, rangeAttribute.Max.y, fieldValue.y)
            );

            Vector2 pointPosition = new(
                graphRect.x + normalizedPosition.x * graphSize,
                graphRect.y + (1 - normalizedPosition.y) * graphSize
            );

            // Draw the point representing the current fieldValue
            Handles.color = Vector2RangeDrawer.GRAPH_POINT_COLOR;
            Handles.DrawSolidDisc(pointPosition, Vector3.forward, Vector2RangeDrawer.GRAPH_POINT_RADIUS_IN_PIXELS);
            #endregion

            #region Value modification with the graph

            // Handle mouse click or drag to update the fieldValue
            Event mouseEvent = Event.current;
            if ((mouseEvent.type == EventType.MouseDown || mouseEvent.type == EventType.MouseDrag) && graphRect.Contains(mouseEvent.mousePosition))
            {
                Vector2 newNormalizedPosition = new(
                    Mathf.Clamp01((mouseEvent.mousePosition.x - graphRect.x) / graphSize),
                    Mathf.Clamp01(1 - (mouseEvent.mousePosition.y - graphRect.y) / graphSize)
                );

                Vector2 newValue = new(
                    Mathf.Lerp(rangeAttribute.Min.x, rangeAttribute.Max.x, newNormalizedPosition.x),
                    Mathf.Lerp(rangeAttribute.Min.y, rangeAttribute.Max.y, newNormalizedPosition.y)
                );

                // Handle the case where p_fieldInformations's type is a List<Vector2>
                if (p_fieldInformations.FieldInfo.FieldType == typeof(List<Vector2>))
                {
                    Event.current.Use();
                    return newValue;
                }

                // Apply the modification via FieldInfo
                p_fieldInformations.FieldInfo.SetValue(p_fieldInformations.DeclaringObject, newValue);
                p_fieldInformations.Value = newValue;

                Event.current.Use();
            }
            #endregion

            return p_fieldInformations.Value;
        }

        #endregion

        #endregion

        #region List

        object DrawListVector2Field(FieldInformations p_fieldInformations)
        {
            List<Vector2> list = p_fieldInformations.Value as List<Vector2>;

            if (list == null) return p_fieldInformations.Value;

            // Show list name
            EditorGUILayout.LabelField(p_fieldInformations.Name);

            // Show each element of the list
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = EditorGUILayout.Vector2Field($"Element {i}", list[i]);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Element"))
            {
                list.Add(default);
            }

            if (list.Count > 0 && GUILayout.Button("Remove Last Element"))
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        object DrawListVector2RangeField(FieldInformations p_fieldInformations)
        {
            Vector2RangeAttribute rangeAttribute = p_fieldInformations.FieldInfo.GetCustomAttribute<Vector2RangeAttribute>();

            // If the p_fieldInformations.Value is equal to 'null'
            if (p_fieldInformations.Value is not List<Vector2> vector2List)
                vector2List = new List<Vector2>();

            if (rangeAttribute == null)
                return DrawListVector2Field(p_fieldInformations);

            EditorGUILayout.LabelField(p_fieldInformations.Name);

            // Displaying current values
            for (int i = 0; i < vector2List.Count; i++)
            {
                // Using DrawVector2RangeField to apply constraints if the attribute is present
                Vector2 newValue = rangeAttribute != null ? (Vector2)DrawVector2RangeField(
                    new FieldInformations(
                        typeof(Vector2).ToString(),
                        p_fieldInformations.Name + $" [{i}]",
                        vector2List[i],
                        p_fieldInformations.DeclaringObject,
                        p_fieldInformations.FieldInfo
                    )
                ) : EditorGUILayout.Vector2Field($" [{i}]", vector2List[i]);

                vector2List[i] = newValue;
            }

            #region Add/remove buttons

            GUILayout.BeginHorizontal();

            // Bouton pour ajouter un élément
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                vector2List.Add(Vector2.zero);
            }

            // Bouton pour supprimer un élément
            if (GUILayout.Button("-", GUILayout.Width(25)) && vector2List.Count > 0)
            {
                vector2List.RemoveAt(vector2List.Count - 1);
            }

            GUILayout.EndHorizontal();
            #endregion

            // Updating values in p_fieldInformations
            p_fieldInformations.Value = vector2List;
            p_fieldInformations.FieldInfo.SetValue(p_fieldInformations.DeclaringObject, vector2List);

            return vector2List;
        }

        object DrawListVector2IntField(FieldInformations p_fieldInformations)
        {
            Debug.LogError("ERROR ! Not implemented.");
            return null;
        }

        object DrawListVector3Field(FieldInformations p_fieldInformations)
        {
            Debug.LogError("ERROR ! Not implemented.");
            return null;
        }

        object DrawListVector3IntField(FieldInformations p_fieldInformations)
        {
            Debug.LogError("ERROR ! Not implemented.");
            return null;
        }
        #endregion

        #endregion

        #region Unity types

        object DrawScriptableObjectField(FieldInformations p_fieldInformations)
        {
            return EditorGUILayout.ObjectField(
                p_fieldInformations.Name,
                p_fieldInformations.FieldInfo.GetValue(p_fieldInformations) as ScriptableObject,
                p_fieldInformations.FieldInfo.FieldType,
                false
            );
        }
        #endregion

        #endregion

        void OnGUI()
        {
            #region Title

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Island generator", GUIStyleUtility.TitleText);
            EditorGUILayout.Space(45);

            // Little bar
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            #endregion

            _scrollViewSize = EditorGUILayout.BeginScrollView(_scrollViewSize);

            #region Generator's variables showing / saving

            EditorGUILayout.LabelField("    Generator's parameters :", GUIStyleUtility.LittleHeaderText);
            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = _classLongestFieldNameWidth;

            // If the variable 'VariableFieldOffsetInPixel' has changed we relaunch SetClassVariables 
            if (_previousVariableFieldOffsetInPixel != VariableFieldOffsetInPixel)
            {
                _previousVariableFieldOffsetInPixel = VariableFieldOffsetInPixel;
                SetClassVariables();
            }

            ShowFields(ref _classFieldsInformations, true);

            EditorGUILayout.Space();
            #endregion

            #region ScriptableObject's variables showing / saving

            // Little bar
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("    ScriptableObject's variables :", GUIStyleUtility.LittleHeaderText);
            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = _scriptableObjectLongestFieldNameWidth;

            ShowFields(ref _scriptableObjectFieldsInformations, false);
            #endregion

            EditorGUILayout.EndScrollView();

            // All GUI that will follows this line will be at the buttom of the Editor window
            GUILayout.FlexibleSpace();

            // Creates a button, and what appends if he is pressed
            if (GUILayout.Button(new GUIContent("Create new island", "If pressed, will create a new ScriptableObject insind the given folder path.")))
            {
                SaveIslandDataToDisk();
            }
        }

        /// <summary> Shows the given field informations, those shown fields can be modified by the user through the EditorWindow.
        /// </summary>
        /// <param name = "p_fieldInformations"> The reference of the field informations that will be shown and modified by the user. </param>
        /// <param name = "p_doesChangeFieldsOriginalValueToo"> If true, will let the declaring object's original fieldValue be modified as well. </param>
        void ShowFields(ref List<FieldInformations> p_fieldInformations, bool p_doesChangeFieldsOriginalValueToo)
        {
            for (int i = 0; i < p_fieldInformations.Count; i++)
            {
                FieldInformations fieldInformations = p_fieldInformations[i];

                // Security
                if (fieldInformations.Value == null)
                {
                    Debug.LogError(
                        $"ERROR ! The field fieldValue of {fieldInformations.Name} is null. " +
                        "That's quite strange because the method named 'GetClassFields' should have initialized the fieldValue if it was 'null', or have printed out an error."
                    );
                    break;
                }

                Type fieldType = fieldInformations.Value.GetType();

                // Security
                if (fieldType == null)
                {
                    Debug.LogError($"ERROR ! The field type of {fieldInformations.Name} is null.");
                    break;
                }

                // Showing "Header" style text if the field has the EditorHeaderAttribute
                EditorHeaderAttribute editorHeaderAttribute = fieldInformations.FieldInfo.GetCustomAttribute<EditorHeaderAttribute>();
                if (editorHeaderAttribute != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($" {editorHeaderAttribute.HeaderText}", EditorStyles.boldLabel);
                }

                // Showing the fields (if the fields' type is planned in the _typeToDrawDictionnary)
                if (_typeToDrawDictionnary.TryGetValue(fieldType, out Func<FieldInformations, object> drawMethod))
                {
                    // Calling the draw method
                    object newValue = drawMethod.Invoke(fieldInformations);

                    // Saving the new fieldValue if changed
                    if (!Equals(newValue, fieldInformations))
                    {
                        fieldInformations.Value = newValue;
                        p_fieldInformations[i] = fieldInformations;

                        if (p_doesChangeFieldsOriginalValueToo)
                        {
                            // Will update the original fieldValue of the declaring class' variable
                            fieldInformations.FieldInfo.SetValue(fieldInformations.DeclaringObject, newValue);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"WARNINIG ! The field '{fieldInformations.Name}' can't be shown in the EditorWindow " +
                        $"because his type '{fieldInformations.Type}' is not planned in the {nameof(_typeToDrawDictionnary)}.",
                        MessageType.Warning
                    );

                    Debug.LogWarning(
                        $"WARNINIG ! The field '{fieldInformations.Name}' can't be shown in the EditorWindow " +
                        $"because his type '{fieldInformations.Type}' is not planned in the {nameof(_typeToDrawDictionnary)}."
                    );
                }
            }
        }

        void SaveIslandDataToDisk()
        {
            // Getting island's name from the EditorWindow's 'Name' field
            string islandName = IslandDefaultName;

            foreach (FieldInformations fieldInformations in _scriptableObjectFieldsInformations)
            {
                if (fieldInformations.Name == "Name")
                {
                    if (fieldInformations.Value is null || fieldInformations.Value is "")
                    {
                        break;
                    }

                    // We use ToString() in case the developper has created a variable called 'Name' which is not a 'string' type
                    islandName = fieldInformations.Value.ToString();
                    break;
                }
            }

            // Selection of the ScriptableObject parent
            string folderPath = EditorUtility.OpenFolderPanel("Please choose a folder to put your island's data in", FileWindowDefaultStartPosition, FileWindowDefaultScriptableObjectPath);

            // If the developer close the folder panel
            if (string.IsNullOrEmpty(folderPath))
                return;

            folderPath = FileUtil.GetProjectRelativePath(folderPath);

            // In case the developer creates new folders, we actualize Unity's assets, so Unity can detects those new folders
            AssetDatabase.Refresh();

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"ERROR ! Invalid chosen file, be sure to choose a file insind the Unity's 'Assets' file, 'Assets' included.");
                return;
            }

            // Adding the prefix to the ScriptableObject
            islandName = ScriptableObjectNamePrefix + islandName;

            string assetPath = $"{folderPath}/{islandName}.asset";

            // Verifying duplicates, can replace the existing file if CanOverrideExistingScriptableObject is true
            if (AssetDatabase.LoadAssetAtPath<IslandData>(assetPath) != null && !CanOverrideExistingScriptableObject)
            {
                Debug.LogWarning($"WARNING ! A ScriptableObject with this name '{islandName}' already exists insind {folderPath}.");
                return;
            }

            // Creating the ScriptableObject
            IslandData newIslandData = CreateInstance<IslandData>();

            foreach (var field in _scriptableObjectFieldsInformations)
            {
                FieldInfo fieldInfo = typeof(IslandData).GetField(field.Name, _scriptableObjectBindingFlags);

                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(newIslandData, field.Value);
                }
            }

            // Saves the created ScriptableObject (Asset)
            AssetDatabase.CreateAsset(newIslandData, assetPath);
            AssetDatabase.SaveAssets();

            // If the Generator parameter 'DoesSelectScriptableObjectAfterCreation' is at true, then it will select the created asset
            if (DoesSelectScriptableObjectAfterCreation)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newIslandData;
            }

            Debug.Log($"Success ! The ScriptableObject has been correctly created at : {assetPath}");
        }
        #endregion
    }
}