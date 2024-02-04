/*
     This file is part of Foobar.

    Foobar is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

    Foobar is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
*/

namespace MonoThreadTaskSchedulerProject
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;

    public class MonoThreadTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly ThreadSafeObservableQueue<Task> _taskQueue = new ThreadSafeObservableQueue<Task>();
        private readonly ManualResetEventSlim _waitForNewTask = new ManualResetEventSlim(false);
        private bool disposedValue;

        public MonoThreadTaskScheduler()
        {
            _taskQueue.NewItem += OnNewTaskAvailable;
            Thread internalThread = new Thread(Worker)
            {
                Name = nameof(MonoThreadTaskScheduler),
                IsBackground = true
            };
            internalThread.Start();
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return new List<Task>(_taskQueue);
        }

        protected override void QueueTask(Task task)
        {
            _taskQueue.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }


        private void Worker(object obj)
        {
            for(;;)
            {
                if (!_taskQueue.Any())
                {
                    _waitForNewTask.Wait();
                    _waitForNewTask.Reset();
                }

                if (_taskQueue.TryDequeue(out Task item))
                {
                    try
                    {
                        TryExecuteTask(item);
                    }
                    finally
                    {
                    }
                }
            }
        }

        private void OnNewTaskAvailable(object sender, EventArgs e)
        {
            _waitForNewTask.Set();
        }

        #region IDispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _waitForNewTask.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDispose
    }
}
