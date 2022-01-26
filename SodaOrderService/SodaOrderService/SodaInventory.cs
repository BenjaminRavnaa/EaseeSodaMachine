#nullable disable
using Newtonsoft.Json;

namespace SodaOrderService {
    public class SodaInventory {
        public SodaInventory()
        {
            string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            InventoryFile = dir + @"\inventory.json";
        }
        public List<Soda> Sodas { get; set; }
        private string InventoryFile { get; set; }

        #region Inventory Functions

        public int GetSodaIndex(string key)
        {
            var inventoryIndex = Sodas.FindIndex(x => x.Name == key);
            return inventoryIndex;
        }

        public void ReserveSoda(string sodaKey)
        {
            RefreshInventory();
            Sodas[GetSodaIndex(sodaKey)].Reserved++;
            SaveInventoryState();
        }

        public void SaveInventoryState()
        {
            var newInventoryState = JsonConvert.SerializeObject(Sodas);
            File.WriteAllText(InventoryFile, newInventoryState);
        }

        public void RefreshInventory()
        {
            using (StreamReader file = new StreamReader(InventoryFile))
            {
                string json = file.ReadToEnd();
                List<Soda> sodas = JsonConvert.DeserializeObject<List<Soda>>(json);
                Sodas = sodas;
            }
        }
        #endregion

        public class Soda {
            public string Name { get; set; }
            public int Stock { get; set; }
            public int Price { get; set; }
            public int Reserved { get; set; }

        }
    }
}
