using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority>
{
    private List<KeyValuePair<TElement, TPriority>> elements = new List<KeyValuePair<TElement, TPriority>>();
    private Comparer<TPriority> comparer = Comparer<TPriority>.Default;

    public int Count => elements.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        elements.Add(new KeyValuePair<TElement, TPriority>(element, priority));
        elements.Sort((x, y) => comparer.Compare(x.Value, y.Value)); // Sort by priority
    }

    public TElement Dequeue()
    {
        if (elements.Count == 0)
        {
            throw new System.InvalidOperationException("The queue is empty.");
        }
        var element = elements[0].Key;
        elements.RemoveAt(0);
        return element;
    }
}
