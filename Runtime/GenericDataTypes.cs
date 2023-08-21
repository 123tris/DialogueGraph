using System;

namespace DialogueGraph.Runtime
{
    // Node currentNode, int conversationLineIndex, bool returnValue;
    [Serializable] public class CheckEvent : SerializableCallback<string, int, bool> { }
    // Guid currentNode
    [Serializable] public class TriggerEvent : SerializableEvent<string, int> { }

    [Serializable] public class StringIntSerializableDictionary : SerializableDictionary<string, int> { }
    [Serializable] public class NodeDictionary : SerializableDictionary<string, Node> { }
    [Serializable] public class PropertyDictionary : SerializableDictionary<string, Property> { }

    [Serializable] public class ActorDataDictionary : SerializableDictionary<string, ActorData> { }

    [Serializable] public class CheckEventDictionary : SerializableDictionary<string, CheckEvent> { }

    [Serializable] public class TriggerEventDictionary : SerializableDictionary<string, TriggerEvent> { }
}