/*
     This file is part of Foobar.

    Foobar is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

    Foobar is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
*/

namespace MonoThreadTaskSchedulerProject.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MonoThreadTaskSchedulerProject;
    using NUnit.Framework;
    using System.Linq;
    using System.Collections.Concurrent;

    [TestFixture]
    public class MonoThreadTaskSchedulerTests
    {
        [TestCase(10)]
        public void CheckAllTasksRunInSameThread(int taskCount)
        {
            List<Func<int>> funcs = new List<Func<int>>();
            for (int i = 0; i < taskCount; ++i)
            {
                funcs.Add(() => Environment.CurrentManagedThreadId);
            }

            MonoThreadTaskScheduler monoThreadTaskScheduler = new MonoThreadTaskScheduler();
            List<Task<int>> tasks = new List<Task<int>>();
            foreach(Func<int> func in funcs)
            {
                tasks.Add(Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, monoThreadTaskScheduler));
            }

            List<int> results = new List<int>();
            foreach (Task<int> task in tasks)
            {
                results.Add(task.Result);
            }

            Assert.IsTrue(results.Distinct().Count() == 1, "At least one task has been run on a different thread!");
        }

        [TestCase(4)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(100000)]
        public void CheckMonoThreadTaskSchedulerCanBeCalledByMultipleThreads(int taskCount)
        {
            List<Func<int>> funcs = new List<Func<int>>();
            for (int i = 0; i < taskCount; ++i)
            {
                funcs.Add(() => Environment.CurrentManagedThreadId);
            }

            MonoThreadTaskScheduler monoThreadTaskScheduler = new MonoThreadTaskScheduler();
            ConcurrentQueue<Task<int>> tasks = new ConcurrentQueue<Task<int>>();
            Parallel.ForEach(funcs, func =>
            {
                // Parallel.ForEach is equivalent to for() { Task.Run(...
                tasks.Enqueue(Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, monoThreadTaskScheduler));
            });

            List<int> results = new List<int>();
            foreach (Task<int> task in tasks)
            {
                results.Add(task.GetAwaiter().GetResult());
            }

            Assert.IsTrue(results.Distinct().Count() == 1, "At least one task has been run on a different thread!");
        }

        [TestCase(4)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void CheckTasksOrderIsRetained(int taskCount)
        {
            List<Func<int>> funcs = new List<Func<int>>();
            for (int i = 0; i < taskCount; ++i)
            {
                int tmpVar = i;
                funcs.Add(() => tmpVar);
            }

            MonoThreadTaskScheduler monoThreadTaskScheduler = new MonoThreadTaskScheduler();
            List<Task<int>> tasks = new List<Task<int>>();
            foreach (Func<int> func in funcs)
            {
                tasks.Add(Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, monoThreadTaskScheduler));
            }

            List<int> results = new List<int>();
            foreach (Task<int> task in tasks)
            {
                results.Add(task.Result);
            }

            for (int i = 0; i < taskCount; ++i)
            {
                Assert.AreEqual(i, results.ElementAt(i), "Order is not retained!");
            }
        }
    }
}
