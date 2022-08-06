using MagicEastern.IndexedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Test
{
    class Program
    {
        class Data
        {
            public int id { get; set; }
            public string str { get; set; }
            public int? maybenull { get; set; }
        }


        static void Main(string[] args)
        {
            Test();
        }

        public static void Test()
        {
            int TOTAL = 15000;
            int IDRANGE = 1000;

            // prepare
            List<Data> list = new List<Data>();
            IIndexedList<Data> indexedList1 = new IndexedList<Data>();
            indexedList1.IndexBy(i => i.id);
            Random r = new Random();
            for (int i = 0; i < TOTAL; i++)
            {
                int n = r.Next(IDRANGE);
                Data d = new Data { id = n, str = n.ToString() };
                list.Add(d);
            }


            Stopwatch sw;
            int ct;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                indexedList1.Add(list[i]);
            }
            sw.Stop();
            Console.WriteLine("IndexedList.Add " + TOTAL + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");


            var indexedList2 = list.IndexBy(i => i.id);
            var indexedList3 = list.IndexBy(i => i.maybenull);
            var nulls = indexedList3.LookFor<int?>("maybenull", null);
            
            
            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = list.Where(j => j.id == list[i].id);
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("List.Where lookup " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");

            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = indexedList1.LookFor("id", list[i].id);
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("IndexedList1 lookup by index name " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");

          
            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = indexedList2.LookFor(new { list[i].id });
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("IndexedList2 lookup by index name " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }



}
