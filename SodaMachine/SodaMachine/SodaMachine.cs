#nullable disable
using SodaMachineExtensions;

namespace SodaMachine
{
    public class SodaMachine
    {
        public static int currentBalance { get; set; }
        public SodaInventory inventory { get; set; }
        public SodaOrders orders { get; set; }
        private List<MachineCommand> availableCommands { get; set; }
        public SodaMachine()
        {
            currentBalance = 0;
            inventory = new SodaInventory();
            orders = new SodaOrders();
        }

        /// <summary>
        /// This is the starter method for the machine
        /// </summary>
        public void Start()
        {

            //Initialize the machine
            InitializeMachineCommands();
            WriteInitScreen();

            //Standby for user input
            while (true)
            {
                OutputBalance();
                var input = Console.ReadLine();

                //refresh inventory before executing command to ensure sync with API
                inventory.RefreshInventory();
                availableCommands.ParseAndExecuteCommand(input);
            }
        }

        #region SodaMachineCommands
        /// <summary>
        /// Retrives the library of available commands for the sodamachine
        /// </summary>
        public void InitializeMachineCommands()
        {
            var clearCommand = new MachineCommand
            {
                Identifier = "clear",
                Description = "- Clears the console window for text",
                Execute = (string parameter1, string parameter2) => {
                    Console.Clear();
                }
            };

            var claimCommand = new MachineCommand
            {
                Identifier = "claim",
                Description = "[retrivalPin] - Claims a reserved soda",
                Execute = (string parameter1, string parameter2) => {
                    AttemptClaimSoda(parameter1);
                }
            };

            /// Displays a overview of the machine's current stock
            var stockCommand = new MachineCommand
            {
                Identifier = "stock",
                Description = "- Displays a overview of the machine's current stock",
                Execute = (string parameter1, string parameter2) => {
                    foreach (var soda in inventory.Sodas)
                    {
                        Console.WriteLine(soda.Name + ": " + soda.Stock + " left in stock(" + soda.Reserved +" Reserved) - Cost: " + soda.Price + " Credits");
                    }
                }
            };

            /// Inserts additional credits to the machine balance
            var insertCommand = new MachineCommand
            {
                Identifier = "insert",
                Description = "[addedCredits] - Inserts additional credits to the machine balance",
                Execute = (string parameter1, string parameter2) => {
                    AttemptAddCredits(parameter1);
                }
            };

            /// Orders the specified soda, and dispenses it given sufficient funds and stock
            var orderCommand = new MachineCommand
            {
                Identifier = "order",
                Description = "[nameOfSoda] - Orders the specified soda, and dispenses it given sufficient funds and stock",
                Execute = (string parameter1, string parameter2) => {
                    var sodaToDispense = inventory.GetSoda(parameter1, parameter2);
                    AttemptDispenseSoda(ref sodaToDispense);
                }
            };

            /// Recalls all current credit of the machine
            var recallCommand = new MachineCommand
            {
                Identifier = "recall",
                Description = "- Recalls all current credit of the machine",
                Execute = (string parameter1, string parameter2) => {
                    ReturnChange();
                }
            };

            /// Returns a description of all available commands for the user
            var helpCommand = new MachineCommand
            {
                Identifier = "help",
                Description = "- Returns a description of all available commands for the user",
                Execute = (string parameter1, string parameter2) => {
                    foreach (var command in availableCommands)
                    {
                        Console.WriteLine(command.Identifier + " " + command.Description);
                    }
                }
            };
            availableCommands = new List<MachineCommand> { stockCommand, insertCommand, orderCommand, recallCommand, helpCommand, clearCommand, claimCommand };
        }
        #endregion

        #region MachineActions

        /// <summary>
        /// Safely inserts a specified amount of currency, returns error message if amount is invalid
        /// </summary>
        /// <param name="creditAmount">The amount of credits to insert</param>
        private static void AttemptAddCredits(string creditAmount)
        {
            var invalidCreditError = "Invalid credit amound specified, please try again!";
            //Add to credit
            try
            {
                var addedCredits = int.Parse(creditAmount);
                if (addedCredits < 0)
                    Console.WriteLine(invalidCreditError);
                else
                {
                    currentBalance += addedCredits;
                    Console.WriteLine("Adding " + addedCredits + " to current balance");
                }
            }
            catch (Exception)
            {
                Console.WriteLine(invalidCreditError);
            }
        }

        /// <summary>
        /// Checks predicates for dispensing a soda, and does so if criterias are met. Returns true if sucessfull
        /// </summary>
        /// <param name="sodaToDispense">The soda to dispense</param>
        private bool AttemptDispenseSoda(ref Soda sodaToDispense, bool pinOk = false)
        {

            // insuficcient funds to buy soda, calculate and return difference
            if (currentBalance < sodaToDispense.Price)
            {
                Console.WriteLine("Need " + (sodaToDispense.Price - currentBalance) + " more credits");
            }

            // soda out of stock
            else if (sodaToDispense.Stock <= 0)
            {
                Console.WriteLine("No " + sodaToDispense.Name + " left in stock");
            }

            // no unreserved soda left in stock
            else if (sodaToDispense.Stock <= sodaToDispense.Reserved && pinOk == false)
            {
                Console.WriteLine("No unreserved " + sodaToDispense.Name + " left in stock");
            }

            // soda in stock, and sufficient funds. Dispense the soda.
            else
            {
                currentBalance -= sodaToDispense.Price;
                Console.WriteLine("Giving " + sodaToDispense.Name + " out");
                ReturnChange();
                sodaToDispense.Stock--;

                //if we are claiming a reserved soda, decrement the reserved counter
                if (pinOk == true)
                    sodaToDispense.Reserved--;

                inventory.SaveInventoryState();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks predicates for claiming and dispensing a soda, and does so if criterias are met
        /// </summary>
        /// <param name="sodaToDispense">The soda to dispense</param>
        private void AttemptClaimSoda(string sPin)
        {
            var pin = 0;
            try{
                pin = int.Parse(sPin);
                var sodaIsClaimable = orders.OrderWithPinExsists(pin);
                if (sodaIsClaimable){
                    var sodaName = orders.GetSodaNameWithPin(pin);
                    var sodaToDispense = inventory.GetSoda(sodaName);
                    var successfullDispense = AttemptDispenseSoda(ref sodaToDispense, sodaIsClaimable == true);
                    if(successfullDispense == true){
                        orders.MarkOrderAsComplete(pin);
                    }
                }
                else{
                    Console.WriteLine("No matching reservation found, are you sure you used the right pin?");
                }

            }
            catch (Exception){
                Console.WriteLine("Invalid pin submitted, please try again");
            }
        }

        /// <summary>
        /// Dispenses remaining change, and resets credit-balance
        /// </summary>
        private void ReturnChange()
        {
            Console.WriteLine("Giving " + currentBalance + " out in change");
            currentBalance = 0;
        }
        #endregion

        #region TextFunctions
        /// <summary>
        /// Outputs a welcome message to the display
        /// </summary>
        private void WriteInitScreen()
        {
            Console.WriteLine("\n\nWelcome to easee Fizz-Buzz Soda!:");
            Console.WriteLine("Please type \"help\" if you would like to see what commands are available");
        }

        /// <summary>
        /// Outputs the current credit-balance to the display
        /// </summary>
        private void OutputBalance()
        {
            Console.WriteLine("-------");
            Console.WriteLine("Inserted Credits: " + currentBalance);
            Console.WriteLine("-------\n\n");
        }
        #endregion
    }
}
