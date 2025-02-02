using UnityEditor;
using UnityEngine;

namespace IslandGenerator
{
    [CustomPropertyDrawer(typeof(Vector2RangeAttribute))]
    public class Vector2RangeDrawer : PropertyDrawer
    {
        public static readonly float GRAPH_SIZE_IN_PIXELS            = 100f;
        public static readonly int GRAPH_Y_POSITION_OFFSET_IN_PIXELS = 25;

        public static readonly float GRAPH_LINES_LENGHT_FACTOR       = 0.9f; // If you go past 0.5f it will change orientation 
        public static readonly Color GRAPH_LINE_COLOR                = Color.gray;

        public static readonly Color GRAPH_POINT_COLOR               = Color.red;
        public static readonly int GRAPH_POINT_RADIUS_IN_PIXELS      = 3;

        public static readonly int VECTOR2_FIELD_HEIGHT_IN_PIXELS    = 20;

        public override void OnGUI(Rect p_position, SerializedProperty p_serializedProperty, GUIContent p_label)
        {
            DrawVector2Range(p_position, p_serializedProperty, p_label, attribute);
        }

        public static void DrawVector2Range(Rect p_position, SerializedProperty p_serializedProperty, GUIContent p_label, PropertyAttribute p_propertyAttribute)
        {
            if (p_serializedProperty.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUILayout.HelpBox($"ERROR ! '{p_label.text}' is not a Vector2 or a List<Vector2> variable. This attribute only works with Vector2 type variables.", MessageType.Error);
                return;
            }

            Vector2RangeAttribute range = (Vector2RangeAttribute)p_propertyAttribute;
            Vector2 value = p_serializedProperty.vector2Value;

            // Editable Vector2Field
            EditorGUI.BeginChangeCheck();
            Vector2 newValue = EditorGUI.Vector2Field(new Rect(p_position.x, p_position.y, p_position.width, VECTOR2_FIELD_HEIGHT_IN_PIXELS), p_serializedProperty.name, value);

            // If the user set a new value by the Vector2Field we clamp it and save it
            if (EditorGUI.EndChangeCheck())
            {
                newValue.x = Mathf.Clamp(newValue.x, range.Min.x, range.Max.x);
                newValue.y = Mathf.Clamp(newValue.y, range.Min.y, range.Max.y);
                p_serializedProperty.vector2Value = newValue;
                p_serializedProperty.serializedObject.ApplyModifiedProperties();
            }

            #region Drawing graph

            // Graph creation
            Rect graphRect = new(p_position.x, p_position.y + GRAPH_Y_POSITION_OFFSET_IN_PIXELS, GRAPH_SIZE_IN_PIXELS, GRAPH_SIZE_IN_PIXELS);
            GUI.Box(graphRect, "");

            #region Draw lines

            Handles.color = GRAPH_LINE_COLOR;

            #region Debugging

            // In case you need to debug the values
            /*
            Vector3 xLinePoint1 = new(graphRect.x + GRAPH_SIZE_IN_PIXELS * (1 - GRAPH_LINES_LENGHT_FACTOR), graphRect.y + GRAPH_SIZE_IN_PIXELS / 2);
            Vector3 xLinePoint2 = new(graphRect.x + GRAPH_SIZE_IN_PIXELS * GRAPH_LINES_LENGHT_FACTOR      , graphRect.y + GRAPH_SIZE_IN_PIXELS / 2);

            Vector3 yLinePoint1 = new(graphRect.x + GRAPH_SIZE_IN_PIXELS / 2, graphRect.y + GRAPH_SIZE_IN_PIXELS * (1 - GRAPH_LINES_LENGHT_FACTOR));
            Vector3 yLinePoint2 = new(graphRect.x + GRAPH_SIZE_IN_PIXELS / 2, graphRect.y + GRAPH_SIZE_IN_PIXELS * GRAPH_LINES_LENGHT_FACTOR);

            // X-axis
            Handles.DrawLine(
                xLinePoint1,
                xLinePoint2
            );

            // Y-axis
            Handles.DrawLine(
                yLinePoint1,
                yLinePoint2
            );
            */
            #endregion

            // X-axis
            Handles.DrawLine(
                new Vector3(graphRect.x + GRAPH_SIZE_IN_PIXELS * (1 - GRAPH_LINES_LENGHT_FACTOR), graphRect.y + GRAPH_SIZE_IN_PIXELS / 2),
                new Vector3(graphRect.x + GRAPH_SIZE_IN_PIXELS * GRAPH_LINES_LENGHT_FACTOR, graphRect.y + GRAPH_SIZE_IN_PIXELS / 2)
            );

            // Y-axis
            Handles.DrawLine(
                new Vector3(graphRect.x + GRAPH_SIZE_IN_PIXELS / 2, graphRect.y + GRAPH_SIZE_IN_PIXELS * (1 - GRAPH_LINES_LENGHT_FACTOR)),
                new Vector3(graphRect.x + GRAPH_SIZE_IN_PIXELS / 2, graphRect.y + GRAPH_SIZE_IN_PIXELS * GRAPH_LINES_LENGHT_FACTOR)
            );
            #endregion

            // Convert the Vector2 coordinates to relative p_position in the graph
            Vector2 normalizedPosition = new(
                Mathf.InverseLerp(range.Min.x, range.Max.x, value.x),
                Mathf.InverseLerp(range.Min.y, range.Max.y, value.y)
            );

            Vector2 pointPosition = new(
                graphRect.x + normalizedPosition.x * GRAPH_SIZE_IN_PIXELS,
                graphRect.y + (1 - normalizedPosition.y) * GRAPH_SIZE_IN_PIXELS
            );

            // Draw the point representing the current p_position
            Handles.color = GRAPH_POINT_COLOR;
            Handles.DrawSolidDisc(pointPosition, Vector3.forward, GRAPH_POINT_RADIUS_IN_PIXELS);
            #endregion

            // Handle mouse click or hold to update value
            Event mouseEvent = Event.current;
            if (mouseEvent.type == EventType.MouseDown || mouseEvent.type == EventType.MouseDrag && graphRect.Contains(mouseEvent.mousePosition))
            {
                Vector2 newNormalizedPosition = new(
                    Mathf.Clamp01((mouseEvent.mousePosition.x - graphRect.x) / GRAPH_SIZE_IN_PIXELS),
                    Mathf.Clamp01(1 - (mouseEvent.mousePosition.y - graphRect.y) / GRAPH_SIZE_IN_PIXELS)
                );

                newValue = new(
                    Mathf.Lerp(range.Min.x, range.Max.x, newNormalizedPosition.x),
                    Mathf.Lerp(range.Min.y, range.Max.y, newNormalizedPosition.y)
                );

                p_serializedProperty.vector2Value = newValue;
                p_serializedProperty.serializedObject.ApplyModifiedProperties();

                Event.current.Use(); // Consume the event
            }

            EditorGUILayout.Space(GRAPH_SIZE_IN_PIXELS + VECTOR2_FIELD_HEIGHT_IN_PIXELS);
        }
    }
}