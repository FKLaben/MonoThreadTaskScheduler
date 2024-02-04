/*
     This file is part of Foobar.

    Foobar is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

    Foobar is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
*/

namespace MonoThreadTaskSchedulerProject
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Text;

    internal class ThreadSafeObservableQueue<T> : ConcurrentQueue<T>
    {
        private readonly object _locker = new object();

        public event EventHandler NewItem;

        public new void Enqueue(T item)
        {
            lock(_locker)
            {
                base.Enqueue(item);
                NewItem?.Invoke(this, new NotifyCollectionChangedEventArgs());
            }
        }

        public new bool TryDequeue(out T result)
        {
            lock (_locker)
            {
                return base.TryDequeue(out result);
            }
        }
    }
}
