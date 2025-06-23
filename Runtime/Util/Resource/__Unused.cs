#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class CanRunAfterConstruction
    {
        private readonly List<Action> _pendingActions = new List<Action>();
        private readonly object _lock = new object();
        private volatile bool _constructionComplete = false;

        public CanRunAfterConstruction()
        {
            // TODO: how to do it without launching a task?

            // Schedule MarkConstructionComplete to run after constructor completes
            // Use a simple task-based approach that works reliably
            System.Threading.Tasks.Task.Run(async () =>
            {
                // Small delay to ensure all constructor chains have completed
                await System.Threading.Tasks.Task.Delay(1);
                MarkConstructionComplete();
            });
        }

        public void RunAfterConstruction(Action action)
        {
            if (action == null) return;

            lock (_lock)
            {
                if (_constructionComplete)
                {
                    // Construction is already complete, execute immediately outside the lock
                    // to avoid potential deadlocks
                }
                else
                {
                    // Construction is still in progress, queue the action
                    _pendingActions.Add(action);
                    return; // Exit early to avoid executing immediately
                }
            }

            // Execute immediately if construction is complete
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing post-construction action: {ex}");
            }
        }

        public void MarkConstructionComplete()
        {
            List<Action> actionsToExecute;

            lock (_lock)
            {
                if (_constructionComplete) return; // Already marked complete

                _constructionComplete = true;

                // Copy actions to execute outside the lock
                actionsToExecute = new List<Action>(_pendingActions);
                _pendingActions.Clear();
            }

            // Execute all pending actions outside the lock to avoid deadlocks
            foreach (var action in actionsToExecute)
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing post-construction action: {ex}");
                }
            }
        }
    }
}