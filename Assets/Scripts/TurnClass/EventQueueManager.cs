using System;
using System.Collections.Generic;

namespace TurnClass
{
    public class EventQueueManager
    {
        private Queue<Action<Action>> eventQueue = new Queue<Action<Action>>();
        private bool isRunning = false;

        public void AddEvent(Action<Action> eventAction)
        {
            eventQueue.Enqueue(eventAction);
        }

        public void Start()
        {
            if (isRunning) return;
            isRunning = true;
            ExecuteNext();
        }

        private void ExecuteNext()
        {
            if (eventQueue.Count > 0)
            {
                var nextEvent = eventQueue.Dequeue();
                nextEvent(ExecuteNext);
            }
            else
            {
                isRunning = false;
            }
        }

        public void Clear()
        {
            eventQueue.Clear();
            isRunning = false;
        }
    }

}