using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public interface IObjectAccessor<out T>
    {
        [CanBeNull]
        T Value { get; }
    }

    public class ObjectAccessor<T> : IObjectAccessor<T>
    {
        public T Value { get; set; }

        public ObjectAccessor()
        {

        }

        public ObjectAccessor([CanBeNull] T obj)
        {
            Value = obj;
        }
    }
}
