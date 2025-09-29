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

    // ---------------------- INTERFACES ----------------------
    public interface IItemReader
    {
        IEnumerable<string> GetCategories();
        IEnumerable<Item> GetAllItems(string category);
        Item GetItemById(int itemId);
    }

    public interface IItemWriter
    {
        void AddNewItem(string category, Item item, int quantity);
        int RestockItem(string category, int itemId, int additionalQuantity);
    }

    public interface IItemBorrowable
    {
        Item BorrowItem(string category, string title);
        Item BorrowItem(string category, int itemId);
        void ReturnItem(string category, int itemId);
    }

    public interface IItemSellable
    {
        Item SellItem(string category, int itemId);
    }

    // ---------------------- INVENTORY (IN-MEMORY) ----------------------
    public class LibraryInventory :
        IItemReader, IItemWriter, IItemBorrowable, IItemSellable
    {
        private List<Item> items = new();
        private Dictionary<string, Dictionary<string, List<int>>> catalog =
            new(StringComparer.OrdinalIgnoreCase);

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
                int newId = items.Count;
                Item copy = item switch
                {
                    Book b => new Book(newId, b.Title, b.Price, b.Author),
                    Magazine m => new Magazine(newId, m.Title, m.Price, m.PublishDate),
                    DVD d => new DVD(newId, d.Title, d.Price, d.Duration),
                    _ => throw new InvalidOperationException("Unknown item type")
                };

                copy.InventoryIndex = newId;
                items.Add(copy);
                catalog[category][item.Title].Add(newId);
            }
        }

        public IEnumerable<string> GetCategories() => catalog.Keys;

        public IEnumerable<Item> GetAllItems(string category)
        {
            if (!catalog.ContainsKey(category))
                yield break;

            foreach (var title in catalog[category].Keys)
                foreach (var idx in catalog[category][title])
                    yield return items[idx];
        }

        public Item GetItemById(int itemId)
        {
            if (itemId < 0 || itemId >= items.Count)
                throw new InvalidIdException("Item ID does not exist");
            return items[itemId];
        }

        public Item BorrowItem(string category, string title)
        {
            if (!catalog.ContainsKey(category) || !catalog[category].ContainsKey(title))
                throw new InvalidCategoryException("Invalid category or title");

            foreach (var idx in catalog[category][title])
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

        public Item BorrowItem(string category, int itemId)
        {
            var it = GetItemById(itemId);
            if (it.State != ItemState.Available)
                throw new BorrowedItemException("This copy is not available for borrowing");

            it.State = ItemState.Borrowed;
            return it;
        }

        public void ReturnItem(string category, int itemId)
        {
            var it = GetItemById(itemId);
            if (it.State != ItemState.Borrowed)
                throw new InvalidOperationException("Item was not borrowed");

            it.State = ItemState.Available;
        }

        public Item SellItem(string category, int itemId)
        {
            var it = GetItemById(itemId);
            if (it.State != ItemState.Available)
                throw new SoldItemException("This copy cannot be sold (not available)");

            it.State = ItemState.Sold;
            return it;
        }

        public int RestockItem(string category, int itemId, int additionalQuantity)
        {
            if (additionalQuantity <= 0)
                throw new ArgumentException("additionalQuantity must be > 0");

            var existing = GetItemById(itemId);

            for (int i = 0; i < additionalQuantity; i++)
            {
                int newId = items.Count;
                Item copy = existing switch
                {
                    Book b => new Book(newId, b.Title, b.Price, b.Author),
                    Magazine m => new Magazine(newId, m.Title, m.Price, m.PublishDate),
                    DVD d => new DVD(newId, d.Title, d.Price, d.Duration),
                    _ => throw new InvalidOperationException("Unknown item type")
                };

                copy.InventoryIndex = newId;
                items.Add(copy);
            }

            return additionalQuantity;
        }
    }

    // ---------------------- FINANCE ----------------------
    public class LibraryFinance
    {
        private Decimal _BorrowingCurrent;
        private Decimal _sellingCurrent;

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
    public class LibraryReporting
    {
        private readonly IItemReader reader;
        private readonly LibraryFinance finance;

        public LibraryReporting(IItemReader reader, LibraryFinance finance)
        {
            this.reader = reader;
            this.finance = finance;
        }

        public void DisplayLibraryStatus()
        {
            Console.WriteLine("----- Library Status Report -----");
            Console.WriteLine($"Borrowing Revenue: {finance.BorrowingCurrent}");
            Console.WriteLine($"Selling Revenue: {finance.SellingCurrent}");

            foreach (var category in reader.GetCategories())
            {
                Console.WriteLine($"Category: {category}");
                foreach (var item in reader.GetAllItems(category))
                    item.DisplayInfo();
            }
        }
    }

    // ---------------------- OPERATIONS ----------------------
    public enum OperationType { Borrow, Sell, Return }

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
        public int ItemId { get; set; }
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

    public class BorrowingTransaction : ITransaction
    {
        private readonly IItemBorrowable borrowable;
        private readonly LibraryFinance finance;

        public BorrowingTransaction(IItemBorrowable borrowable, LibraryFinance finance)
        {
            this.borrowable = borrowable;
            this.finance = finance;
        }

        public void Execute(TransactionRequest request)
        {
            var req = (BorrowRequest)request;
            borrowable.BorrowItem(req.Category, req.ItemId);
            finance.FinancialBorrowingTransaction(req.Charge);
        }
    }

    public class SellingTransaction : ITransaction
    {
        private readonly IItemSellable sellable;
        private readonly LibraryFinance finance;

        public SellingTransaction(IItemSellable sellable, LibraryFinance finance)
        {
            this.sellable = sellable;
            this.finance = finance;
        }

        public void Execute(TransactionRequest request)
        {
            var req = (SellRequest)request;
            sellable.SellItem(req.Category, req.ItemId);
            finance.FinancialSellingTransaction(req.Charge);
        }
    }

    public class ReturnTransaction : ITransaction
    {
        private readonly IItemBorrowable borrowable;

        public ReturnTransaction(IItemBorrowable borrowable)
        {
            this.borrowable = borrowable;
        }

        public void Execute(TransactionRequest request)
        {
            var req = (ReturnRequest)request;
            borrowable.ReturnItem(req.Category, req.ItemId);
        }
    }

    // ---------------------- LIBRARY (FACADE) ----------------------
    public class Library
    {
        private readonly IItemReader reader;
        private readonly IItemWriter writer;
        private readonly IItemBorrowable borrowable;
        private readonly IItemSellable sellable;
        private readonly LibraryFinance finance;
        private readonly LibraryReporting reporting;
        private readonly Dictionary<OperationType, ITransaction> transactions;

        public Library(IItemReader r, IItemWriter w, IItemBorrowable b, IItemSellable s, LibraryFinance f)
        {
            reader = r;
            writer = w;
            borrowable = b;
            sellable = s;
            finance = f;
            reporting = new LibraryReporting(reader, finance);

            transactions = new Dictionary<OperationType, ITransaction>
            {
                { OperationType.Borrow, new BorrowingTransaction(b, f) },
                { OperationType.Sell, new SellingTransaction(s, f) },
                { OperationType.Return, new ReturnTransaction(b) }
            };
        }

        public void AddNewItem(string category, Item item, int quantity) => writer.AddNewItem(category, item, quantity);
        public void RestockItem(string category, int itemId, int qty) => writer.RestockItem(category, itemId, qty);

        public void ExecTransaction(OperationType type, TransactionRequest request)
        {
            if (transactions.ContainsKey(type))
                transactions[type].Execute(request);
        }

        public void ShowReport() => reporting.DisplayLibraryStatus();
    }
}
