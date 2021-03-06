﻿using KScript.Callable;
using KScript.KAttribute;
using KScript.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("List", MapModifier.Static, MapType.CommonClass)]
    public class KList : KBuiltIn, Indexable, IEnumerable<object>
    {
        private List<object> elements;

        [MemberMap("test", MapModifier.Static, MapType.Method)]
        public static string Test(int a)
        {
            return "Hello, this is a test for static natFunc. Your arg: " + a;
        }

        [MemberMap("count", MapModifier.Instance, MapType.Data)]
        public int Count
        {
            get { return elements.Count; }
        }

        public object this[object index]
        {
            get
            {
                return IndexParser.GetElementAt(index, Count, (i) => elements[i], 
                                elements, (ienum) => new KList(ienum.ToArray()));
            }
            set
            {
                var res = IndexParser.ParseIndex(index, Count);
                //[x:x]or[x]
                if (res.Item2 > 0 || res.Item2 == 0 && res.Item3 != 0)
                {
                    var front = elements.Take(res.Item1);
                    var back = elements.Skip(res.Item1 + res.Item2);
                    //elements.RemoveRange(res.Item1, res.Item2);
                    //elements.InsertRange(res.Item1, value as IEnumerable<object>);
                    elements = new List<object>
                        (front.Concat(value as IEnumerable<object>).Concat(back));
                }
                else if (res.Item2 < 0)
                {
                    var front = elements.Take(res.Item1 + res.Item2 + 1);
                    var back = elements.Skip(res.Item1 + 1);
                    elements = new List<object>(front
                        .Concat((value as IEnumerable<object>).Reverse())
                        .Concat(back));
                }
                else
                {
                    elements[res.Item1] = value;
                }     
            }
        }

        [MemberMap("_cons", MapModifier.Static, MapType.Constructor)]
        public KList(int f)
        {
            elements = new List<object>(f);
        }

        public KList(IEnumerable<object> objs)
        {
            //vector.AddRange(obj);
            elements = objs.ToList();
        }

        public KList(params object[] objs)
        {
            //vector.AddRange(obj);
            elements = objs.ToList();
        }

        [MemberMap("insert", MapModifier.Instance, MapType.Method)]
        public void Insert(int index, object obj)
        {
            elements.Insert(index, obj);
        }

        [MemberMap("add", MapModifier.Instance, MapType.Method, true)]
        public void Add(object obj, params object[] objs)
        {
            elements.Add(obj);
            if(objs.Length > 0)
                elements.AddRange(objs);
        }

        [MemberMap("addRange", MapModifier.Instance, MapType.Method)]
        public void AddRange(object obj)
        {
            if(obj is KList)
            {
                elements.AddRange((obj as KList).elements);
            }
        }

        [MemberMap("remove", MapModifier.Instance, MapType.Method)]
        public void Remove(object obj)
        {
            var res = IndexParser.ParseIndex(obj, Count);
            int start = res.Item1, len = res.Item2;
            if (len == 0)
                elements.RemoveAt(start);
            else
                elements.RemoveRange(start, len);
        }

        [MemberMap("sort", MapModifier.Static, MapType.Method)]
        public static void Sort(KList list, Function comp = null)
        {
            if (comp == null)
                list.elements.Sort();
            else
                list.elements.Sort((x, y) => Convert.ToInt32(comp.Invoke(null, x, y)));
        }

        [MemberMap("sort", MapModifier.Instance, MapType.Method)]
        public void Sort(Function comp)
        {
            Sort(this, comp);
        }

        [MemberMap("sort", MapModifier.Instance, MapType.Method)]
        public void Sort()
        {
            Sort(this);
        }

        [MemberMap("shuffle", MapModifier.Instance, MapType.Method)]
        public void Shuffle()
        {
            Random ran = new Random();
            for (int i = 0; i < elements.Count; i++)
            {
                int j = ran.Next(0, elements.Count);
                object temp = elements[i];
                elements[i] = elements[j];
                elements[j] = temp;
            }
        }

        [MemberMap("_add", MapModifier.Instance, MapType.Method)]
        public object _Add(object obj)
        {
            if (obj is KList)
            {
                return new KList(this.Union((obj as KList)).ToArray());
            }
            throw new Exception();
        }

        [MemberMap("_mul", MapModifier.Instance, MapType.Method)]
        public object Multiply(object obj)
        {
            if (obj is KList)
            {
                return new KList(
                    this.Zip((obj as KList), 
                    (o1, o2) => new KTuple(o1, o2)).ToArray());
            }
            else if (obj.GetType().IsValueType)
            {
                double k = Convert.ToDouble(obj);
                return new KList(this
                    .Select(e => k * Convert.ToDouble(e))
                    .Cast<object>()
                    .ToArray());
            }
            throw new Exception();
        }

        [MemberMap("_dmul", MapModifier.Instance, MapType.Method)]
        public KList DoubleMultiply(int time)
        {
            var temp = new KList();
            for(int i = 0; i < time; i++)
            {
                temp.AddRange(this);
            }
            return temp;
        }

        public override KString ToStr()
        {
            return KUtil.ToString(elements);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            else if (obj is KList)
            {
                var li = (obj as KList).elements;
                if (li.Count != elements.Count) return false;
                //elements.Zip((obj as KList), (x, y) => x.Equals(y)).Aggregate((x,y) => x && y);
                for (int i = 0; i < Count; i++)
                {
                    if (!li[i].Equals(elements[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
