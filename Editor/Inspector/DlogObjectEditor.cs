using DialogueGraph.Runtime;
using Sirenix.OdinInspector.Editor;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using PropertyType = DialogueGraph.Runtime.PropertyType;

namespace DialogueGraph
{
    [CustomEditor(typeof(RuntimeDialogueGraph))]
    public class DlogObjectEditor : OdinEditor
    {
        RuntimeDialogueGraph graph => (RuntimeDialogueGraph) target;
        public SerializableDictionary<string, ActorData> ActorData = new();
        private bool actorFolded;
        private bool triggersFolded;
        private bool checksFolded;

        public override void OnInspectorGUI()
        {
            BeginVertical("Box");
            LabelField("Dialogue Graph Data", EditorStyles.centeredGreyMiniLabel);
            graph.DlogObject = ObjectField("Dialogue Graph", graph.DlogObject, typeof(DlogObject), false) as DlogObject;
            if (GUI.changed && graph.DlogObject != null)
            {
                Debug.Log("Changed Dialogue Graph Object");
                UpdateRuntimeData(graph.DlogObject);
                Undo.RecordObject(graph, "Updated GraphRuntimeData");
            }

            if (graph.DlogObject == null)
            {
                EndVertical();
                return;
            }

            //Checks:
            actorFolded = Foldout(actorFolded, "Actors");
            if (actorFolded)
            {
                foreach (Property prop in graph.DlogObject.GetActorData())
                {
                    graph.Data.ActorData.TryGetValue(prop.Guid, out ActorData actorData);
                    if (actorData == null)
                        continue;
                    prop.DisplayName = TextField("Actor name", prop.DisplayName);



                    graph.Data.ActorData[prop.Guid].CustomData = ObjectField("Custom Actor Data", actorData.CustomData, typeof(ScriptableObject), false) as ScriptableObject;
                }
            }

            triggersFolded = Foldout(triggersFolded, "Triggers");
            if (triggersFolded)
            {
                int index = -1;
                foreach (Property prop in graph.DlogObject.GetTriggerData())
                {
                    index++;
                    graph.Data.TriggerData.TryGetValue(prop.Guid, out TriggerEvent triggerEvent);
                    if (triggerEvent == null)
                        continue;
                    LabelField(prop.DisplayName);

                    var so = new SerializedObject(target);
                    var serializedData = so.FindProperty("Data");
                    var triggerData = serializedData.FindPropertyRelative("TriggerData.m_values");

                    var property = triggerData.GetArrayElementAtIndex(index);
                    PropertyField(property);
                    so.ApplyModifiedProperties();
                }
            }

            checksFolded = Foldout(checksFolded, "Checks");
            if (checksFolded)
            {
                int index = -1;
                foreach (Property prop in graph.DlogObject.GetCheckData())
                {
                    index++;
                    LabelField(prop.DisplayName);

                    var so = new SerializedObject(target);
                    var serializedData = so.FindProperty("Data");
                    var triggerData = serializedData.FindPropertyRelative("CheckData.m_values");

                    if (triggerData.arraySize <= index)
                        continue;

                    var property = triggerData.GetArrayElementAtIndex(index);
                    PropertyField(property);
                    so.ApplyModifiedProperties();
                }
            }

            //LabelField("Debug");
            //base.OnInspectorGUI();

            EndVertical();

            //graph.DlogObject EditorGUILayout.ObjectField()
        }

        private void UpdateRuntimeData(DlogObject dlogObject)
        {
            graph.Data = new DlogObjectData(); //clear data

            foreach (Property property in graph.DlogObject.Properties)
            {
                if (property.Type == Runtime.PropertyType.Actor)
                {
                    graph.Data.ActorData[property.Guid] = new ActorData(property);
                }

                if (property.Type == PropertyType.Trigger)
                    graph.Data.TriggerData[property.Guid] = new TriggerEvent();

                if (property.Type == PropertyType.Check)
                    graph.Data.CheckData[property.Guid] = new CheckEvent();
            }
        }
    }
}

//namespace DialogueGraph {
//    [CustomEditor(typeof(RuntimeDialogueGraph))]
//    public class DlogObjectEditor : Editor {
//        private RuntimeDialogueGraph dialogueGraph;
//        private UnityEditor.SerializedProperty dlogObjectProperty;
//        private UnityEditor.SerializedProperty dlogObjectAssetGuidProperty;
//        private VisualElement rootElement;

//        private DlogObject DlogObject {
//            get => (DlogObject) dlogObjectProperty.objectReferenceValue;
//            set => dlogObjectProperty.objectReferenceValue = value;
//        }

//        private int currentDataPropertyIndex;
//        private UnityEditor.SerializedProperty currentDataPropertyCache;
//        private UnityEditor.SerializedProperty CurrentDataProperty {
//            get {
//                var currentIndex = dialogueGraph.CurrentIndex;
//                currentDataPropertyCache = serializedObject.FindProperty($"PersistentData.Array.data[{currentIndex}]");
//                return currentDataPropertyCache;
//            }
//        }

//        public void OnEnable() {
//            dialogueGraph = (RuntimeDialogueGraph) target;
//            rootElement = new VisualElement();
//            dlogObjectProperty = serializedObject.FindProperty("DlogObject");
//            dlogObjectAssetGuidProperty = serializedObject.FindProperty("CurrentAssetGuid");

//            var visualTree = Resources.Load<VisualTreeAsset>("Inspector/DlogObjectEditor");
//            visualTree.CloneTree(rootElement);
//            rootElement.AddStyleSheet("Inspector/Styles/DlogObjectEditor");

//            UpdatePersistentData();
//        }

//        public override VisualElement CreateInspectorGUI() {
//            var dlogObjectField = rootElement.Q<ObjectField>("dlogObjectField");
//            var propertiesContainer = rootElement.Q<VisualElement>("propertiesContainer");
//            var invalidContainer = rootElement.Q<VisualElement>("invalidContainer");

//            var clearDataButton = rootElement.Q<Button>("clearData");
//            clearDataButton.RegisterCallback<ClickEvent>(evt => {
//                dialogueGraph.ClearData();
//                UpdateInspectorProperties();
//            });

//            RefreshDlogObjectView(propertiesContainer, invalidContainer);
//            dlogObjectField.objectType = typeof(DlogObject);
//            dlogObjectField.BindProperty(dlogObjectProperty);
//            dlogObjectField.value = dlogObjectProperty.objectReferenceValue;
//            dlogObjectField.RegisterCallback<ChangeEvent<Object>>(evt => {
//                dlogObjectProperty.objectReferenceValue = (DlogObject) evt.newValue;
//                dlogObjectAssetGuidProperty.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(evt.newValue));
//                RefreshDlogObjectView(propertiesContainer, invalidContainer);
//                serializedObject.ApplyModifiedProperties();
//                UpdateInspectorProperties();
//            });
//            UpdateInspectorProperties();
//            return rootElement;
//        }

//        private void RefreshDlogObjectView(VisualElement propertiesContainer, VisualElement invalidContainer) {
//            if (dlogObjectProperty.objectReferenceValue == null) {
//                propertiesContainer.AddToClassList("hidden");
//                invalidContainer.RemoveFromClassList("hidden");
//            } else {
//                propertiesContainer.RemoveFromClassList("hidden");
//                invalidContainer.AddToClassList("hidden");
//            }
//        }

//        private void UpdateInspectorProperties() {
//            if (DlogObject == null) return;
//            var currentData = dialogueGraph.CurrentData;
//            UpdatePersistentData();

//            var actorContainer = rootElement.Q("actorList");
//            actorContainer.AddStyleSheet("Inspector/Styles/ActorEditor");
//            var actorTemplate = Resources.Load<VisualTreeAsset>("Inspector/ActorEditor");
//            actorContainer.Clear();
//            var actorProperties = DlogObject.Properties.Where(property => property.Type == PropertyType.Actor).ToList();

//            // draw actor interface
//            foreach (var actorProperty in actorProperties) {
//                var actorRoot = new VisualElement();
//                actorTemplate.CloneTree(actorRoot);

//                var actorTitle = actorRoot.Q<Label>("actorIdentifier");
//                actorTitle.text = $"{actorProperty.DisplayName}";
//                var actorName = actorRoot.Q<TextField>("actorName");
//                actorName.SetValueWithoutNotify(currentData.ActorData[currentData.ActorDataIndices[actorProperty.Guid]].Name);
//                actorName.RegisterValueChangedCallback(evt => { currentData.ActorData[currentData.ActorDataIndices[actorProperty.Guid]].Name = evt.newValue; });
//                actorName.RegisterCallback<FocusOutEvent>(evt => { serializedObject.ApplyModifiedProperties(); });

//                var customData = actorRoot.Q<ObjectField>("customActorData");
//                customData.objectType = typeof(ScriptableObject);
//                customData.SetValueWithoutNotify(currentData.ActorData[currentData.ActorDataIndices[actorProperty.Guid]].CustomData);
//                customData.RegisterValueChangedCallback(evt => {
//                    currentData.ActorData[currentData.ActorDataIndices[actorProperty.Guid]].CustomData = (ScriptableObject)evt.newValue;
//                    serializedObject.ApplyModifiedProperties();
//                });

//                actorContainer.Add(actorRoot);
//            }

//            var checkContainer = rootElement.Q("checkList");
//            checkContainer.AddStyleSheet("Inspector/Styles/CheckEditor");
//            var checkTemplate = Resources.Load<VisualTreeAsset>("Inspector/CheckEditor");
//            checkContainer.Clear();

//            // draw check interface
//            var checkProperties = DlogObject.Properties.Where(property => property.Type == PropertyType.Check).ToList();
//            foreach (var checkProperty in checkProperties) {
//                var checkRoot = new VisualElement();
//                checkTemplate.CloneTree(checkRoot);

//                var checkTitle = checkRoot.Q<Label>("checkIdentifier");
//                checkTitle.text = checkProperty.DisplayName;

//                var index = currentData.CheckDataIndices[checkProperty.Guid];
//                var property = CurrentDataProperty?.FindPropertyRelative($"CheckData.Array.data[{index}]");
//                if (property != null) {
//                    var checkField = new IMGUIContainer(() => { EditorGUILayout.PropertyField(property); });
//                    checkRoot.Q("checkContainer").Add(checkField);
//                }

//                checkContainer.Add(checkRoot);
//            }

//            var triggerContainer = rootElement.Q("triggerList");
//            triggerContainer.AddStyleSheet("Inspector/Styles/TriggerEditor");
//            var triggerTemplate = Resources.Load<VisualTreeAsset>("Inspector/TriggerEditor");
//            triggerContainer.Clear();

//            // draw trigger interface
//            foreach (var triggerProperty in DlogObject.Properties.Where(property => property.Type == PropertyType.Trigger)) {
//                var triggerRoot = new VisualElement();
//                triggerTemplate.CloneTree(triggerRoot);

//                var triggerTitle = triggerRoot.Q<Label>("triggerIdentifier");
//                triggerTitle.text = triggerProperty.DisplayName;

//                var index = currentData.TriggerDataIndices[triggerProperty.Guid];
//                var property = CurrentDataProperty?.FindPropertyRelative($"TriggerData.Array.data[{index}]");
//                if (property != null) {
//                    var triggerField = new IMGUIContainer(() => { EditorGUILayout.PropertyField(property); });
//                    triggerRoot.Q("triggerContainer").Add(triggerField);
//                }

//                triggerContainer.Add(triggerRoot);
//            }
//        }

//        private void UpdatePersistentData() {
//            if (DlogObject == null) return;
//            var currentData = dialogueGraph.CurrentData;

//            // check if check data is different to persistent data
//            var checkProperties = DlogObject.Properties.Where(property => property.Type == PropertyType.Check).ToList();
//            foreach (var checkProperty in checkProperties) {
//                if (!currentData.CheckDataIndices.ContainsKey(checkProperty.Guid))
//                    currentData.AddCheckEvent(checkProperty.Guid, new CheckEvent());
//            }

//            /*
//         TODO: This would work if you updated the Indices dictionary after deleting
//         TODO: Right now it's kind of a memory leak, as in if you delete a property,
//         TODO: it will remain in memory forever, or until clearing the cache.
//        // Remove properties which no longer exist
//        var keysToRemove = new List<string>();
//        var checkDataKeys = currentData.CheckDataIndices.Keys;
//        foreach (var checkKey in checkDataKeys) {
//            if (!checkProperties.Exists(prop => prop.Guid == checkKey))
//                keysToRemove.Add(checkKey);
//        }

//        int offset = 0;
//        keysToRemove.ForEach(key => {
//            currentData.CheckData.RemoveAt(currentData.CheckDataIndices[key] - offset);
//            currentData.CheckDataIndices.Remove(key);
//            offset++;
//        });
//        */
//            // check if trigger data is different to persistent data
//            var triggerProperties = DlogObject.Properties.Where(property => property.Type == PropertyType.Trigger).ToList();
//            foreach (var triggerProperty in triggerProperties) {
//                if (!currentData.TriggerDataIndices.ContainsKey(triggerProperty.Guid))
//                    currentData.AddTriggerEvent(triggerProperty.Guid, new TriggerEvent());
//            }

//            /*keysToRemove.Clear();
//        var triggerDataKeys = currentData.TriggerDataIndices.Keys.ToList();
//        foreach (var triggerKey in triggerDataKeys) {
//            if (!triggerProperties.Exists(prop => prop.Guid == triggerKey))
//                keysToRemove.Add(triggerKey);
//        }

//        offset = 0;
//        keysToRemove.ForEach(key => {
//            currentData.TriggerData.RemoveAt(currentData.TriggerDataIndices[key] - offset);
//            currentData.TriggerDataIndices.Remove(key);
//            offset++;
//        });*/

//            // check if actor data is different to persistent data
//            var actorProperties = DlogObject.Properties.Where(property => property.Type == PropertyType.Actor).ToList();
//            foreach (var actorProperty in actorProperties) {
//                if (!currentData.ActorDataIndices.ContainsKey(actorProperty.Guid))
//                    currentData.AddActorData(actorProperty.Guid, new ActorData("Empty name", null, actorProperty));
//            }

//            /*keysToRemove.Clear();
//        var actorDataKeys = currentData.ActorDataIndices.Keys.ToList();
//        foreach (var actorKey in actorDataKeys) {
//            if (!actorProperties.Exists(prop => prop.Guid == actorKey))
//                keysToRemove.Add(actorKey);
//        }

//        offset = 0;
//        keysToRemove.ForEach(key => {
//            currentData.ActorData.RemoveAt(currentData.ActorDataIndices[key] - offset);
//            currentData.ActorDataIndices.Remove(key);
//            offset++;
//        });*/
//        }
//    }
//}