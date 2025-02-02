using UnityEngine;

namespace IslandGenerator
{
    public static class GUIStyleUtility
    {
        public static GUIStyle TitleText = new(GUI.skin.label)
        {
            fixedHeight = 60,
            fontSize = 50,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new(1, 1, 1, 1)
        };

        public static GUIStyle SubTitleText = new(GUI.skin.label)
        {
            fixedHeight = 50,
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new(1, 1, 1, 1)
        };

        public static GUIStyle SubSubTitleText = new(GUI.skin.label)
        {
            fixedHeight = 40,
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new(1, 1, 1, 1)
        };

        public static GUIStyle CenteredHeaderText = new(GUI.skin.label)
        {
            fixedHeight = 30,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new(1, 1, 1, 1)
        };

        public static GUIStyle HeaderText = new(GUI.skin.label)
        {
            fixedHeight = 30,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            border = new(1, 1, 1, 1)
        };

        public static GUIStyle LittleHeaderText = new(GUI.skin.label)
        {
            fixedHeight = 0,
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            border = new(1, 1, 1, 1)
        };
    }
}