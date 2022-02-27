using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace Character.Interactions.Dialouge
{
    public class DialougeReader : MonoBehaviour
    {
        #region Singleton
        public static DialougeReader instance;
        void Awake()
        {
            if (instance == null) instance = this;
            //if (instance != this) Destroy(this);
        }
        #endregion

        public DialougeSO dialougeObject;

        public int currentIndex;    //The index on the SO's List
        public string outputText;   //The text being outputted
        public int choiceID;        //The choice number for which choice of the List of SO[index].Choices[choiceID]
        public float speed = 40f;   //The reading speed factor, 1 is unbearable, 40 is snappy
        public bool doneWithLine = true;   //is the reader done with the current line?

        [Header("References to UI")]
        [SerializeField] private GameObject DialougeUIGameObject;
        [SerializeField] private TextMeshProUGUI textUI;
        [SerializeField] private TextMeshProUGUI nameUI;
        [SerializeField] private GameObject textBoxButton;
        [SerializeField] private List<GameObject> Buttons = new List<GameObject>();

        public void Begin()
        {
            Reset();
            DialougeUIGameObject.SetActive(true);
            DisplayText();
            DisplayChoices(false);
        }
        public void Reset()
        {
            doneWithLine = true;//Allow the reader to continue
            currentIndex = 0;
        }
        public void Next()
        {
            DialougeUIGameObject.SetActive(true);//Ensure the Dialouge is actually being shown.

            string text = dialougeObject.dialouge[currentIndex].text;
            int numberOfChoices = dialougeObject.dialouge[currentIndex].choices.Count;
            List<Choice> choices = dialougeObject.dialouge[currentIndex].choices;

            if (doneWithLine)
            {
                //If you are done reading:
                //change the UI to the next screen
                if(numberOfChoices < 1)
                {
                    //If there are no choices then it is Done
                    End();
                }
                else if (numberOfChoices < 2)
                {
                    //If there is only one choice then take it.
                    currentIndex = choices[0].to;
                }
                else
                {
                    //If there are multiple choices
                    //Set the index to the next one in the chain
                    currentIndex = choices[choiceID].to;
                }

                DisplayText();
                DisplayChoices(false);
                //Display();
            }
            else
            {
                //If you are not done:
                //Skip the reading and complete the text but don't move on.
                StopCoroutine(Read(text, speed));
                DoneReading(text);
            }


        }
        void DoneReading(string text)
        {
            StopAllCoroutines();
            outputText = text;
            textUI.text = outputText;
            doneWithLine = true;
            DisplayChoices(true);
        }

        void Display()
        {
            string text = dialougeObject.dialouge[currentIndex].text;
            int numberOfChoices = dialougeObject.dialouge[currentIndex].choices.Count;
            List<Choice> choices = dialougeObject.dialouge[currentIndex].choices;
            //Next

            //Stop any coroutines
            StopAllCoroutines();
            //StartReading the next dialouge
            StartCoroutine(Read(text, speed));

            //Decide whether to display choices or just one box
            if (numberOfChoices < 2)
            {
                //If there are no choices or one choice then only the box is the choice
                textBoxButton.GetComponent<UnityEngine.UI.Button>().enabled = true; //Enable Big Button

                for (int i = 0; i < Buttons.Count; i++) //Set all the other buttons to hide
                {
                    Buttons[i].SetActive(false);
                }
            }
            else
            {
                //If there are multiple choices then display them.
                textBoxButton.GetComponent<UnityEngine.UI.Button>().enabled = true; //disable Big Button

                //Update the choices
                for (int i = 0; i < numberOfChoices; i++)
                {
                    //Show
                    Buttons[i].SetActive(i < choices.Count);
                    Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choices[i].text;
                }
            }

            
        }
        void DisplayText()
        {
            string text = dialougeObject.dialouge[currentIndex].text;

            //Stop any coroutines
            StopAllCoroutines();
            //StartReading the next dialouge
            StartCoroutine(Read(text, speed));
        }
        void DisplayChoices(bool show)
        {
            int numberOfChoices = dialougeObject.dialouge[currentIndex].choices.Count;
            List<Choice> choices = dialougeObject.dialouge[currentIndex].choices;
            //Next

            //Decide whether to display choices or just one box
            if (numberOfChoices < 2 && show)
            {
                //If there are no choices or one choice then only the box is the choice
                textBoxButton.GetComponent<UnityEngine.UI.Button>().enabled = true; //Enable Big Button

                for (int i = 0; i < Buttons.Count; i++) //Set all the other buttons to hide
                {
                    Buttons[i].SetActive(false);
                }
            }
            else
            {
                //If there are multiple choices then display them.
                textBoxButton.GetComponent<UnityEngine.UI.Button>().enabled = true; //disable Big Button

                //Update the choices
                for (int i = 0; i < numberOfChoices; i++)
                {
                    //Show
                    Buttons[i].SetActive(i < choices.Count && show);
                    Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choices[i].text;
                }
            }
        }

        public void End()
        {
            DialougeUIGameObject.SetActive(false);
        }

        public IEnumerator Read(string text, float speed)
        {
            doneWithLine = false;
            outputText = "";
            for (int c = 0; c < text.Length; c++)
            {
                string add = text.ToCharArray()[c].ToString();
                if (add == " ")//if it is a space then do the next letter too.
                {
                    c++;
                    add += text.ToCharArray()[c].ToString();
                }

                outputText += add;
                textUI.text = outputText;
                yield return new WaitForSecondsRealtime(speed);
            }
            DoneReading(text);
        }

        public void SetChoice(int choiceID)
        {
            this.choiceID = choiceID;
        }
    }
}