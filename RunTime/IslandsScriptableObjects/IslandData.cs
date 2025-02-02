using System.Collections.Generic;
using UnityEngine;

namespace IslandGenerator
{
    public class IslandData : ScriptableObject
    {
        [Header("Secret informations")] 
        [EditorHeader("Secret informations")]
        [SerializeField] string _topSecretInformations  = "Top secret informations";

        [Header("Basic informations")]
        [EditorHeader("Basic informations")]
        public string Name                              = "Island";
        public string Description                       = "Exemple description";

        [Header("Treasure informations")]
        [EditorHeader("Treasure informations")]
        [Vector2Range(-10, -10, 10, 10)]
        public List<Vector2> TresurePositions;

        // You can add more variable if you want, the generator will update automaticly
    }
}