using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static System.String;

namespace DialogueGraph.Runtime
{
    [AddComponentMenu("Dialogue Graph/Dialogue Graph")]
    public class RuntimeDialogueGraph : MonoBehaviour
    {
        [HideInInspector]
        public DlogObject DlogObject;

        public DlogObjectData Data;

        private bool conversationDone;
        private string currentNodeGuid;

        public void ResetConversation()
        {
            conversationDone = false;
            currentNodeGuid = DlogObject.StartNode;
        }

        public void EndConversation()
        {
            conversationDone = true;
            currentNodeGuid = null;
        }

        public bool IsCurrentNpc()
        {
            var currentNode = DlogObject.NodeDictionary[currentNodeGuid];
            return currentNode.Type == NodeType.NPC;
        }

        public bool IsConversationDone()
        {
            return conversationDone;
        }

        public ActorData GetCurrentActor()
        {
            var currentNode = DlogObject.NodeDictionary[currentNodeGuid];
            if (currentNode.Type != NodeType.NPC)
                return null;
            var currentNodeActorGuid = currentNode.ActorGuid;
            var actor = Data.ActorData[currentNodeActorGuid];
            return actor;
        }

        public List<ConversationLine> GetCurrentLines()
        {
            var currentNode = DlogObject.NodeDictionary[currentNodeGuid];
            return currentNode.Lines;
        }

        public string ProgressNpc()
        {
            var lines = GetCurrentLines();
            for (var i = 0; i < lines.Count - 1; i++)
            {
                var line = lines[i];
                var currentCheck = ExecuteChecks(line, i);


                if (currentCheck)
                {
                    Progress(line);
                    ExecuteTriggers(line, i);
                    return line.Message;
                }
            }

            var lastLine = lines[lines.Count - 1];
            Progress(lastLine);
            ExecuteTriggers(lastLine, lines.Count - 1);
            return lastLine.Message;
        }

        public string ProgressSelf(int lineIndex)
        {
            var lines = GetCurrentLines();
            Progress(lines[lineIndex]);
            ExecuteTriggers(lines[lineIndex], lineIndex);
            return lines[lineIndex].Message;
        }

        private bool ExecuteChecks(ConversationLine line, int lineIndex)
        {
            bool currentCheck = true;
            foreach (CheckTree tree in line.CheckTrees)
            {
                currentCheck = EvaluateCheckTree(tree, lineIndex) && currentCheck;
            }

            return currentCheck;
        }

        private void ExecuteTriggers(ConversationLine line, int lineIndex)
        {
            foreach (var triggerGuid in line.Triggers)
            {
                Data.TriggerData[triggerGuid].Invoke(currentNodeGuid, lineIndex);
            }
        }

        private void Progress(ConversationLine line)
        {
            if (IsNullOrEmpty(line.Next))
            {
                conversationDone = true;
                currentNodeGuid = null;
                return;
            }

            currentNodeGuid = line.Next;
        }

        private bool EvaluateCheckTree(CheckTree tree, int lineIndex)
        {
            switch (tree.NodeKind)
            {
            case CheckTree.Kind.Property when IsNullOrEmpty(tree.PropertyGuid):
            case CheckTree.Kind.Property when !Data.CheckData.ContainsKey(tree.PropertyGuid):
                return false;
            case CheckTree.Kind.Property:
                return Data.CheckData[tree.PropertyGuid].Invoke(currentNodeGuid, lineIndex);
            case CheckTree.Kind.Unary:
            {
                bool check = EvaluateCheckTree(tree.SubtreeA, lineIndex);
                return EvaluateUnaryOperation(tree.BooleanOperation, check);
            }
            case CheckTree.Kind.Binary:
            {
                bool checkA = EvaluateCheckTree(tree.SubtreeA, lineIndex);
                bool checkB = EvaluateCheckTree(tree.SubtreeB, lineIndex);
                return EvaluateBinaryOperation(tree.BooleanOperation, checkA, checkB);
            }
            default:
                // Unreachable
                throw new Exception("Unreachable");
            }
        }

        private static bool EvaluateUnaryOperation(BooleanOperation operation, bool value)
        {
            switch (operation)
            {
            case BooleanOperation.NOT:
                return !value;
            default:
                throw new Exception("Unreachable");
            }
        }

        private static bool EvaluateBinaryOperation(BooleanOperation operation, bool valueA, bool valueB)
        {
            switch (operation)
            {
            case BooleanOperation.AND:
                return valueA && valueB;
            case BooleanOperation.OR:
                return valueA || valueB;
            case BooleanOperation.XOR:
                return valueA ^ valueB;
            case BooleanOperation.NAND:
                return !(valueA && valueB);
            case BooleanOperation.NOR:
                return !(valueA || valueB);
            case BooleanOperation.XNOR:
                return !(valueA ^ valueB);
            default:
                throw new Exception("Unreachable");
            }
        }
    }
}