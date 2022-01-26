#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace SodaMachineExtensions
{
    public static class SodaMachineExtensions
    {
        /// <summary>
        /// Parses the input of a Soda-Machine user and attempts to run the respective command from available commands
        /// </summary>
        /// <param name="availableCommands">The collection of available commands</param>
        /// <param name="input">The users input</param>
        public static void ParseAndExecuteCommand(this List<MachineCommand> availableCommands, string input)
        {
            //divide the different parts of the command into an array
            var commandAndParams = input.Split(' ');

            //assign the command and respective parameters
            var intputCommand = commandAndParams[0];
            var par1 = commandAndParams.Length > 1 ? commandAndParams[1] : "";
            var par2 = commandAndParams.Length > 2 ? commandAndParams[2] : "";

            // Find the command amongst the available commands, and execute it, if no match return error
            var commandToExecute = availableCommands.FirstOrDefault(x => x.Identifier == intputCommand);
            if (commandToExecute != null)
            {
                commandToExecute.Execute(par1, par2);
            }
            else
            {
                Console.WriteLine("Invalid command given, try command \"help\" if you would like to learn about the available commands");
            }
        }
    }

    /// <summary>
    /// Soda with a name, price and stock attached to it
    /// </summary>
    public class Soda
    {
        public string Name { get; set; }
        public int Stock { get; set; }
        public int Price { get; set; }
        public int Reserved { get; set; }

    }

    /// <summary>
    /// A collection of sodas, accompanied with physical storage file and functions to manipulate it
    /// </summary>
    public class SodaInventory
    {
        public SodaInventory()
        {
            string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.FullName;
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

        public Soda GetSoda(string soda, string subcategory = "")
        {
            var key = soda;
            if (subcategory.Length > 0)
                key += " " + subcategory;

            return Sodas[GetSodaIndex(key)];
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

        public void SaveInventoryState()
        {
            var newInventoryState = JsonConvert.SerializeObject(Sodas);
            File.WriteAllText(InventoryFile, newInventoryState);
        }
        #endregion
    }

    // <summary>
    /// A collection of orders submitted to the API
    /// </summary>
    public class SodaOrders
    {
        public List<SodaOrder> Orders { get; set; }
        private string StorageFile { get; set; }

        public SodaOrders(){
            string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.FullName;
            StorageFile = dir + @"\SodaOrders.json";
        }

        #region SodaOrderStorage

        private void RefreshStorage()
        {
            using (StreamReader file = new StreamReader(StorageFile))
            {
                string json = file.ReadToEnd();
                List<SodaOrder> sodas = JsonConvert.DeserializeObject<List<SodaOrder>>(json);
                Orders = sodas;
            }
        }

        public List<SodaOrder> GetOrders(){
            RefreshStorage();
            return Orders;
        }

        public string GetSodaNameWithPin(int pin){
            return Orders.Find(order => order.PinCode == pin).Soda;
        }

        public bool OrderWithPinExsists(int pin) {
            // sucessfull if there exsists an order in orders which has a matching pin
            var success = GetOrders().FindIndex(x => x.PinCode == pin) != -1;
            return success;
        }

        public void MarkOrderAsComplete(int pin)
        {
            var newOrders = GetOrders();

            var orderIndex = newOrders.FindIndex(x => x.PinCode == pin);
            if (orderIndex >= 0){
                newOrders[orderIndex].IsComplete = true;
                SaveOrders(newOrders);
            }


        }

        public void SaveOrders(List<SodaOrder> orders){
            var newInventoryState = JsonConvert.SerializeObject(orders);
            File.WriteAllText(StorageFile, newInventoryState);
        }
        #endregion
    }

    /// <summary>
    /// A order to reserve soda for claim at machine, compliant with API structure
    /// </summary>
    public class SodaOrder
    {
        public long Id { get; set; }
        public string Soda { get; set; }
        public int PinCode { get; set; }
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Delegate used to attach functions of up to 2 parameters to a MachineCommand
    /// </summary>
    public delegate void Command(string parameter1, string parameter2);

    /// <summary>
    /// A Soda-Machine command which is available for execution by the user
    /// </summary>
    public class MachineCommand
    {

        public string Identifier { get; set; }
        public string Description { get; set; }
        public Command Execute { get; set; }

    }
}
