using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a collection of key/value pairs that are accessible by the key or index.
/// Intended to be used as a replacement for the weakly typed 
/// <see cref="System.Collections.Specialized.OrderedDictionary"/> collection. 
/// </summary>
/// <typeparam name="TKey">Type of the keys.</typeparam>
/// <typeparam name="TValue">Type of the values.</typeparam>
/// <remarks>
/// <para>
/// This implementation is compatible with .NET 2.0 and higher. 
/// </para>
/// <para>
/// Provides a workaround to a missing generic implementation in
/// the .NET Framework as of v4.5. 
/// </para>
/// <para>
/// Internally this data structure is made up of one Dictionary(Of K, V) instance for random 
/// access operations and one List(KeyValuePair(Of K, Of V) instance for ordered operations. 
/// This implementation delegates its calls to the internal dictionary for unordered 
/// operations and to the list for ordered operations. 
/// The two internal data structures are kept in sync with shared KVPs.
/// </para>
/// </remarks>
/// 
public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    /// The internal List for ordered access to KVPs. 
    /// </summary>
    internal List<KeyValuePair<TKey, TValue>> internalList;

    /// <summary>
    /// The internal dictionary for random access to KVPs. 
    /// </summary>
    internal Dictionary<TKey, TValue> internalDictionary;

    #region Utility members

    /// <summary>
    /// Utility method to retrieve the first matching KVP by key name. 
    /// </summary>
    /// <param name="key">The sought key.</param>
    /// <param name="valueFound">The first KVP found.</param>
    /// <returns>true if the key was found otherwise false </returns>
    /// 
    internal bool TryGetPairFromListByKey(TKey key, out KeyValuePair<TKey, TValue> valueFound, out int indexFound)
    {
        int indexCurrent = 0;
        bool foundMatch = false;

        Predicate<KeyValuePair<TKey, TValue>> matcher = delegate (KeyValuePair<TKey, TValue> kvp)
        {
            if (ReferenceEquals(null, key) && ReferenceEquals(null, kvp.Key) || !ReferenceEquals(null, key) && key.Equals(kvp.Key))
            {
                return foundMatch = true;
            }
            indexCurrent++;
            return false;
        };

        valueFound = this.internalList.Find(matcher);
        indexFound = indexCurrent;
        return foundMatch;
    }

    /// <summary>
    /// Returns the ordered set of KVPs.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<TKey, TValue>> GetOrderedPairs()
    {
        foreach (KeyValuePair<TKey, TValue> pair in this.internalList)
            yield return pair;
    }

    /// <summary>
    /// Returns the ordered set of keys. 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TKey> GetOrderedKeys()
    {
        foreach (KeyValuePair<TKey, TValue> item in this.internalList)
            yield return item.Key;
    }

    /// <summary>
    /// Returns the ordered set of values. 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TValue> GetOrderedValues()
    {
        foreach (KeyValuePair<TKey, TValue> item in this.internalList)
            yield return item.Value;
    }

    /// <summary>
    /// Gets the count of the internal list. 
    /// </summary>
    internal int ListCount { get { return this.internalList.Count; } }

    /// <summary>
    /// Gets the count of the internal dictionary. 
    /// </summary>
    internal int DictionaryCount { get { return this.internalDictionary.Count; } }

    #endregion

    #region Support existing dictionary and list initialization options

    public OrderedDictionary()
    {
        this.internalDictionary = new Dictionary<TKey, TValue>();
        this.internalList = new List<KeyValuePair<TKey, TValue>>();
    }

    public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
    {
        this.internalDictionary = new Dictionary<TKey, TValue>(dictionary);
        this.internalList = new List<KeyValuePair<TKey, TValue>>(dictionary);
    }

    public OrderedDictionary(IEqualityComparer<TKey> comparer)
    {
        this.internalDictionary = new Dictionary<TKey, TValue>(comparer);
        this.internalList = new List<KeyValuePair<TKey, TValue>>();
    }

    public OrderedDictionary(Int32 capacity)
    {
        this.internalDictionary = new Dictionary<TKey, TValue>(capacity);
        this.internalList = new List<KeyValuePair<TKey, TValue>>(capacity);
    }

    public OrderedDictionary(IDictionary<TKey, TValue> values, IEqualityComparer<TKey> comparer)
    {
        this.internalDictionary = new Dictionary<TKey, TValue>(values, comparer);
        this.internalList = new List<KeyValuePair<TKey, TValue>>(values);
    }

    public OrderedDictionary(Int32 capacity, IEqualityComparer<TKey> comparer)
    {
        this.internalDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        this.internalList = new List<KeyValuePair<TKey, TValue>>(capacity);
    }

    public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        this.internalDictionary = new Dictionary<TKey, TValue>();
        foreach (KeyValuePair<TKey, TValue> pair in items)
            ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Add(pair);
        this.internalList = new List<KeyValuePair<TKey, TValue>>(items);
    }

    #endregion

    #region The plain old Dictionary and List member implementations

    /// <summary>
    /// Appends an item to the end of the collection. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(TKey key, TValue value)
    {
        KeyValuePair<TKey, TValue> newItem = new KeyValuePair<TKey, TValue>(key, value);

        ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Add(newItem);
        this.internalList.Add(newItem);
    }

    /// <summary>
    /// Tells if the collection contains the specified key. 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(TKey key)
    {
        return this.internalDictionary.ContainsKey(key);
    }

    /// <summary>
    /// Returns the unordered set of keys. 
    /// To get an ordered set of keys
    /// use the <see cref="GetOrderedKeys()"/> method instead.
    /// </summary>
    public ICollection<TKey> Keys
    {
        get
        {
            /* I couldn't find an efficient way of returning these from the 
             * internal list for an ordered set of keys without creating a
             * new interim collection. Therefore I created the GetOrderedKeys(..) method
             * to do that. 
             */
            return this.internalDictionary.Keys;
        }
    }

    /// <summary>
    /// Removes the item with the specified key from the collection. 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(TKey key)
    {

        if (!this.internalDictionary.Remove(key))
            return false; // doesn't exist 

        int index = -1;
        KeyValuePair<TKey, TValue> pair;
        if (!this.TryGetPairFromListByKey(key, out pair, out index))
            throw new Exception(string.Format("failed to find {0} item in internal list after it was removed from associated dictionary", pair), null);

        this.internalList.RemoveAt(index);

        return true;
    }

    /// <summary>
    /// Tries to retrieve the value of the given key. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        return this.internalDictionary.TryGetValue(key, out value);
    }

    /// <summary>
    /// Returns the unordered set of values. 
    /// To get an ordered set of values
    /// use the <see cref="GetOrderedValues()"/> method instead.
    /// </summary>
    public ICollection<TValue> Values
    {
        get { return this.internalDictionary.Values; }
    }

    /// <summary>
    /// Gets or sets the value associated with the given key. 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TValue this[TKey key]
    {
        get
        {
            return this.internalDictionary[key];
        }
        set
        {
            TValue foundValue;
            if (this.internalDictionary.TryGetValue(key, out foundValue))
            {
                KeyValuePair<TKey, TValue> soughtListItem = new KeyValuePair<TKey, TValue>(key, foundValue);

                int index;
                if ((index = this.internalList.FindIndex(0, kvp => kvp.Equals(soughtListItem))) < 0)
                    throw new Exception("out of sync");

                this.internalDictionary[key] = value;
                this.internalList[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
            else
            {
                KeyValuePair<TKey, TValue> newItem = new KeyValuePair<TKey, TValue>(key, value);
                this.Add(newItem);
            }
        }
    }

    /// <summary>
    /// Appends an item to the collection. 
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Add(item);
        this.internalList.Add(item);
    }

    /// <summary>
    /// Clears all items from the collection. It will have a zero count. 
    /// </summary>
    public void Clear()
    {
        this.internalDictionary.Clear();
        this.internalList.Clear();
    }

    /// <summary>
    /// Tells if the collection contains the given item. 
    /// </summary>
    /// <param name="pair"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<TKey, TValue> pair)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Contains(pair);
    }

    /// <summary>
    /// Copies this collection to the given array. 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Tells how many items are in the collection. 
    /// </summary>
    public int Count
    {
        get { return this.internalDictionary.Count; }
    }

    /// <summary>
    /// Tells if the collection is read only. 
    /// </summary>
    public bool IsReadOnly
    {
        get { return ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).IsReadOnly; }
    }

    /// <summary>
    /// Removes the given item from the collection. 
    /// </summary>
    /// <param name="pair"></param>
    /// <returns></returns>
    public bool Remove(KeyValuePair<TKey, TValue> pair)
    {

        if (!((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Remove(pair))
            return false; //doesn't exist 

        if (!this.internalList.Remove(pair))
            throw new Exception(string.Format("Item {0} removed from internal dictionary but not from internal list", pair));

        return true;
    }

    /// <summary>
    /// Returns an enumerator instance over the ordered set of items in this collection. 
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return this.internalList.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator instance over the ordered set of items in this collection. 
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.internalList.GetEnumerator();
    }

    /// <summary>
    /// Returns the index position of the given item in this collection. 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int IndexOf(KeyValuePair<TKey, TValue> item)
    {
        return this.internalList.IndexOf(item);
    }

    /// <summary>
    /// Inserts the given item at the specified position in the collection. 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Add(item);
        this.internalList.Insert(index, item);
    }

    /// <summary>
    /// Removes the item at the specified index in the collection. 
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
        KeyValuePair<TKey, TValue> item = this.internalList[index];
        this.internalList.RemoveAt(index);
        if (!((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Remove(item))
            throw new Exception(string.Format("Removed item {1} at index position {0} of internal list but failed to remove it from the internal dictionary", index, item));
    }

    /// <summary>
    /// Gets or sets the item at the given index position in the collection. 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public KeyValuePair<TKey, TValue> this[int index]
    {
        get
        {
            return this.internalList[index];
        }
        set
        {
            KeyValuePair<TKey, TValue> oldItem = this.internalList[index];
            this.internalList.Insert(index, value);
            this.internalList.RemoveAt(index + 1);

            if (!this.internalDictionary.Remove(oldItem.Key))
                throw new Exception(string.Format("Replaced item {1} into index position {0} of internal list but failed to replace it in the internal dictionary", index, value));
            ((ICollection<KeyValuePair<TKey, TValue>>)this.internalDictionary).Add(value);
        }
    }

    #endregion
}
