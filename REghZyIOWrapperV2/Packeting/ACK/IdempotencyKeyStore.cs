using System;
using System.Collections.Generic;
using System.Text;

namespace REghZyIOWrapperV2.Packeting.ACK {
    /// <summary>
    /// A class for efficiently storing idempotency keys
    /// </summary>
    public class IdempotencyKeyStore {
        private readonly Node first;

        private uint highest;

        public IdempotencyKeyStore() {
            this.first = new Node();
            this.first.range = new Range(0);
        }

        public bool Put(uint key) {
            if (key > this.highest) {
                this.highest = key;
            }

            Node node = this.first;
            Node prev = node;
            Range range = node.range;
            while (true) {
                if (range.IsBetween(key)) {
                    return false;
                }
                else if (range.IsAbove(key)) {
                    uint keySub1 = key - 1;
                    uint keyAdd1 = key + 1;
                    if (node.range.max == keySub1) {
                        node.range.IncrMax();
                        if (node.next == null) {
                            return true;
                        }
                        else if (node.next.range.min == keyAdd1) {
                            node.range.SetMax(node.next.range.max);
                            node.next.Remove();
                            return true;
                        }
                    }
                    else {
                        prev = node;
                        node = node.next;
                        if (node == null) {
                            Node newNode = new Node();
                            newNode.range = new Range(key);
                            newNode.AddAfter(prev);
                            return true;
                        }
                        else {
                            range = node.range;
                        }
                    }
                }
                else if (range.IsBelow(key)) {
                    if (node.prev == prev) {
                        uint keySub1 = key - 1;
                        uint keyAdd1 = key + 1;
                        bool prevIncrement = false;
                        if (prev.range.max == keySub1) {
                            prev.range.IncrMax();
                            prevIncrement = true;
                        }
                        if (node.range.min == keyAdd1) {
                            if (prevIncrement) {
                                prev.range.SetMax(node.range.max);
                                node.Remove();
                            }

                            return true;
                        }
                        else if (prevIncrement) {
                            return true;
                        }
                        else {
                            Node newNode = new Node();
                            newNode.range = new Range(key);
                            newNode.InsertBetween(prev, node);
                            return true;
                        }
                    }
                    else {
                        prev = node;
                        node = node.prev;
                        if (node == null) {
                            throw new Exception("Huh...");
                        }

                        range = node.range;
                    }
                }
                else {
                    throw new Exception("What.....");
                }
            }
        }

        public bool HasKey(uint key) {
            if (key > this.highest) {
                return false;
            }
            else {
                Node node = this.first;
                while(node != null) {
                    if (node.range.IsBetween(key)) {
                        return true;
                    }
                    else if (node.range.IsBelow(key)) {
                        return false;
                    }
                    else {
                        // it's above, so just continue ignore it
                        node = node.next;
                    }
                }

                return false;
            }
        }

        // literally cant be bothered to add a remove RemoveKey function because
        // it would take ages to make haha

        public IEnumerable<uint> GetEnumerator() {
            Node node = this.first;
            while(node != null) {
                for(uint i = node.range.min, end = node.range.max + 1; i < end; i++) {
                    yield return i;
                }

                node = node.next;
            }
        }

        // [] --- [] --- []
        // [] --- []
        // []
        //        []
        //               []
        // [] --- US --- [] --- []
        // [] --- [] --- US --- []
        private class Node {
            public Node prev;
            public Node next;
            public Range range;

            public Node() {

            }

            /// <summary>
            /// Makes this node the given node's next node
            /// </summary>
            /// <param name="node">The new previous node</param>
            public void AddAfter(Node node) {
                node.next = this;
                this.prev = node;
            }

            /// <summary>
            /// Makes this node the given node's previous node
            /// </summary>
            /// <param name="node">The new next node</param>
            public void AddBefore(Node node) {
                node.prev = this;
                this.next = node;
            }

            public void InsertAfter(Node node) {
                if (node.next != null) {
                    this.next = node.next;
                    node.next.prev = this;
                }

                node.next = this;
                this.prev = node;
            }

            public void InsertBefore(Node node) {
                if (node.prev != null) {
                    this.prev = node.prev;
                    node.prev.next = this;
                }

                node.prev = this;
                this.next = node;
            }

            // Connects the prev and next together, removing this entirely
            public void Remove() {
                if (this.prev == null || this.next == null) {
                    if (this.prev == null) {
                        this.next.prev = null;
                        this.next = null;
                    }
                    else {
                        this.prev.next = null;
                        this.prev = null;
                    }
                }
                else {
                    this.prev.next = this.next;
                    this.next.prev = this.prev;
                    this.prev = null;
                    this.next = null;
                }
            }

            public void InsertBetween(Node a, Node b) {
                if (a != null) {
                    AddAfter(a);
                }
                if (b != null) {
                    AddBefore(b);
                }
            }

            public override string ToString() {
                return new StringBuilder().Append("[").Append(this.range.min).Append("-").Append(this.range.max).Append("]").ToString();
            }
        }

        private struct Range {
            public uint min;
            public uint max;

            public Range(uint value) {
                this.min = value;
                this.max = value;
            }

            public Range(uint min, uint max) {
                this.min = min;
                this.max = max;
            }

            public void IncrMax() {
                this.max++;
            }

            public void DecrMin() {
                this.min--;
            }

            public void SetMin(uint min) {
                this.min = min;
            }

            public void SetMax(uint max) {
                this.max = max;
            }

            public bool IsBetween(uint value) {
                return value >= this.min && value <= this.max;
            }

            public bool IsNotBetween(uint value) {
                return value < this.min || value > this.max;
            }

            public bool IsAbove(uint value) {
                return value > this.max;
            }

            public bool IsBelow(uint value) {
                return value < this.min;
            }
        }
    }
}
