![IslandGeneratorToolVisual](https://github.com/user-attachments/assets/58c69529-8874-4ccb-9795-f82b17e0cd45)

## ➤ Context of the tool's creation :
- This tool is a study project at La Horde were I had to create a tool in 2.43 weeks (in other words, 17 days | January 14 to January 31 inclusive), keep in mind that project is a coursework, so I could not work on it during the week, only the week-end. I was the only student programmers working on this project.

- This coursework has been created to test our knowledge about :
  - Unity's EditorWindow
  - C# Reflexion
  - Unity's attribute
  - Assets creation insind Unity (ScriptableObject)

## ➤ Concept of the tool :

### DISCLAIMER ▸ This tool does not create an island from scratch, it only creates data (ScriptableObject) that can be used for example of an already existing island to place treasures in it.

You can :
- Make a ScritpableObject (IslandData type).
- Add / remove variables insind the IslandData.cs script, the Tool will automaticly detect the changements.
- Modify directly the behaviour of the Tool (EditorWindow).
- Had even more parameters to the Tool by adding public variable insind the IslandGeneratorEditorWindow.cs script, the Tool will automaticly detect the changements.
- Had the Attribute ```[EditorHeaderAttribute("HeaderText")]``` to create the same effect as the Unity's ```[Header("HeaderText")]``` but insind the tool's EditorWindow.
- Had the Attribute ```[EditorHeaderAttribute("HeaderText")]``` to a Vector2 or List<Vector2> variable to define a range for a Vector2 field and to show a interactable graph of the Vector2 (work in the Unity's inspector, and in the EditorWindow) :

![Vector2RangeAttributeDocumentation](https://github.com/user-attachments/assets/9630f8f5-970f-4bcb-99c6-c18faba4e35a)
![image](https://github.com/user-attachments/assets/97e346e5-c02c-4de4-bf0a-b0d2fcfe3066)
![image](https://github.com/user-attachments/assets/3f4c31c1-b6e8-48d9-8e0a-74f821d847d2)

## ➤ Credits :

### ▸ Programmers :
- [Alexandre RICHARD](https://github.com/Alexandre94fr)

### ▸ External assets
- No external assets used
