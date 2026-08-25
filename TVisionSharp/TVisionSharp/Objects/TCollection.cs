using System;

namespace TVision
{
    public class TCollection
    {
        protected object[] Items;
        public int Count;
        public int Limit;
        public int Delta;
        public bool ShouldDelete;

        public TCollection(int aLimit, int aDelta)
        {
            Delta = aDelta;
            Items = new object[aLimit];
            Limit = aLimit;
            Count = 0;
            ShouldDelete = true;
        }

        protected TCollection() { Items = Array.Empty<object>(); }

        public virtual void ShutDown()
        {
            if (ShouldDelete) FreeAll();
        }

        public virtual int Insert(object item)
        {
            if (Count >= Limit) SetLimit(Limit + Delta);
            Items[Count] = item;
            return Count++;
        }

        public virtual void AtInsert(int index, object item)
        {
            if (Count >= Limit) SetLimit(Limit + Delta);
            for (int i = Count; i > index; i--)
                Items[i] = Items[i - 1];
            Items[index] = item;
            Count++;
        }

        public object At(int index) => Items[index];

        public virtual void AtPut(int index, object item) => Items[index] = item;

        public virtual int IndexOf(object item)
        {
            for (int i = 0; i < Count; i++)
                if (Items[i] == item) return i;
            return -1;
        }

        public void AtFree(int index)
        {
            var item = Items[index];
            Items[index] = null;
            FreeItem(item);
            for (int i = index; i < Count - 1; i++)
                Items[i] = Items[i + 1];
            Count--;
        }

        public void AtRemove(int index)
        {
            for (int i = index; i < Count - 1; i++)
                Items[i] = Items[i + 1];
            Count--;
        }

        public void Remove(object item)
        {
            int idx = IndexOf(item);
            if (idx >= 0) AtRemove(idx);
        }

        public void Free(object item)
        {
            int idx = IndexOf(item);
            if (idx >= 0) AtFree(idx);
        }

        public void FreeAll()
        {
            for (int i = 0; i < Count; i++)
            {
                FreeItem(Items[i]);
                Items[i] = null;
            }
            Count = 0;
        }

        public void RemoveAll()
        {
            for (int i = 0; i < Count; i++)
                Items[i] = null;
            Count = 0;
        }

        public void Pack()
        {
            int j = 0;
            for (int i = 0; i < Count; i++)
            {
                if (Items[i] != null)
                    Items[j++] = Items[i];
            }
            Count = j;
        }

        public virtual void SetLimit(int aLimit)
        {
            if (aLimit < Count) aLimit = Count;
            if (aLimit != Limit)
            {
                var newArr = new object[aLimit];
                for (int i = 0; i < Count; i++)
                    newArr[i] = Items[i];
                Items = newArr;
                Limit = aLimit;
            }
        }

        public object FirstThatGeneric(Predicate<object> test)
        {
            for (int i = 0; i < Count; i++)
                if (test(Items[i])) return Items[i];
            return default;
        }

        public object FirstThat(Func<object, bool> test)
        {
            for (int i = 0; i < Count; i++)
                if (test(Items[i])) return Items[i];
            return null;
        }

        public object LastThat(Func<object, bool> test)
        {
            for (int i = Count - 1; i >= 0; i--)
                if (test(Items[i])) return Items[i];
            return null;
        }

        public void ForEach(Action<object> action)
        {
            for (int i = 0; i < Count; i++)
                action(Items[i]);
        }

        public int GetCount() => Count;

        protected virtual void FreeItem(object item) { }

        public T At<T>(int index) => (T)Items[index];
    }

    public class TSortedCollection : TCollection
    {
        public bool Duplicates;

        public TSortedCollection(int aLimit, int aDelta) : base(aLimit, aDelta)
        {
            Duplicates = false;
        }

        protected TSortedCollection() { Duplicates = false; }

        protected virtual int Compare(object key1, object key2)
        {
            if (key1 is IComparable c) return c.CompareTo(key2);
            return string.Compare(key1?.ToString(), key2?.ToString(), StringComparison.Ordinal);
        }

        public virtual bool Search(object key, out int index)
        {
            int lo = 0, hi = Count - 1;
            index = 0;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                int cmp = Compare(key, Items[mid]);
                if (cmp == 0)
                {
                    index = mid;
                    return true;
                }
                if (cmp < 0) hi = mid - 1;
                else lo = mid + 1;
            }
            index = lo;
            return false;
        }

        public override int Insert(object item)
        {
            int idx = 0;
            if (!Duplicates && Search(KeyOf(item), out idx))
                return idx;
            AtInsert(idx, item);
            return idx;
        }

        public override int IndexOf(object item)
        {
            int idx;
            if (Search(KeyOf(item), out idx))
                return idx;
            return -1;
        }

        protected virtual object KeyOf(object item) => item;
    }

    public class TStringCollection : TSortedCollection
    {
        public TStringCollection(int aLimit, int aDelta) : base(aLimit, aDelta) { }

        protected override int Compare(object key1, object key2)
        {
            return string.Compare((string)key1, (string)key2, StringComparison.Ordinal);
        }

        protected override object KeyOf(object item) => item;
    }
}
