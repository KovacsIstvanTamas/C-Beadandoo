using System.Collections.Concurrent;
using System.Threading;

namespace ConsoleApp1
{

    class DataProcessor
    {
        private ConcurrentDictionary<int, string> dataDictionary = new ConcurrentDictionary<int, string>();
        
        /// <summary>
        /// Hozzáad egy új kulcs-érték párt a gyűjteményhez, ha a kulcs még nem létezik.
        /// </summary>
        /// <param name="key">A hozzáadni kívánt kulcs.</param>
        /// <param name="value">A hozzáadni kívánt érték.</param>
        public void AddData(int key, string value)
        {
            dataDictionary.TryAdd(key, value);
        }

        /// <summary>
        /// Törli a megadott kulccsal rendelkező elemet a gyűjteményből.
        /// </summary>
        /// <param name="key">A törlendő elem kulcsa.</param>
        /// <returns>True, ha sikerült eltávolítani az elemet; különben false.</returns>
        public bool RemoveData(int key)
        {
            string removedValue;
            return dataDictionary.TryRemove(key, out removedValue);
        }

        /// <summary>
        /// Ellenőrzi, hogy a gyűjtemény tartalmazza-e a megadott kulcsot.
        /// </summary>
        /// <param name="key">Az ellenőrizni kívánt kulcs.</param>
        /// <returns>True, ha a gyűjtemény tartalmazza a megadott kulcsot; különben false.</returns>
        public bool ContainsKey(int key)
        {
            return dataDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Visszaadja a gyűjteményben található elemek számát.
        /// </summary>
        /// <value>A gyűjteményben található elemek száma.</value>
        public int Count
        {
            get { return dataDictionary.Count; }
        }

        /// <summary>
        /// Törli az összes elemet a gyűjteményből.
        /// </summary>
        public void ClearData()
        {
            dataDictionary.Clear();
        }

        /// <summary>
        /// Frissíti a megadott kulccsal rendelkező elem értékét a gyűjteményben.
        /// Ha a kulcs már létezik, az értékét frissíti az új értékre, egyébként hozzáadja az új kulcs-érték párost.
        /// </summary>
        /// <param name="key">A frissíteni kívánt elem kulcsa.</param>
        /// <param name="newValue">Az új érték.</param>
        public void UpdateData(int key, string newValue)
        {
            dataDictionary.AddOrUpdate(key, newValue, (oldKey, oldValue) => newValue);
        }

        /// <summary>
        /// Feldolgozza az összes adatot a gyűjteményben, aszinkron módon.
        /// </summary>
        public void ProcessData()
        {
            CountdownEvent countdownEvent = new CountdownEvent(dataDictionary.Count);

            foreach (var kvp in dataDictionary)
            {
                ThreadPool.QueueUserWorkItem(ProcessDataItem, new object[] { kvp.Key, kvp.Value, countdownEvent });
            }

            countdownEvent.Wait();
            Console.WriteLine("Az összes adat feldolgozva.");
        }

        /// <summary>
        /// Feldolgozza az adott kulccsal és értékkel rendelkező adatot.
        /// </summary>
        /// <param name="state">A feldolgozandó adat és a CountdownEvent.</param>
        private void ProcessDataItem(object state)
        {
            var parameters = (object[])state;
            int key = (int)parameters[0];
            string value = (string)parameters[1];
            CountdownEvent countdownEvent = (CountdownEvent)parameters[2];

            Console.WriteLine($"Feldolgozás... Kulcs: {key}, Érték: {value}");

            // Szimuláljuk a feldolgozási időt
            Thread.Sleep(3000);

            // Feldolgozás befejezve, csökkentjük a CountdownEvent számlálóját
            countdownEvent.Signal();
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            DataProcessor processor = new DataProcessor();

            // Adatok hozzáadása a különálló osztályhoz
            int index = 0;
            Console.WriteLine("Adatok hozzáadása");
            foreach (var data in Enumerable.Range(0, 20).Select(i => $"Data{i}"))
            {
                processor.AddData(index++, data);
            }

            // Adatok feldolgozása
            processor.ProcessData();

            // Kiírjuk a gyűjtemény méretét
            Console.WriteLine($"A gyűjtemény mérete: {processor.Count}");

            Console.ReadLine();
            // Kulcs jelenlétének ellenőrzése
            Console.WriteLine("Kulcs jelenlétének ellenőrzése");
            bool containsKey = processor.ContainsKey(10);
            Console.WriteLine($"Contains key 10: {containsKey}");
            Console.ReadLine();

            // Adatok eltávolítása
            index = 0;
            Console.WriteLine("Adatok eltávolítása");
            foreach (var data in Enumerable.Range(0, 11).Select(i => $"Data{i}"))
            {
                processor.RemoveData(index++);
            }

            // Adatok feldolgozása
            processor.ProcessData();
            Console.ReadLine();
            Console.WriteLine($"A gyűjtemény mérete: {processor.Count}");

            // Kulcs jelenlétének ellenőrzése
            Console.WriteLine("Kulcs jelenlétének ellenőrzése");
            containsKey = processor.ContainsKey(10);
            Console.WriteLine($"Contains key 10: {containsKey}");
            Console.ReadLine();

            // Gyűjtemény ürítése
            Console.WriteLine("Gyűjtemény ürítése");
            processor.ClearData();
            Console.WriteLine($"A gyűjtemény mérete: {processor.Count}");

            // Adatok feldolgozása
            processor.ProcessData();
            Console.ReadLine();

            // Új adatok hozzáadása
            Console.WriteLine("Új adatok hozzáadása");
            processor.AddData(20, "Data20");
            processor.AddData(21, "Data21");
            Console.WriteLine($"A gyűjtemény mérete: {processor.Count}");

            // Adatok feldolgozása
            processor.ProcessData();
            Console.ReadLine();

            // Adatok frissítése
            Console.WriteLine("Adatok frissítése");
            processor.UpdateData(20, "UpdatedData20");

            // Adatok feldolgozása
            processor.ProcessData();

            Console.ReadLine();
        }
    }
}