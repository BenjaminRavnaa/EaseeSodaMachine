#nullable disable
using System.Linq;
using SodaOrderService.Models;
using Newtonsoft.Json;

namespace SodaOrderService
{
    public class OrderStorage
    {
        public List<SodaOrder> Orders { get; set; }
        private string StorageFile { get; set; }

        public OrderStorage()
        {
            string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            StorageFile = dir + @"\SodaOrders.json";
        }

        private void RefreshStorage()
        {
            using (StreamReader file = new StreamReader(StorageFile))
            {
                string json = file.ReadToEnd();
                List<SodaOrder> sodas = JsonConvert.DeserializeObject<List<SodaOrder>>(json);
                Orders = sodas;
            }
        }

        public List<SodaOrder> GetOrders()
        {
            RefreshStorage();
            return Orders;
        }

        public void AddOrderToStorage (ref SodaOrder order)
        {
            var newOrders = GetOrders();

            order.PinCode = GetRetrivalPin();
            order.Id = GetOrderId();

            newOrders.Add(order);
            SaveOrders(newOrders);
        }

        public void ReserveSoda(ref SodaOrder order)
        {
            var newOrders = GetOrders();

            order.PinCode = GetRetrivalPin();
            order.Id = GetOrderId();

            newOrders.Add(order);
            SaveOrders(newOrders);
        }

        public void SaveOrders(List<SodaOrder> orders)
        {
            var newInventoryState = JsonConvert.SerializeObject(orders);
            File.WriteAllText(StorageFile, newInventoryState);
        }

        private int GetRetrivalPin()
        {
            var exclude = Orders.Select(x => x.PinCode).ToHashSet();
            var range = Enumerable.Range(1111, 9999).Where(i => !exclude.Contains(i));

            var randomGenerator = new Random();
            int index = randomGenerator.Next(0, 8888 - exclude.Count);
            return range.ElementAt(index);
        }

        private int GetOrderId()
        {
            if (Orders.Count <= 0)
                return 1;

            var highestIdFound = Orders.Select(x => x.Id).ToList().Max();
            return (int)highestIdFound+ 1;
        }
    }
}
