using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueGraph.Runtime {
    [Serializable]
    public class DlogObjectData {
        public ActorDataDictionary ActorData;
        public CheckEventDictionary CheckData;
        public TriggerEventDictionary TriggerData;

        public DlogObjectData() {
            
            ActorData = new ActorDataDictionary();
            CheckData = new CheckEventDictionary();
            TriggerData = new TriggerEventDictionary();
        }

        public void AddActorData(string guid, ActorData data) {
            ActorData[guid] = data;
        }

        public void AddCheckEvent(string guid, CheckEvent evt) {
            CheckData[guid] = evt;
            evt.dynamic = true;
        }

        public void AddTriggerEvent(string guid, TriggerEvent evt) {
            TriggerData[guid] = evt;
            evt.dynamic = true; 
        }
    }
}