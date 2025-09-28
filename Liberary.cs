namespace PROGRAM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        // keep this - helps later for O(1) ops if we need them
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

    // ---------------------- LIBRARY INVENTORY (keeps your style) ----------------------
    public class LibraryInventory
    {
        // global items list (each copy is an Item instance)
        private List<Item> items;

        // catalog[category][title] => list of indices into items
        private Dictionary<string, Dictionary<string, List<int>>> catalog;

        public LibraryInventory()
        {
            items = new List<Item>();
            // ignore case on categories/titles so front-end can pass any case
            catalog = new Dictionary<string, Dictionary<string, List<int>>>(StringComparer.OrdinalIgnoreCase);
        }

        // add quantity copies of the given 'item' (we clone so caller's instance isn't reused)
        public void AddNewItem(string category, Item item, int quantity)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("category required");
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (quantity <= 0)
                throw new ArgumentException("quantity must be > 0");

            if (!catalog.ContainsKey(category))
                catalog[category] = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

            if (!catalog[category].ContainsKey(item.Title))
                catalog[category][item.Title] = new List<int>();

            for (int i = 0; i < quantity; i++)
            {
                // create a shallow clone with new ID (ID here used as the global index)
                Item copy;
                int newId = items.Count;

                if (item is Book b)
                    copy = new Book(newId, b.Title, b.Price, b.Author);
                else if (item is Magazine m)
                    copy = new Magazine(newId, m.Title, m.Price, m.PublishDate);
                else if (item is DVD d)
                    copy = new DVD(newId, d.Title, d.Price, d.Duration);
                else
                    throw new InvalidOperationException("Unknown item type");

                copy.InventoryIndex = newId;
                items.Add(copy);
                catalog[category][item.Title].Add(newId);
            }
        }

        public IEnumerable<string> GetCategories() => catalog.Keys;

        // return all copies for a category
        public IEnumerable<Item> GetAllItems(string category)
        {
            if (!catalog.ContainsKey(category))
                yield break;

            foreach (var title in catalog[category].Keys)
            {
                foreach (var idx in catalog[category][title])
                {
                    yield return items[idx];
                }
            }
        }

        // return copies for a specific title
        public IEnumerable<Item> GetCopies(string category, string title)
        {
            if (!catalog.ContainsKey(category))
                yield break;
            if (!catalog[category].ContainsKey(title))
                yield break;

            foreach (var idx in catalog[category][title])
                yield return items[idx];
        }

        public Item GetItemById(int itemId)
        {
            if (itemId < 0 || itemId >= items.Count)
                throw new InvalidIdException("Item ID does not exist");
            return items[itemId];
        }

        // borrow first available copy
        public Item BorrowItem(string category, string title)
        {
            if (!catalog.ContainsKey(category) || !catalog[category].ContainsKey(title))
                throw new InvalidCategoryException("Invalid category or title");

            var list = catalog[category][title];
            if (list.Count == 0)
                throw new BorrowedItemException($"{title} is not available");

            foreach (var idx in list)
            {
                var it = items[idx];
                if (it.State == ItemState.Available)
                {
                    it.State = ItemState.Borrowed;
                    return it;
                }
            }

            throw new BorrowedItemException($"{title} has no available copies to borrow");
        }

        // borrow a specific copy by itemId (frontend can pass item ID)
        public Item BorrowItem(string category, int itemId)
        {
            var it = GetItemById(itemId);
            if (!catalog.ContainsKey(category) || !catalog[category].ContainsKey(it.Title))
                throw new InvalidCategoryException("Invalid category or title");

            if (it.State != ItemState.Available)
                throw new BorrowedItemException("This copy is not available for borrowing");

            it.State = ItemState.Borrowed;
            return it;
        }

        // sell a specific copy (remove it from catalog list but keep object for history)
        public Item SellItem(string category, int itemId)
        {
            var it = GetItemById(itemId);
            if (!catalog.ContainsKey(category) || !catalog[category].ContainsKey(it.Title))
                throw new InvalidCategoryException("Invalid category or title");

            if (it.State != ItemState.Available)
                throw new SoldItemException("This copy cannot be sold (not available)");

            it.State = ItemState.Sold;

            var list = catalog[category][it.Title];
            // remove the id from the title list; O(n) in copies count (fine for small project)
            bool removed = list.Remove(itemId);

            // if you want to keep catalog clean, you could remove title if list empty - optional
            if (removed && list.Count == 0)
                catalog[category].Remove(it.Title);

            return it;
        }

        // return a borrowed-specific copy
        public void ReturnItem(string category, int itemId)
        {
            var it = GetItemById(itemId);
            if (it.State != ItemState.Borrowed)
                throw new InvalidOperationException("Item was not borrowed");

            it.State = ItemState.Available;

            if (!catalog.ContainsKey(category))
                throw new InvalidCategoryException("Invalid category");

            if (!catalog[category].ContainsKey(it.Title))
                catalog[category][it.Title] = new List<int>();

            var list = catalog[category][it.Title];
            if (!list.Contains(itemId))
                list.Add(itemId);
        }

        // add more copies based on an existing item (restock)
        public int RestockItem(string category, int itemId, int additionalQuantity)
        {
            if (additionalQuantity <= 0)
                throw new ArgumentException("additionalQuantity must be > 0");

            var existing = GetItemById(itemId);
            if (existing == null)
                throw new InvalidIdException("Item id not found");

            if (!catalog.ContainsKey(category))
                catalog[category] = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            if (!catalog[category].ContainsKey(existing.Title))
                catalog[category][existing.Title] = new List<int>();

            for (int i = 0; i < additionalQuantity; i++)
            {
                int newId = items.Count;
                Item copy;
                if (existing is Book b)
                    copy = new Book(newId, b.Title, b.Price, b.Author);
                else if (existing is Magazine m)
                    copy = new Magazine(newId, m.Title, m.Price, m.PublishDate);
                else if (existing is DVD d)
                    copy = new DVD(newId, d.Title, d.Price, d.Duration);
                else
                    throw new InvalidOperationException("Unknown item type");

                copy.InventoryIndex = newId;
                items.Add(copy);
                catalog[category][existing.Title].Add(newId);
            }

            return additionalQuantity;
        }

        public bool IsEmpty() => items.Count == 0;
    }

    // ---------------------- FINANCE ----------------------
    public class LibraryFinance
    {
        private Decimal _BorrowingCurrent;
        private Decimal _sellingCurrent;

        public LibraryFinance()
        {
            this._BorrowingCurrent = this._sellingCurrent = 0;
        }

        public Decimal BorrowingCurrent => _BorrowingCurrent;
        public Decimal SellingCurrent => _sellingCurrent;

        public void FinancialBorrowingTransaction(Decimal charge)
        {
            this._BorrowingCurrent += charge * 0.1m;
        }

        public void FinancialSellingTransaction(decimal charge)
        {
            this._sellingCurrent += charge;
        }
    }

    // ---------------------- REPORTING ----------------------
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
                ExtraDetails = $"Author: {book.Author}";
            else if (i is Magazine m)
                ExtraDetails = $"Publish Date: {m.PublishDate}";
            else if (i is DVD d)
                ExtraDetails = $"Duration: {d.Duration} hours";
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

    // ---------------------- OPERATIONS / HISTORY ----------------------
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

    // ---------------- INTERFACES & REQUESTS ----------------
    public interface ITransaction
    {
        void Execute(TransactionRequest request);
    }

    public abstract class TransactionRequest
    {
        public string Category { get; set; }
        public string ItemTitle { get; set; }
    }

    public class BorrowRequest : TransactionRequest
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }   // optional: if user picked a copy (set -1 if not)
        public decimal Charge { get; set; }
        public int Period { get; set; }
    }

    public class SellRequest : TransactionRequest
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public decimal Charge { get; set; }
    }

    public class ReturnRequest : TransactionRequest
    {
        public int ItemId { get; set; }
    }

    // ---------------- TRANSACTIONS ----------------
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

        public void Execute(TransactionRequest request)
        {
            var req = (BorrowRequest)request;
            LibraryOperationRecord record = new LibraryOperationRecord(
                req.ItemId, req.UserId, OperationType.Borrow, DateTime.Now, req.Period);

            try
            {
                Item i;
                // If ItemId provided -> borrow that copy; otherwise borrow first available
                if (req.ItemId >= 0)
                    i = this.inventory.BorrowItem(req.Category, req.ItemId);
                else
                    i = this.inventory.BorrowItem(req.Category, req.ItemTitle);

                try
                {
                    this.finance.FinancialBorrowingTransaction(req.Charge);
                    this.history.AddRecord(record);
                    Console.WriteLine("Borrowing transaction completed successfully");
                }
                catch (Exception err)
                {
                    // rollback inventory state if finance fails (we changed item state to borrowed above)
                    this.inventory.ReturnItem(req.Category, i.ID);
                    Console.WriteLine(err);
                    throw;
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

        public void Execute(TransactionRequest request)
        {
            var req = (SellRequest)request;

            try
            {
                Item i;
                if (req.ItemId >= 0)
                    i = inventory.SellItem(req.Category, req.ItemId);
                else
                    throw new InvalidOperationException("Selling requires a specific item id");

                try
                {
                    finance.FinancialSellingTransaction(req.Charge);
                    LibraryOperationRecord record = new LibraryOperationRecord(
                        req.ItemId, req.UserId, OperationType.Sell, DateTime.Now, 0);
                    history.AddRecord(record);
                    Console.WriteLine("Item Sold Successfully");
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                    throw;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                throw;
            }
        }
    }

    public class ReturnTransaction : ITransaction
    {
        private readonly LibraryInventory inventory;
        private readonly LibraryOperationHistory history;

        public ReturnTransaction(LibraryInventory inv, LibraryOperationHistory his)
        {
            this.history = his;
            this.inventory = inv;
        }

        public void Execute(TransactionRequest request)
        {
            var req = (ReturnRequest)request;
            try
            {
                this.inventory.ReturnItem(req.Category, req.ItemId);
                LibraryOperationRecord record = new LibraryOperationRecord(
                    req.ItemId, 0, OperationType.Return, DateTime.Now, 0);
                history.AddRecord(record);
                Console.WriteLine("Item returned successfully");
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                throw;
            }
        }
    }

    // ---------------- LIBRARY ----------------
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
                { OperationType.Borrow, new BorrowingTransaction(finance, inventory, history) },
                { OperationType.Sell, new SellingTransaction(finance, inventory, history) },
                { OperationType.Return, new ReturnTransaction(inventory, history) }
            };
        }

        public void AddNewItem(string category, Item i, int quantity)
        {
            this.inventory.AddNewItem(category, i, quantity);
            Console.WriteLine($"Item added successfully");
        }

        public void ExecTransaction(OperationType operationType, TransactionRequest request)
        {
            if (Transactions.ContainsKey(operationType))
                Transactions[operationType].Execute(request);
            else
                Console.WriteLine("Unsupported operation");
        }

        // convenience wrappers (keeps your older usage patterns)
        public void BorrowItem(string category, int itemId)
        {
            this.inventory.BorrowItem(category, itemId);
        }

        public void SellItem(string category, int itemId)
        {
            this.inventory.SellItem(category, itemId);
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
                throw;
            }
        }

        public void RestockItem(string category, int itemId, int quantity)
        {
            this.inventory.RestockItem(category, itemId, quantity);
        }

        // I kept your typo method names so it feels like you :)
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
            var itm = this.inventory.GetItemById(itemId);
            Console.WriteLine($"Item '{itm.Title}' (ID: {itemId}) has state: {itm.State}");
        }
    }

    // ---------------- CUSTOMER & TRANSACTION SERVICE ----------------
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
        // kept your approach, and adapted to new signatures (library sells by id)
        public bool CustomerBuyItem(Library library, Customer customer, string category, Item item)
        {
            bool balanceDecreased = false;
            try
            {
                customer.DecreaseBalance(item.Price);
                balanceDecreased = true;

                // library.SellItem expects category + itemId now
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

                // library.BorrowItem now uses category + itemId
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
