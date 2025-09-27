
namespace PROGRAM
{



class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== LIBRARY SYSTEM FUNCTIONALITY TESTS ===\n");

    // 1. Test Item Creation and Validation (Expanded)
    Console.WriteLine("--- Item Creation and Validation (Expanded) ---");
    // Valid Book
    try { var b = new Book(0, "lowercase title", 0m, "lowercase author"); b.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }
    try { var b = new Book(10000, "UPPERCASE TITLE", 99999.99m, "UPPERCASE AUTHOR"); b.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Valid Magazine
    try { var m = new Magazine(0, "magazine", 0m, 1000); m.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }
    try { var m = new Magazine(10000, "MAGAZINE", 99999.99m, 2026); m.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Valid DVD
    try { var d = new DVD(0, "dvd", 0m, 0.0); d.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }
    try { var d = new DVD(10000, "DVD", 99999.99m, 10.0); d.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }

    // Invalid Book IDs
    try { var b = new Book(-1, "Bad", 10m, "A"); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    try { var b = new Book(10001, "Bad", 10m, "A"); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid Book Price
    try { var b = new Book(1, "Bad", -0.01m, "A"); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Book casing
    try { var b = new Book(2, "MiXeD CaSe", 10m, "MiXeD CaSe"); b.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }

    // Invalid Magazine IDs
    try { var m = new Magazine(-1, "Bad", 1m, 2020); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    try { var m = new Magazine(10001, "Bad", 1m, 2020); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid Magazine Price
    try { var m = new Magazine(1, "Bad", -0.01m, 2020); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid Magazine Year (too old, too new)
    try { var m = new Magazine(1, "Bad", 1m, 999); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    try { var m = new Magazine(1, "Bad", 1m, 2027); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }

    // Invalid DVD IDs
    try { var d = new DVD(-1, "Bad", 1m, 1.0); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    try { var d = new DVD(10001, "Bad", 1m, 1.0); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid DVD Price
    try { var d = new DVD(1, "Bad", -0.01m, 1.0); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid DVD Duration
    try { var d = new DVD(1, "Bad", 1m, -0.01); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    try { var d = new DVD(1, "Bad", 1m, 10.01); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // DVD casing
    try { var d = new DVD(2, "MiXeD CaSe", 10m, 2.0); d.DisplayInfo(); } catch (Exception e) { Console.WriteLine(e.Message); }

    // 2. Test User Creation and Validation (Expanded)
    Console.WriteLine("\n--- User Creation and Validation (Expanded) ---");
    // Valid Customer
    try { var c = new Customer("Valid User", "01234567890", 100m); Console.WriteLine($"Customer: {c.Name}, {c.Phone}, {c.Balance}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Empty name
    try { var c = new Customer("", "01234567890", 100m); Console.WriteLine($"Customer: '{c.Name}'"); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Whitespace name
    try { var c = new Customer("   ", "01234567890", 100m); Console.WriteLine($"Customer: '{c.Name}'"); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Name casing
    try { var c = new Customer("MiXeD CaSe", "01234567890", 100m); Console.WriteLine($"Customer: {c.Name}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Min phone (all zeros)
    try { var c = new Customer("MinPhone", "00000000000", 100m); Console.WriteLine($"Customer: {c.Phone}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Max phone (all nines, 10 digits)
    try { var c = new Customer("MaxPhone", "99999999999", 100m); Console.WriteLine($"Customer: {c.Phone}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Invalid phone (letters)
    try { var c = new Customer("Bad Phone", "abc123", 100m); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid phone (special chars)
    try { var c = new Customer("Bad Phone", "0123-456-7890", 100m); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Invalid phone (too short)
    try { var c = new Customer("ShortPhone", "12345", 100m); Console.WriteLine($"Customer: {c.Phone}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Invalid phone (too long)
    try { var c = new Customer("LongPhone", "123456789012345", 100m); Console.WriteLine($"Customer: {c.Phone}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Min balance (zero)
    try { var c = new Customer("ZeroBalance", "1234567890", 0m); Console.WriteLine($"Customer: {c.Balance}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Max balance (large value)
    try { var c = new Customer("MaxBalance", "1234567890", decimal.MaxValue); Console.WriteLine($"Customer: {c.Balance}"); } catch (Exception e) { Console.WriteLine(e.Message); }
    // Negative balance
    try { var c = new Customer("Negative Balance", "1234567890", -10m); } catch (Exception e) { Console.WriteLine("Expected: " + e.Message); }
    // Duplicate phone (should be allowed, as no check in logic)
    try { var c1 = new Customer("User1", "5555555555", 10m); var c2 = new Customer("User2", "5555555555", 20m); Console.WriteLine($"Duplicate phone customers created: {c1.Phone}, {c2.Phone}"); } catch (Exception e) { Console.WriteLine(e.Message); }

        // 3. Test Library Inventory Operations
        Console.WriteLine("\n--- Library Inventory Operations ---");
        Library lib = new Library();
        Book bookA = new Book(100, "Book A", 20m, "Author A");
        Book bookB = new Book(101, "Book B", 30m, "Author B");
        Magazine magA = new Magazine(200, "Mag A", 5m, 2024);
        DVD dvdA = new DVD(300, "DVD A", 15m, 1.5);
        lib.AddNewItem("Books", bookA, 2);
        lib.AddNewItem("Books", bookB, 1);
        lib.AddNewItem("Magazines", magA, 3);
        lib.AddNewItem("DVDs", dvdA, 1);
        lib.DesplayLibraryStatus();

        // 4. Test Restocking
        Console.WriteLine("\n--- Restocking ---");
        lib.RestockItem("Books", 100, 3);
        lib.DesplayLibraryStatus();

        // 5. Test Borrowing, Selling, Returning (Direct Library)
        Console.WriteLine("\n--- Direct Borrowing ---");
        lib.BorrowItem("Books", 100); // Should succeed
        lib.BorrowItem("Books", 100); // Should succeed
        lib.BorrowItem("Books", 100); // Should succeed
        lib.BorrowItem("Books", 100); // Should succeed
        lib.BorrowItem("Books", 100); // Should succeed
        lib.BorrowItem("Books", 100); // Should fail (no more left)


        Console.WriteLine("\n--- Direct Selling ---");
        lib.SellItem("Books", 101); // Should succeed
        lib.SellItem("Books", 101); // Should fail (no more left)

        Console.WriteLine("\n--- Direct Returning ---");
        lib.ReturnItem("Books", 100); // Should succeed
        lib.ReturnItem("Books", 100); // Should fail (not borrowed)

        // 6. Test Display Available Items
        Console.WriteLine("\n--- Available Items ---");
        lib.DesplayAvelableItem();

        // 7. Test TransactionService (Customer Buy/Borrow/Return)
        Console.WriteLine("\n--- TransactionService: Customer Buy/Borrow/Return ---");
        TransactionService ts = new TransactionService();
        Customer cust = new Customer("Alice", "1112223333", 100m);
        ts.CustomerBuyItem(lib, cust, "Magazines", magA); // Should succeed
        ts.CustomerBuyItem(lib, cust, "Magazines", magA); // Should succeed
        ts.CustomerBuyItem(lib, cust, "Magazines", magA); // Should succeed
        ts.CustomerBuyItem(lib, cust, "Magazines", magA); // Should fail (no more left)

        // Borrowing
        ts.CustomerBorrowItem(lib, cust, "DVDs", dvdA); // Should succeed
        ts.CustomerBorrowItem(lib, cust, "DVDs", dvdA); // Should fail (no more left)

        // Returning
        ts.CustomerReturnItem(lib, cust, "DVDs", 300); // Should succeed
        ts.CustomerReturnItem(lib, cust, "DVDs", 300); // Should fail (not borrowed)

        // 8. Test Insufficient Balance
        Console.WriteLine("\n--- Insufficient Balance ---");
        Customer poor = new Customer("Bob", "9998887777", 1m);
        ts.CustomerBuyItem(lib, poor, "Books", bookA); // Should fail
        ts.CustomerBorrowItem(lib, poor, "Books", bookA); // Should fail

        // 9. Test Invalid Category/ID
        Console.WriteLine("\n--- Invalid Category/ID ---");
        lib.BorrowItem("Nonexistent", 100);
        lib.SellItem("Books", 9999);
        lib.ReturnItem("Magazines", 9999);

        // 10. Test Rollback on Failure
        Console.WriteLine("\n--- Rollback on Failure ---");
        Customer rollback = new Customer("Rollback", "1231231234", 50m);
        Book fakeBook = new Book(9999, "Fake", 10m, "Nobody");
        ts.CustomerBuyItem(lib, rollback, "Books", fakeBook); // Should fail and restore balance
        ts.CustomerBorrowItem(lib, rollback, "Books", fakeBook); // Should fail and restore balance

        // 11. Final Status
        Console.WriteLine("\n--- Final Library Status ---");
        lib.DesplayLibraryStatus();
        Console.WriteLine("\n--- Final Available Items ---");
        lib.DesplayAvelableItem();

        Console.WriteLine("\n=== ALL TESTS COMPLETED ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
}

    
