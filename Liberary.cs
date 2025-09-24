using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace PROGRAM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;




    public class BorrowedItemException : Exception {
    public BorrowedItemException(string msg):base(msg){}
}

    public class SoldItemException : Exception {
    public SoldItemException(string msg):base(msg){}
}






    public class InvalidCategoryException : Exception {
        public InvalidCategoryException(string str):base(str){}
    }

    public class InvlaidDurationException : Exception {
        public InvlaidDurationException(string str) : base(str) { }
    }

    public class InvalidIdException : Exception {
        public InvalidIdException(string msg):base(msg){}
}
    public class InvalidPhoneExeption : Exception {
        public InvalidPhoneExeption(string msg) : base(msg){}
}

    public class InvalidBalanceValueException : Exception {
        public InvalidBalanceValueException(string msg):base(msg){}

}

    public class InvalidPriceException : Exception {
    public InvalidPriceException(string msg):base(msg){}
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
            set => _name = value.Trim();
        }
        public string Phone
        {
            get => _phone;
            set
            {
                if (!value.All(char.IsDigit))
                {
                    throw new InvalidPhoneExeption("input has non digit characters");
                }
                _phone = value;
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

        protected Item(int id, string title, decimal price)
        {
            if (id < 0 || id > 10000)
                throw new InvalidIdException("not valid id");

            _id = id;
            _title = title.ToUpper();
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
            set{
                if (value<0)
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

    // ---------------------- LIBRARY ----------------------
    public class Library
    {
        protected class LibraryEntry
        {
            public Item Item { get; set; }
            public int Quantity { get; set; }
            public ItemState State { get; set; } = ItemState.Avelable;
        }

        private Dictionary<string, Dictionary<int, LibraryEntry>> data;
        private decimal _monyFromBorrow;
        private decimal _monyFromSelling;

        public decimal MonyFromBorrow
        {
            get => _monyFromBorrow;
            set => _monyFromBorrow = value;
        }
        public decimal MonyFromSelling
        {
            get => _monyFromSelling;
            set => _monyFromSelling = value;
        }

        public Library()
        {
            data = new Dictionary<string, Dictionary<int, LibraryEntry>>();
        }

        public bool AddItem(string key, Item i, int quantity)
        {
            if (!this.data.ContainsKey(key))
            {
                this.data[key] = new Dictionary<int, LibraryEntry>();
            }

            if (!this.data[key].ContainsKey(i.ID))
            {
                this.data[key][i.ID] = new LibraryEntry { Item = i, Quantity = quantity };
                return true;
            }
            else
            {
                this.data[key][i.ID].Quantity += quantity;
                if (this.data[key][i.ID].Quantity > 0 && this.data[key][i.ID].State != ItemState.Avelable)
                {
                    this.data[key][i.ID].State = ItemState.Avelable;

                }
            }
            Console.WriteLine($"Restocked {i.Title}. New quantity: {this.data[key][i.ID].Quantity}");

            return true;

        }


        public void BorrowItem(string category, int itemId)
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
                _monyFromBorrow += libraryItem.Item.Price * 0.1m;

            }
                Console.WriteLine($"Successfully borrowed {libraryItem.Item.Title}");

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
                _monyFromSelling += libraryItem.Item.Price;

            }

                Console.WriteLine($" {libraryItem.Item.Title} Successfully Sold");

        }

        public void ReturnItem(string category, int itemId)
        {
            if (!this.data.ContainsKey(category) )
            {
                throw new InvalidCastException("category dose not exsist");
            }
            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException("invalid id");
            }

            LibraryEntry libraryItem = this.data[category][itemId];

            if (libraryItem.State != ItemState.Borrowed)
            {
                throw new BorrowedItemException("item is not borrowed in the first place");
            }

                libraryItem.Quantity += 1;
                libraryItem.State = ItemState.Avelable;

                Console.WriteLine($"{libraryItem.Item.Title} returned successfully");
        }

        public void RestockItem(string category, int itemId, int additionalQuantity)
        {
              if (!this.data.ContainsKey(category) )
            {
                throw new InvalidCastException("category dose not exsist");
            }
            if (!this.data[category].ContainsKey(itemId))
            {
                throw new InvalidIdException("invalid id");
            }else
            {
                LibraryEntry libraryItem = this.data[category][itemId];
                libraryItem.Quantity += additionalQuantity;
                if (libraryItem.State != ItemState.Avelable)
                {
                    libraryItem.State = ItemState.Avelable;
                }
            }
        }

        public void DisplayLibraryStatus()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("                 LIBRARY STATUS REPORT");
            Console.WriteLine(new string('=', 60));

            if (data.Count == 0)
            {
                Console.WriteLine("Library is empty!");
                return;
            }

            foreach (var category in data.Keys)
            {
                Console.WriteLine($"\n📚 CATEGORY: {category.ToUpper()}");
                Console.WriteLine(new string('-', 40));

                foreach (var entry in data[category].Values)
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
            Console.WriteLine($"   Money from Borrowing: ${_monyFromBorrow:F2}");
            Console.WriteLine($"   Money from Selling: ${_monyFromSelling:F2}");
            Console.WriteLine($"   Total Revenue: ${(_monyFromBorrow + _monyFromSelling):F2}");
            Console.WriteLine(new string('=', 60));
        }

        public void DisplayAvailableItems()
        {
            Console.WriteLine("\n📖 AVAILABLE ITEMS FOR BORROWING/SELLING:");
            Console.WriteLine(new string('-', 50));

            bool hasAvailable = false;
            foreach (var category in data.Keys)
            {
                foreach (var entry in data[category].Values)
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




    public enum CustomerItemState
    {
        buyed,
        borrowed,
    }



    public class Customer : User
    {


        private class CustomerItem
        {
            public Item item { get; set; }
            public CustomerItemState custoemrItemSate { get; set; }
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
            citem.custoemrItemSate = CustomerItemState.buyed;
            this.CustomerItems.Add(citem);
        }

        public void BorrowItem(Item item)
        {
            CustomerItem citem = new CustomerItem();
            citem.item = item;
            citem.custoemrItemSate = CustomerItemState.borrowed;
            this.CustomerItems.Add(citem);
        }


        public void DecreseBalance(Decimal cost)
        {
            if (Balance <cost)
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
        try
        {
            customer.DecreseBalance(item.Price);    
            library.SellItem(category, item.ID);   
            customer.BuyItem(item);

            Console.WriteLine("Customer bought the item successfully");
            return true;
        }
        catch (InvalidBalanceValueException ex)
        {
            Console.WriteLine($"❌ Purchase failed: {ex.Message}");
            return false;
        }
        catch (InvalidCategoryException ex)
        {
            Console.WriteLine($"❌ Purchase failed: {ex.Message}");
            return false;
        }
        catch (InvalidIdException ex)
        {
            Console.WriteLine($"❌ Purchase failed: {ex.Message}");
            return false;
        }
        catch (BorrowedItemException ex)
        {
            Console.WriteLine($"❌ Purchase failed: {ex.Message}");
            return false;
        }
        catch (SoldItemException ex)
        {
            Console.WriteLine($"❌ Purchase failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error during purchase: {ex.Message}");
            return false;
        }
    }

    public bool CustomerBorrowItem(Library library, Customer customer, string category, Item item)
    {
        try
        {
            library.BorrowItem(category, item.ID);   
            customer.BorrowItem(item);

            Console.WriteLine("Customer borrowed the item successfully");
            return true;
        }
        catch (InvalidCategoryException ex)
        {
            Console.WriteLine($"❌ Borrow failed: {ex.Message}");
            return false;
        }
        catch (InvalidIdException ex)
        {
            Console.WriteLine($"❌ Borrow failed: {ex.Message}");
            return false;
        }
        catch (BorrowedItemException ex)
        {
            Console.WriteLine($"❌ Borrow failed: {ex.Message}");
            return false;
        }
        catch (SoldItemException ex)
        {
            Console.WriteLine($"❌ Borrow failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error during borrowing: {ex.Message}");
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
        catch (InvalidCategoryException ex)
        {
            Console.WriteLine($"❌ Return failed: {ex.Message}");
            return false;
        }
        catch (InvalidIdException ex)
        {
            Console.WriteLine($"❌ Return failed: {ex.Message}");
            return false;
        }
        catch (BorrowedItemException ex)
        {
            Console.WriteLine($"❌ Return failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error during return: {ex.Message}");
            return false;
        }
    }
}













}

