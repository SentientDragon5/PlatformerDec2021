using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character.Interactions.Dialouge
{
    [CreateAssetMenu(fileName = "Dialouge", menuName = "Dialouge")]
    public class DialougeSO : ScriptableObject
    {
        public string characterName = "Name";
        public List<DialougeChunk> dialouge = new List<DialougeChunk>();
    }

    [System.Serializable]
    public class DialougeChunk
    {
        public int index = 0;
        public string text = "Speaking now!";
        public List<Choice> choices;

        public DialougeChunk(int i, string text, List<Choice> choices)
        {
            index = i;
            this.text = text;
            this.choices = choices;
        }
    }

    [System.Serializable]
    public class Choice
    {
        public string text = "Choice 0";
        public int to = 0;
        public Choice(string text, int to)
        {
            this.text = text;
            this.to = to;
        }
    }
}