using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentObjects {

    [Serializable]
    public class Study {
        [SerializeField] public string studyName;
        [SerializeField] public bool flip;
        [SerializeField] public List<Case> cases;
    }

    [Serializable]
    public class Case {
        [SerializeField] public string caseName;
        [SerializeField] public List<TextAsset> maps;
        [SerializeField] public string scene;
        [NonSerialized] public int mapIndex;

        public Case() {
            RandomizeCurrentMap();
        }

        public TextAsset GetCurrentMap() {
            return maps[mapIndex];
        }

        public void RandomizeCurrentMap() {
            if (maps != null) {
                mapIndex = UnityEngine.Random.Range(0, maps.Count);
            } else {
                mapIndex = 0;
            }
        }
    }

    [Serializable]
    public class StudyCompletionTracker {
        public int studyCompletion;
        public int[] casesCompletion;

        public StudyCompletionTracker(int studyCompletion, int[] casesCompletion) {
            this.studyCompletion = studyCompletion;
            this.casesCompletion = casesCompletion;

        }
    }

    public struct Coord {
        public float x;
        public float z;
    }

}
