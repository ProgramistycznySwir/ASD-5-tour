// Hmm, kod zdefiniowany tutaj nie wykorzystuje żadnego kodu z .NET

namespace ASD___5
{
    class Street : IItemWithPriority<Street>
    {
        public readonly int index;
        // Do wypisywania itp
        public int Number => index + 1;
        public readonly int end0;
        public readonly int end1;
        public readonly int lenght;
        public readonly int sensation;

        public int NetSensation  => sensation - lenght;
        public int HalfSensation => sensation - lenght / 2;
        /// <summary>
        /// Priority of this item in queue is it's sensation minus half of lenght.
        /// </summary>
        // Na razie to nie musi być float dla szybszego działania kolejki.
        // Kolejne dziwna składnia, googluj "C# switch expression", kolejna perełka.
        public int Priority(PriorityMode mode) =>
            mode switch
            {
                PriorityMode.Full => NetSensation,
                PriorityMode.Half => HalfSensation,
                PriorityMode.InverseFull => -NetSensation,
            };


        public int OtherEnd(int end) => ((end == end0) ? end1 : end0);

        public Street(Street_FreeAccess streetToSeal)
        {
            this.index = streetToSeal.index;
            this.end0 = streetToSeal.end0;
            this.end1 = streetToSeal.end1;
            this.lenght = streetToSeal.lenght;
            this.sensation = streetToSeal.sensation;
        }
        public Street(int index, int end0, int end1, int lenght, int sensation)
        {
            this.index = index;
            this.end0 = end0;
            this.end1 = end1;
            this.lenght = lenght;
            this.sensation = sensation;
        }

        public override string ToString()
            => $"{index}. ({end0} {end1})";
    }

    struct Street_FreeAccess
    {
        public int index;
        public int end0;
        public int end1;
        public int lenght;
        public int sensation;

        public int NetSensation => sensation - lenght;

        public Street_FreeAccess(int index, int end0, int end1, int lenght, int sensation)
        {
            this.index = index;
            this.end0 = end0;
            this.end1 = end1;
            this.lenght = lenght;
            this.sensation = sensation;
        }
    }

    /// <typeparam name="T">Item with int indicating priority.</typeparam>
    class PriorityQueue<T> where T : IItemWithPriority<T>
    {
        // Można by zamiast tego użyć bst z poprzedniego zadania, ale byłby to overkill, ponieważ potrzebujemy struktury która
        //  jedynie utrzymuje pierwszy element jako element o najniższym priorytecie.

        public readonly int heapCapacity;
        T[] heap;
        int count;
        readonly PriorityMode priorityMode;

        public bool IsEmpty => (count == 0);

        // Ściągawka do nawigowania po stercie:
        //  parent = (n-1)/2
        //  left = 2*n+1
        //  right = 2*n+2

        public PriorityQueue(int heapCapacity, PriorityMode priorityMode)
        {
            this.heapCapacity = heapCapacity;
            heap = new T[heapCapacity];
            this.priorityMode = priorityMode;
        }

        public void Add(T addedItem)
        {
            int addedItemIndex = count++;
            int parent = (addedItemIndex - 1) / 2;
            // Wstawienie nowego elementu jako ostatni element stosu.
            heap[addedItemIndex] = addedItem;
            // Ustawienie elementu w odpowiednim miejscu stosu.
            while (heap[addedItemIndex].Priority(priorityMode) > heap[parent].Priority(priorityMode))
            {
                Swap(addedItemIndex, parent);
                addedItemIndex = parent;
                parent = (addedItemIndex - 1) / 2;
            }
        }
        public T Peek => heap[0];
        public T Poll()
        {
            if (IsEmpty)
                throw new System.NullReferenceException();
            T mostPriorityItem = heap[0];
            heap[0] = heap[--count];

            int currentItemIndex = 0;
            int left, right, swapIndex;
            while (true)
            {
                left = 2 * currentItemIndex + 1;
                right = 2 * currentItemIndex + 2;

                if (left < count)
                {
                    swapIndex = left;
                    if (right < count && heap[left].Priority(priorityMode) < heap[right].Priority(priorityMode))
                        swapIndex = right;

                    if (heap[currentItemIndex].Priority(priorityMode) < heap[swapIndex].Priority(priorityMode))
                        Swap(currentItemIndex, currentItemIndex = swapIndex);
                    break;
                }
                break;
            }

            return mostPriorityItem;
        }


        /// <summary>
        /// Takes two indexes to swap in heap;
        /// </summary>
        void Swap(int a, int b)
        {
            T temp = heap[a];
            heap[a] = heap[b];
            heap[b] = temp;
        }
    }

    // Łatwiejsze niż tworzenie kilku priorytetów. Generalnie iirc (mogę się mylić) algorytm korzysta z jednego z 3
    //  typów priorytetów, ale zostawiłem wszystkie 3 w celu eksperymentowania.
    public enum PriorityMode { Full, Half, InverseFull}

    public interface IItemWithPriority<T>
    {
        public int Priority(PriorityMode priorityMode);
    }
}