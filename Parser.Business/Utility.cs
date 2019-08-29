using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parser.Business
{
    public class Utility
    {
        //constructor
        public Utility()
        {
            groupedList = new Dictionary<string, int>();
            isInProgress = false;
        }

        //fields
        private int currentState;
        private int currentRow;
        private int totalRow;
        public bool isCancelled;
        public bool isInProgress;

        private Dictionary<string, int> groupedList;

        public delegate void StateInvoker(int currentState);

        public StateInvoker GetCurrentState;

        private Dictionary<string, int> ParseLine(string inputLine, bool caseInsensitive)
        {
            if (isCancelled)
            {
                isInProgress = false;
                throw new OperationCanceledException();
            }

            string[] words = inputLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string key = words[i];

                if (caseInsensitive)
                    key = words[i].ToLower();

                if (groupedList.ContainsKey(key))
                    groupedList[key] += 1;
                else
                    groupedList[key] = 1;
            }

            currentRow += 1;
            currentState = currentRow * 100 / totalRow;
            GetCurrentState(currentState);

            return groupedList;
        }

        public Dictionary<string, int> Parse(string path, bool caseInsensitive = false)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            ResetParseState();
            isInProgress = true;
            totalRow = File.ReadLines(path).Count();
            var result = File.ReadLines(path).Select(line => ParseLine(line, caseInsensitive)).Last();
            isInProgress = false;
            return result;
        }

        private void ResetParseState()
        {
            groupedList.Clear();
            currentRow = 0;
            totalRow = 0;
            currentState = 0;
        }
    }
}
