namespace PROGRAM
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text.RegularExpressions;

    // ---------------------- EXCEPTIONS ----------------------
    public class BorrowedItemException : Exception { public BorrowedItemException(string msg) : base(msg) { } }
    public class InvalidCategoryException : Exception { public InvalidCategoryException(string str) : base(str) { } }
    public class InvalidDurationException : Exception { public InvalidDurationException(string str) : base(str) { } }
    public class InvalidIdException : Exception { public InvalidIdException(string msg) : base(msg) { } }
    public class InvalidPhoneException : Exception { public InvalidPhoneException(string msg) : base(msg) { } }
    public class InvalidBalanceValueException : Exception { public InvalidBalanceValueException(string msg) : base(msg) { } }
    public class InvalidPriceException : Exception { public InvalidPriceException(string msg) : base(msg) { } }
    public class SoldItemException : Exception { public SoldItemException(string msg) : base(msg) { } }

    // ---------------------- ENUMS ----------------------
    public enum ItemState
    {
        Available,
        Borrowed,
        Sold,
    }

    // ---------------------- USER ----------------------
    public abstract class User
    {
        protected string _name;
        protected string _phone;
        protected decimal _balance;

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be empty or whitespace.");
                if (value.All(char.IsDigit))
                    throw new ArgumentException("Name cannot be digits.");
                _name = value.Trim().ToUpper();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                string cleanedValue = Regex.Replace(value, @"[^\d]", "");

                if (!cleanedValue.All(char.IsDigit))
                    throw new InvalidPhoneException("Input has non-digit characters.");
                if (cleanedValue.Length != 11)
                    throw new InvalidPhoneException("Phone number must be 11 digits.");
                if (!cleanedValue.StartsWith("01"))
                    throw new InvalidPhoneException("Phone number must start with 01.");

                int counter = 1;
                for (int i = 1; i < cleanedValue.Length; i++)
                {
                    if (cleanedValue[i] == cleanedValue[i - 1])
                    {
                        counter++;
                        if (counter > 3)
                            throw new InvalidPhoneException("Phone number has more than 3 repeated digits.");
                    }
                    else counter = 1;
                }

                _phone = cleanedValue;
            }
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (value < 0)
                    throw new InvalidBalanceValueException("Invalid balance, cannot be negative");
                _balance = value;
            }
        }

        public User(string name, string phone, decimal balance)
        {
            Name = name;
            Phone = phone;
            Balance = balance;
        }
    }

    // ---------------------- ITEM ----------------------
    public abstract class Item
    {
        protected int _id;
        protected string _title;
        protected decimal _price;
        protected ItemState _state;

        // New: track inventory index for quick return
        public int InventoryIndex { get; set; }

        public ItemState State
        {
            get => _state;
            set => _state = value;
        }

        protected Item(int id, string title, decimal price)
        {
            if (id < 0 || id > 10000)
                throw new InvalidIdException("Not valid ID");

            _id = id;
            _state = ItemState.Available;
            _title = title.ToUpper();
            _price = price >= 0 ? price : throw new InvalidPriceException("Price must be >= 0");
        }

        public int ID
        {
            get => _id;
            set
            {
                if (value < 0 || value > 10000)
                    throw new InvalidIdException("Not valid ID");
                _id = value;
            }
        }

        public string Title
        {
            get => _title;
            set => _title = value.ToUpper();
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new InvalidPriceException("Invalid price, must be >= 0");
                _price = value;
            }
        }

        public abstract void DisplayInfo();
    }

    // ---------------------- BOOK ----------------------
    public class Book : Item
    {
        private string _author;
        public Book(int id, string title, decimal price, string author)
            : base(id, title, price)
        {
            _author = author.ToUpper();
        }

        public string Author
        {
            get => _author;
            set => _author = value.ToUpper();
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"Book ID: {_id}, Title: {_title}, Author: {_author}, Price: {_price}");
        }
    }

    // ---------------------- MAGAZINE ----------------------
    public class Magazine : Item
    {
        private int _pubDate;
        public int PublishDate => _pubDate;

        public Magazine(int id, string title, decimal price, int date)
            : base(id, title, price)
        {
            if (date < 1000 || date > 2026)
                throw new InvalidDataException("The date you entered is invalid.");
            _pubDate = date;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"Magazine ID: {_id}, Title: {_title}, Publish Date: {_pubDate}, Price: {_price}");
        }
    }

    // ---------------------- DVD ----------------------
    public class DVD : Item
    {
        private double _duration;

        public DVD(int id, string title, decimal price, double duration)
            : base(id, title, price)
        {
            if (duration < 0 || duration > 10)
                throw new InvalidDurationException("The duration is invalid");
            _duration = duration;
        }

        public double Duration => _duration;

        public override void DisplayInfo()
        {
            Console.WriteLine($"DVD ID: {_id}, Title: {_title}, Duration: {_duration} hours, Price: {_price}");
        }
    }

    public class LibraryInventory
    {
        private List<Item> items;
        public Dictionary<string, Dictionary<string, Queue<int>>> data;

        public LibraryInventory()
        {
            items = new List<Item>();
            data = new Dictionary<string, Dictionary<string, Queue<int>>>();
        }

        public void AddNewItem(string category, Item item, int quantity)
        {
            if (!data.ContainsKey(category))
                data[category] = new Dictionary<string, Queue<int>>();

            if (!data[category].ContainsKey(item.Title))
                data[category][item.Title] = new Queue<int>();

            for (int i = 0; i < quantity; i++)
            {
                items.Add(item);
                int index = items.Count - 1;
                data[category][item.Title].Enqueue(index);
            }
        }

        public Item BorrowItem(string category, string title)
        {
            if (!data.ContainsKey(category) || !data[category].ContainsKey(title))
                throw new InvalidCategoryException("Invalid category or title");

            var queue = data[category][title];
            if (queue.Count == 0)
                throw new BorrowedItemException($"{title} is not available");

            foreach (var idx in queue)
            {
                var item = items[idx];
                if (item.State == ItemState.Available)
                {
                    item.State = ItemState.Borrowed;
                    return item;
                }
            }

            // If all items are borrowed/sold
            throw new BorrowedItemException($"{title} has no available copies to borrow will be added soon");
        }


        public Item SellItem(string category, string title)
        {
            if (!data.ContainsKey(category) || !data[category].ContainsKey(title))
                throw new InvalidCategoryException("Invalid category or title");

            var queue = data[category][title];
            if (queue.Count == 0)
                throw new SoldItemException($"{title} is out of stock");

            int idx = queue.Dequeue();
            var item = items[idx];

            if (item.State != ItemState.Available)
                throw new SoldItemException($"{title} cannot be sold (already borrowed/sold)");

            item.State = ItemState.Sold;
            return item;
        }


        public void ReturnItem(string category, string title, Item item)
        {
            if (!data.ContainsKey(category) || !data[category].ContainsKey(title))
                throw new InvalidCategoryException("Invalid category or title");

            if (item.State != ItemState.Borrowed)
                throw new InvalidOperationException("Item was not borrowed");

            item.State = ItemState.Available;
        }

        public bool IsEmpty()
        {
            return items.Count == 0;
        }
        public IEnumerable<string> GetCategories()
        {
            return data.Keys;
        }
        public IEnumerable<Item> GetAllItems(string category)
        {
            if (!data.ContainsKey(category))
                throw new InvalidCategoryException("Invalid category");

            foreach (var title in data[category].Keys)
            {
                foreach (var index in data[category][title])
                {
                    yield return items[index];
                }
            }
        }
    }





    public class LibraryFinance
    {
        private Decimal _BorrowingCurrent;
        private Decimal _sellingCurrent;


        public LibraryFinance()
        {
            this._BorrowingCurrent = this._sellingCurrent = 0;
        }

        public Decimal BorrowingCurrent
        {
            get => _BorrowingCurrent;

        }

        public Decimal SellingCurrent
        {
            get => _sellingCurrent;
        }

        public void FinancialBorrowingTransaction(Decimal charge)
        {



            this._BorrowingCurrent += charge * 0.1m;
        }

        public void FinancialSellingTransaction(decimal charge)
        {

            this._sellingCurrent += charge;
        }




    }

    public class LibraryReportData
    {
        public List<CategoryReportData> Categories { get; set; } = new List<CategoryReportData>();
        public decimal BorrowingRevenue { get; set; }
        public decimal SellingRevenue { get; set; }
        public decimal TotalRevenue => BorrowingRevenue + SellingRevenue;
    }

    public class CategoryReportData
    {
        public string Category { get; set; }
        public List<ItemReportData> Items { get; set; } = new List<ItemReportData>();
    }

    public class ItemReportData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public ItemState State { get; set; }
        public string ExtraDetails { get; set; } // Author, Publish Date, Duration, etc.
        public void GetExtraDetails(Item i)
        {
            if (i is Book book)
                ExtraDetails = book.Author;

            if (i is Magazine m)
                ExtraDetails = m.PublishDate.ToString();
            if (i is DVD d)
                ExtraDetails = d.Duration.ToString();

        }
    }


    public class LibraryReportBuilder
    {
        private LibraryFinance finance;
        private LibraryInventory inventory;

        public LibraryReportBuilder(LibraryFinance finance, LibraryInventory inventory)
        {
            this.finance = finance;
            this.inventory = inventory;
        }
        public LibraryReportData Builder()
        {
            var reportData = new LibraryReportData
            {
                BorrowingRevenue = finance.BorrowingCurrent,
                SellingRevenue = finance.SellingCurrent
            };
            foreach (var category in inventory.GetCategories())
            {
                var categoryData = new CategoryReportData { Category = category };
                foreach (var item in inventory.GetAllItems(category))
                {
                    var itemdate = new ItemReportData
                    {
                        Id = item.ID,
                        Title = item.Title,
                        Price = item.Price,
                        State = item.State
                    };
                    itemdate.GetExtraDetails(item);
                    categoryData.Items.Add(itemdate);
                }
                reportData.Categories.Add(categoryData);
            }
            return reportData;
        }
    }


    public class LibraryReporting
    {
        private LibraryFinance finance;
        private LibraryInventory inventory;
        private LibraryReportBuilder reportBuilder;

        public LibraryReporting(LibraryFinance finance, LibraryInventory inventory)
        {
            this.finance = finance;
            this.inventory = inventory;
            this.reportBuilder = new LibraryReportBuilder(finance, inventory);
        }

        public void DisplayLibraryStatus()
        {
            var reportData = reportBuilder.Builder();

            Console.WriteLine("----- Library Status Report -----");
            Console.WriteLine($"Total Borrowing Revenue: ${reportData.BorrowingRevenue:F2}");
            Console.WriteLine($"Total Selling Revenue: ${reportData.SellingRevenue:F2}");
            Console.WriteLine($"Overall Total Revenue: ${reportData.TotalRevenue:F2}");
            Console.WriteLine();

            foreach (var category in reportData.Categories)
            {
                Console.WriteLine($"Category: {category.Category}");
                foreach (var item in category.Items)
                {
                    Console.WriteLine($"  ID: {item.Id}, Title: {item.Title}, Price: ${item.Price:F2}, State: {item.State}, Extra: {item.ExtraDetails}");
                }
                Console.WriteLine();
            }
        }

        public void DisplayAvailableItems()
        {
            if (inventory.IsEmpty())
            {
                Console.WriteLine("No items available in the library.");
                return;
            }

            Console.WriteLine("----- Available Items -----");
            foreach (var category in inventory.GetCategories())
            {
                Console.WriteLine($"Category: {category}");
                foreach (var item in inventory.GetAllItems(category).Where(i => i.State == ItemState.Available))
                {
                    item.DisplayInfo();
                }
                Console.WriteLine();
            }
        }
    }


    public enum OperationType
    {
        Borrow,
        Sell,
        Return,
        Restock
    }




    public class LibraryOperationRecord
    {
        private static int nextOperationId = 0;
        public int operationId;
        public int userId { get; set; }
        public int itemId { get; set; }

        public OperationType operationType { get; set; }
        public DateTime operationDate { get; set; }
        public int operationBerid { get; set; }
        public LibraryOperationRecord(int itemid, int userid, OperationType type, DateTime date, int berid)
        {
            nextOperationId++;
            this.operationId = nextOperationId;
            this.itemId = itemid;
            this.userId = userid;
            this.operationType = type;
            this.operationDate = date;
            this.operationBerid = berid;
        }
    }


    public class LibraryOperationHistory
    {
        private List<LibraryOperationRecord> records;

        public LibraryOperationHistory()
        {
            records = new List<LibraryOperationRecord>();
        }

        public void AddRecord(LibraryOperationRecord record)
        {
            records.Add(record);
        }

        public IEnumerable<LibraryOperationRecord> GetAllRecords()
        {
            return records;
        }
    }





    public interface ITransaction
    {
        public void Execute(string category, string itemTitle, int userId, int itemId, decimal charge, int berid);
    }

    public class BorrowingTransaction : ITransaction
    {
        private readonly LibraryFinance finance;
        private readonly LibraryInventory inventory;
        private readonly LibraryOperationHistory history;

        public BorrowingTransaction(LibraryFinance f, LibraryInventory inv, LibraryOperationHistory his)
        {
            this.finance = f;
            this.history = his;
            this.inventory = inv;
        }

        public void Execute(string category, string itemTitle, int userId, int itemId, decimal charge, int berid)
        {
            LibraryOperationRecord libraryOperationRecord = new LibraryOperationRecord(itemId, userId, OperationType.Borrow, DateTime.Now, berid);
            try
            {
                Item i = this.inventory.BorrowItem(category, itemTitle);

                try
                {
                    this.finance.FinancialBorrowingTransaction(charge);
                    this.history.AddRecord(libraryOperationRecord);
                    Console.WriteLine("Borrowing transaction completed successfully");
                }
                catch (Exception err)
                {
                    this.inventory.ReturnItem(category, itemTitle, i);
                    Console.WriteLine(err);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                throw;
            }

        }




    }


    public class SellingTransaction : ITransaction
    {
        private readonly LibraryFinance finance;
        private readonly LibraryInventory inventory;
        private readonly LibraryOperationHistory history;

        public SellingTransaction(LibraryFinance f, LibraryInventory inv, LibraryOperationHistory his)
        {
            this.finance = f;
            this.history = his;
            this.inventory = inv;
        }

        public void Execute(string category, string itemTitle, int userId, int itemId, decimal charge, int berid)
        {
            try
            {
                Item i = inventory.SellItem(category, itemTitle);
                try
                {
                    finance.FinancialSellingTransaction(charge);
                    LibraryOperationRecord libraryOperationRecord = new LibraryOperationRecord(itemId, userId, OperationType.Sell, DateTime.Now, berid);
                    history.AddRecord(libraryOperationRecord);
                    Console.WriteLine("Item Sold Successfully");
                }
                catch (Exception err)
                {
                    inventory.ReturnItem(category, itemTitle, i);
                    Console.WriteLine(err);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

        }

}

    public class ReturnTransaction : ITransaction
    {

        private readonly LibraryFinance finance;
        private readonly LibraryInventory inventory;
        private readonly LibraryOperationHistory history;

        public ReturnTransaction(LibraryFinance f, LibraryInventory inv, LibraryOperationHistory his)
        {
            this.finance = f;
            this.history = his;
            this.inventory = inv;
        }


        public void Execute(string category, string itemTitle, int userId, int itemId, decimal charge, int berid)
        {
    
}




}


    public class Library
    {



        private LibraryFinance finance;
        private LibraryInventory inventory;
        private LibraryReporting report;
        private LibraryOperationHistory history;
        private Dictionary<OperationType, ITransaction> Transactions;


        public Library()
        {
            inventory = new LibraryInventory();
            finance = new LibraryFinance();
            report = new LibraryReporting(finance, inventory);
            history = new LibraryOperationHistory();
            Transactions = new Dictionary<OperationType, ITransaction>
            {
                {OperationType.Borrow,new BorrowingTransaction(finance,inventory,history)},
                {OperationType.Sell,new SellingTransaction(finance,inventory,history)}
            };
        }


        public void AddNewItem(string key, Item i, int quantity)
        {
            this.inventory.AddNewItem(key, i, quantity);
            Console.WriteLine($"Item added successfully");

        }

        public void ExecTransation(OperationType operationType, string category, string itemTitle, int userId, int itemId, decimal charge, int berid,Item item)
        {
            if (operationType is OperationType ot)

                Transactions[operationType].Execute(category, itemTitle, userId, itemId, charge, berid,item);

            else
                Console.WriteLine("unsupported operation");


        }

        public void ReturnItem(string category, int itemId)
        {
            try
            {
                this.inventory.ReturnItem(category, itemId);
                Console.WriteLine("Item returned successfully");
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        public void RestockItem(string category, int itemId, int quantity)
        {
            this.inventory.RestockItem(category, itemId, quantity);
        }


        public void DesplayLibraryStatus()
        {
            this.report.DisplayLibraryStatus();
        }

        public void DesplayAvelableItem()
        {
            this.report.DisplayAvailableItems();
        }

        public void GetItemQuantity(string category, int itemId)
        {
            var libitem = this.inventory.GetItem(category, itemId);
            Console.WriteLine($"Item '{libitem.Item.Title}' (ID: {itemId}) has quantity: {libitem.Quantity}");
        }



    }




    public enum CustomerItemState
    {
        Bought,
        Borrowed,
    }

        public class CustomerItem
        {
            public Item item { get; set; }
            public CustomerItemState customerItemState { get; set; }
        }
    public class Customer : User
    {

        private List<CustomerItem> CustomerItems;

        public Customer(string name, string phone, Decimal balance) : base(name, phone, balance)
        {
            this.CustomerItems = new List<CustomerItem>();
        }

        public void BuyItem(Item item)
        {
            CustomerItem citem = new CustomerItem();
            citem.item = item;
            citem.customerItemState = CustomerItemState.Bought;
            this.CustomerItems.Add(citem);
        }

        public void BorrowItem(Item item)
        {
            CustomerItem citem = new CustomerItem();
            citem.item = item;
            citem.customerItemState = CustomerItemState.Borrowed;
            this.CustomerItems.Add(citem);
        }

        public void DecreaseBalance(Decimal cost)
        {
            if (Balance < cost)
            {
                throw new InvalidBalanceValueException("balance is less than cost");
            }
            _balance -= cost;
        }

        public void IncreaseBalance(Decimal cost)
        {
            _balance += cost;
        }
    }

    public class TransactionService
    {
        public bool CustomerBuyItem(Library library, Customer customer, string category, Item item)
        {
            bool balanceDecreased = false;
            try
            {
                customer.DecreaseBalance(item.Price);
                balanceDecreased = true;

                library.SellItem(category, item.ID);

                customer.BuyItem(item);

                Console.WriteLine("Customer bought the item successfully");
                return true;
            }
            catch (InvalidBalanceValueException ex)
            {
                Console.WriteLine($"Purchase failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                if (balanceDecreased)
                {
                    customer.IncreaseBalance(item.Price);
                    Console.WriteLine("Balance restored due to purchase failure");
                }
                Console.WriteLine($"Purchase failed: {ex.Message}");
                return false;
            }
        }

        public bool CustomerBorrowItem(Library library, Customer customer, string category, Item item)
        {
            bool balanceDecreased = false;
            decimal borrowingFee = item.Price * 0.1m;

            try
            {
                customer.DecreaseBalance(borrowingFee);
                balanceDecreased = true;

                library.BorrowItem(category, item.ID);

                customer.BorrowItem(item);

                Console.WriteLine($"Customer borrowed the item successfully. Fee charged: ${borrowingFee:F2}");
                return true;
            }
            catch (InvalidBalanceValueException ex)
            {
                Console.WriteLine($"Borrow failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                if (balanceDecreased)
                {
                    customer.IncreaseBalance(borrowingFee);
                    Console.WriteLine("Balance restored due to borrow failure");
                }
                Console.WriteLine($"Borrow failed: {ex.Message}");
                return false;
            }
        }

        public bool CustomerReturnItem(Library library, Customer customer, string category, int itemId)
        {
            try
            {
                library.ReturnItem(category, itemId);
                Console.WriteLine("Customer returned the item successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Return failed: {ex.Message}");
                return false;
            }
        }
    }


}












