using System;
using System.Threading.Tasks;

namespace DnDInitiativeTracker.Extensions
{
    public static class TaskAsyncExtensions
    {
        public static Task<T> MakeAsync<T>(Action<Action<T>> syncAction)
        {
            var taskCompletion = new TaskCompletionSource<T>();
            
            syncAction.Invoke(taskCompletion.SetResult);
            
            return taskCompletion.Task;
        }
    }
}