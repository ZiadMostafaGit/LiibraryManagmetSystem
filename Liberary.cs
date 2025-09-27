using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace PROGRAM
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class BorrowedItemException : Exception
    {
        public BorrowedItemException(string msg) : base(msg) { }
    }






    public class InvalidCategoryException : Exception
    {
        public InvalidCategoryException(string str) : base(str) { }
    }

    public class InvlaidDurationException : Exception
    {
        public InvlaidDurationException(string str) : base(str) { }
    }

    public class InvalidIdException : Exception
    {
        public InvalidIdException(string msg) : base(msg) { }
    }
    public class InvalidPhoneException : Exception
    {
        public InvalidPhoneException(string msg) : base(msg) { }
    }

    public class InvalidBalanceValueException : Exception
    {
        public InvalidBalanceValueException(string msg) : base(msg) { }

    }

    public class InvalidPriceException : Exception
    {
        public InvalidPriceException(string msg) : base(msg) { }
    }
public class SoldItemException : Exception
    {
        public SoldItemException(string msg) : base(msg) { }
    }


    public enum ItemState
    {
        Avelable,
        Borrowed,
        Sold,
    }


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
                {
                    throw new ArgumentException("Name cannot be empty or whitespace.");
                }
                if (value.All(char.IsDigit))
                {
                    throw new ArgumentException("Name cannot be digits.");
                }
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
        {
            throw new InvalidPhoneException("Input has non-digit characters.");
        }

        if (cleanedValue.Length != 11)
        {
            throw new InvalidPhoneException("Phone number must be 11 digits.");
        }

        if (!cleanedValue.StartsWith("01"))
        {
            throw new InvalidPhoneException("Phone number must start with 01.");
        }

        int counterOfRepeatedDigits = 1;
        for (int i = 1; i < cleanedValue.Length; i++)
        {
            if (cleanedValue[i] == cleanedValue[i - 1])
            {
                counterOfRepeatedDigits++;
                if (counterOfRepeatedDigits > 3)
                {
                    throw new InvalidPhoneException("Phone number has more than 3 repeated digits.");
                }
            }
            else
            {
                counterOfRepeatedDigits = 1;
            }
        }

        _phone = cleanedValue;
    }
}


        public Decimal Balance
        {
            get => _balance;
            set
            {

                if (value < 0)
                {
                    throw new InvalidBalanceValueException("invalid balance cant be nigative");

                }

                _balance = value;


            }
        }



        public User(string name, string phone, Decimal balance)
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
        protected decimal _itemState;

        public decimal ItemState{ get; set; }




        protected Item(int id, string title, decimal price)
        {
            if (id < 0 || id > 10000)
                throw new InvalidIdException("not valid id");

            _id = id;
            _title = title.ToUpper();
            _itemState = ItemState.Avelable;
            _price = price >= 0 ? price : throw new InvalidPriceException("Price must be >= 0");
        }

        public int ID
        {
            get => _id;
            set
            {
                if (value < 0 || value > 10000)
                {
                    throw new InvalidIdException("not valid id");
                }
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
                {
                    throw new InvalidPriceException("your input is not a valid price");
                }
                _price = value;

            }
        }


        public abstract void DisplayInfo();
    }

    // ---------------------- BOOK ----------------------
    public class Book : Item
    {
        private string _auther;
        public Book(int id, string title, decimal price, string auther)
            : base(id, title, price)
        {
            _auther = auther.ToUpper();
        }

        public string Auther
        {
            get => _auther;
            set => _auther = value.ToUpper();
        }

        public override void DisplayInfo()
        {
            Console.WriteLine("Book id is " + _id + " and Book name is " + _title + " and book auther is " + _auther + " and its price is " + _price);
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
            {
                throw new InvalidDataException("the date you enter is ether too old or in the fuecher");
            }
            _pubDate = date;
        }


        public override void DisplayInfo()
        {
            Console.WriteLine("magazine id is " + _id + " and magazine name is " + _title + " and publish date is " + _pubDate + " and its price is " + _price);
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
            {
                throw new InvlaidDurationException("the duration is invalid");
            }
            _duration = duration;
        }

        public double Duration => _duration;


        public override void DisplayInfo()
        {
            Console.WriteLine($"DVD id is {_id}, title is {_title}, duration is {_duration} hours, and its price is {_price}");
        }
    }

    public class LibraryEntry
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public ItemState State { get; set; } = ItemState.Avelable;

    }
    // ---------------------- LIBRARY ----------------------


    public class LibraryInventory
    {

        public Dictionary<string, Dictionary<int, LibraryEntry>> data;

        public IEnumerable<string> GetCategories()
        {
            return data.Keys;
        }

        public IEnumerable<LibraryEntry> GetItemsInCategory(string category)
        {
            if (!data.ContainsKey(category))
                return new List<LibraryEntry>();

            return data[category].Values;
        }



        public bool IsEmpty()
        {
            return this.data.Count == 0;
        }


        public LibraryEntry GetItem(string category, int itemId)
        {
            if (!this.data.ContainsKey(category))
            {
                throw new InvalidCategoryException("Invalid category");
            }

            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException($"The item with ID {itemId} is not in the library");
            }

            return this.data[category][itemId];
        }
        public LibraryInventory()
        {
            data = new Dictionary<string, Dictionary<int, LibraryEntry>>();

        }

        public void AddNewItem(string key, Item i, int quantity)
        {
            if (!this.data.ContainsKey(key))
            {
                this.data[key] = new Dictionary<int, LibraryEntry>();
            }

            if (!this.data[key].ContainsKey(i.ID))
            {
                this.data[key][i.ID] = new LibraryEntry { Item = i, Quantity = quantity };
            }
            else
            {
                this.data[key][i.ID].Quantity += quantity;
                if (this.data[key][i.ID].Quantity > 0 && this.data[key][i.ID].State != ItemState.Avelable)
                {
                    this.data[key][i.ID].State = ItemState.Avelable;

                }
            }


        }



        public bool BorrowItem(string category, int itemId)
        {
            if (!this.data.ContainsKey(category))
            {
                throw new InvalidCategoryException("Invalid category");
            }

            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException($"The item with ID {itemId} is not in the library");
            }

            LibraryEntry libraryItem = this.data[category][itemId];

            if (libraryItem.Quantity == 0)
            {
                if (libraryItem.State == ItemState.Borrowed)
                {
                    throw new BorrowedItemException("sorry the item borrowed and will be avelable soon");
                }
                else if (libraryItem.State == ItemState.Sold)
                {
                    throw new SoldItemException("item sold out");
                }
            }

            if (libraryItem.Quantity > 0)
            {
                libraryItem.Quantity -= 1;

                if (libraryItem.Quantity == 0)
                {
                    libraryItem.State = ItemState.Borrowed;
                }

            }
            return true;

        }





        public void SellItem(string category, int itemId)
        {
            if (!this.data.ContainsKey(category))
            {
                throw new InvalidCategoryException("category dose not exsist");
            }

            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException("item id dose not esist");
            }

            LibraryEntry libraryItem = this.data[category][itemId];

            if (libraryItem.Quantity == 0)
            {
                if (libraryItem.State == ItemState.Borrowed)
                {
                    throw new BorrowedItemException("Item borrowed and will return soon");
                }
                else if (libraryItem.State == ItemState.Sold)
                {
                    throw new SoldItemException("Item sold out");
                }
            }

            if (libraryItem.Quantity > 0)
            {
                libraryItem.Quantity -= 1;

                if (libraryItem.Quantity == 0)
                {
                    libraryItem.State = ItemState.Sold;
                }

            }


        }

        public void ReturnItem(string category, int itemId)
        {
            if (!this.data.ContainsKey(category))
            {
                throw new InvalidCategoryException("category dose not exsist");
            }
            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException("invalid id");
            }

            LibraryEntry libraryItem = this.data[category][itemId];

            if (libraryItem.State == ItemState.Sold)
            {
                throw new SoldItemException($"item {itemId} is sold and cant be returned");
            }
            if (libraryItem.State == ItemState.Borrowed)
            {
                libraryItem.State = ItemState.Avelable;

            }

            libraryItem.Quantity += 1;
            

        }

        public void RestockItem(string category, int itemId, int additionalQuantity)
        {
            if (!this.data.ContainsKey(category))
            {
                throw new InvalidCategoryException("category dose not exsist");
            }
            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException("invalid id");
            }
            else
            {
                LibraryEntry libraryItem = this.data[category][itemId];
                libraryItem.Quantity += additionalQuantity;
                if (libraryItem.State != ItemState.Avelable)
                {
                    libraryItem.State = ItemState.Avelable;
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



    public class LibraryReporting
    {

        private LibraryInventory Inventory;
        private LibraryFinance Finance;

        public LibraryReporting(LibraryFinance finance, LibraryInventory inventory)
        {
            this.Finance = finance;
            this.Inventory = inventory;
        }

        public void DisplayLibraryStatus()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("                 LIBRARY STATUS REPORT");
            Console.WriteLine(new string('=', 60));

            if (this.Inventory.IsEmpty())
            {
                Console.WriteLine("Library is empty!");
                return;
            }

            foreach (var category in this.Inventory.GetCategories())
            {
                Console.WriteLine($"\n📚 CATEGORY: {category.ToUpper()}");
                Console.WriteLine(new string('-', 40));

                foreach (var entry in this.Inventory.GetItemsInCategory(category))
                {
                    Console.WriteLine($"ID: {entry.Item.ID} | Title: {entry.Item.Title}");
                    Console.WriteLine($"   Price: ${entry.Item.Price} | Quantity: {entry.Quantity} | State: {entry.State}");

                    // Display specific item details
                    if (entry.Item is Book book)
                    {
                        Console.WriteLine($"   Author: {book.Auther}");
                    }
                    else if (entry.Item is Magazine magazine)
                    {
                        Console.WriteLine($"   Publish Date: {magazine.PublishDate}");
                    }
                    else if (entry.Item is DVD dvd)
                    {
                        Console.WriteLine($"   Duration: {dvd.Duration} hours");
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("💰 FINANCIAL SUMMARY:");
            Console.WriteLine($"   Money from Borrowing: ${this.Finance.BorrowingCurrent:F2}");
            Console.WriteLine($"   Money from Selling: ${this.Finance.SellingCurrent:F2}");
            Console.WriteLine($"   Total Revenue: ${(this.Finance.BorrowingCurrent + this.Finance.SellingCurrent):F2}");
            Console.WriteLine(new string('=', 60));
        }

        public void DisplayAvailableItems()
        {
            Console.WriteLine("\n📖 AVAILABLE ITEMS FOR BORROWING/SELLING:");
            Console.WriteLine(new string('-', 50));

            bool hasAvailable = false;
            foreach (var category in this.Inventory.GetCategories())
            {
                foreach (var entry in this.Inventory.GetItemsInCategory(category))
                {
                    if (entry.Quantity > 0)
                    {
                        hasAvailable = true;
                        Console.WriteLine($"[{category}] {entry.Item.Title} - Quantity: {entry.Quantity} - Price: ${entry.Item.Price}");
                    }
                }
            }

            if (!hasAvailable)
            {
                Console.WriteLine("No items currently available!");
            }
        }



    }






    public class Library
    {



        private LibraryFinance finance;
        private LibraryInventory inventory;
        private LibraryReporting report;


        public Library()
        {
            inventory = new LibraryInventory();
            finance = new LibraryFinance();
            report = new LibraryReporting(finance, inventory);
        }


        public void AddNewItem(string key, Item i, int quantity)
        {
            this.inventory.AddNewItem(key, i, quantity);
            var libitem = inventory.GetItem(key, i.ID);
            Console.WriteLine($"Restocked {libitem.Item.Title}. New quantity: {libitem.Quantity}");

        }

        public void BorrowItem(string category, int itemId)
        {
            try
            {
                var libitem = this.inventory.GetItem(category, itemId);
                this.inventory.BorrowItem(category, itemId);

                this.finance.FinancialBorrowingTransaction(libitem.Item.Price);
                Console.WriteLine("Borrow transaction completed successfully");
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        public void SellItem(string category, int itemId)
        {
            try
            {
                var libitem = this.inventory.GetItem(category, itemId);
                this.inventory.SellItem(category, itemId);

                this.finance.FinancialSellingTransaction(libitem.Item.Price);
                Console.WriteLine("Selling transaction completed successfully");
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
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

    public class Customer : User
    {
        private class CustomerItem
        {
            public Item item { get; set; }
            public CustomerItemState customerItemState { get; set; }
        }

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

