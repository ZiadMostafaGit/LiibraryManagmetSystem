
namespace PROGRAM
{


    class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== LIBRARY SYSTEM TEST START ===\n");

            var library = new Library();
            var tx = new TransactionService();

            // Create valid items
            Console.WriteLine("Creating items...");
            var book1 = new Book(1, "The Legend of Zelda", 10.50m, "Shigeru Miyamoto");
            var book2 = new Book(2, "C# In Depth", 35.00m, "Jon Skeet");
            var mag1 = new Magazine(10, "Time", 5.00m, 2020);
            var dvd1 = new DVD(100, "The Matrix", 7.50m, 2.5);

            // Add inventory
            Console.WriteLine("\nAdding inventory...");
            library.AddItem("Books", book1, 3);      // 3 copies
            library.AddItem("Books", book2, 1);      // 1 copy
            library.AddItem("Magazines", mag1, 2);   // 2 copies
            library.AddItem("DVDs", dvd1, 2);        // 2 copies

            // Display status
            Console.WriteLine();
            library.DisplayAvailableItems();
            library.DisplayLibraryStatus();

            // Create customers
            Console.WriteLine("\nCreating customers...");
            var alice = new Customer("Alice", "0123456789", 50.00m);
            var bob = new Customer("Bob", "9876543210", 5.00m); // low balance

            // Happy path: Alice borrows a book
            Console.WriteLine("\n-- Alice borrows a book (book1) --");
            tx.CustomerBorrowItem(library, alice, "Books", book1); // should succeed
            library.DisplayAvailableItems();

            // Borrow same book until it's out, then show behavior
            Console.WriteLine("\n-- Borrowing the same book until quantity runs out --");
            tx.CustomerBorrowItem(library, alice, "Books", book1); // second copy
            tx.CustomerBorrowItem(library, alice, "Books", book1); // third copy -> after this quantity becomes 0 and state Borrowed
                                                                   // Next borrow should fail (book is borrowed / out)
            Console.WriteLine("\nAttempt one more borrow (should fail) — handled by TransactionService:");
            tx.CustomerBorrowItem(library, alice, "Books", book1);

            // Return the book (should succeed)
            Console.WriteLine("\n-- Returning a book (book1) --");
            tx.CustomerReturnItem(library, alice, "Books", book1.ID);
            library.DisplayAvailableItems();

            // Alice buys a DVD
            Console.WriteLine("\n-- Alice buys a DVD (dvd1) --");
            tx.CustomerBuyItem(library, alice, "DVDs", dvd1);
            library.DisplayLibraryStatus();

            // Bob tries to buy an expensive book (insufficient balance)
            Console.WriteLine("\n-- Bob tries to buy an expensive book (should fail) --");
            tx.CustomerBuyItem(library, bob, "Books", book2); // bob balance 5 < 35 -> failure

            // Try invalid category
            Console.WriteLine("\n-- Try buying from an invalid category (should fail) --");
            tx.CustomerBuyItem(library, alice, "Comics", book1);

            // Try invalid id
            Console.WriteLine("\n-- Try borrowing with invalid item id (should fail) --");
            tx.CustomerBorrowItem(library, alice, "Books", new Book(9999, "Ghost", 1.0m, "No One")); // id 9999 not in library

            // Test RESTOCK behavior
            Console.WriteLine("\n-- Restocking book1 by 2 copies --");
            library.RestockItem("Books", book1.ID, 2);
            library.DisplayAvailableItems();

            // Test Sell until sold out
            Console.WriteLine("\n-- Selling all copies of book2 (only 1 copy) --");
            tx.CustomerBuyItem(library, alice, "Books", book2); // should sell the single copy
            Console.WriteLine("Attempt to buy book2 again (should fail)");
            tx.CustomerBuyItem(library, alice, "Books", book2);

            // Edge-case constructor failures (invalid data)
            Console.WriteLine("\n-- Constructor error tests (invalid values) --");
            try
            {
                var badBook = new Book(-5, "Bad", 10m, "Author"); // invalid id
            }
            catch (InvalidIdException ex)
            {
                Console.WriteLine($"Caught expected InvalidIdException: {ex.Message}");
            }

            try
            {
                var badPrice = new Book(20, "BadPrice", -1m, "Author"); // invalid price
            }
            catch (InvalidPriceException ex)
            {
                Console.WriteLine($"Caught expected InvalidPriceException: {ex.Message}");
            }

            try
            {
                var badMag = new Magazine(21, "FutureMag", 1m, 3000); // invalid publish date -> throws InvalidDataException
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Caught expected InvalidDataException (publish date): {ex.Message}");
            }

            try
            {
                var badDVD = new DVD(30, "TooLongMovie", 3m, 20.0); // if > allowed limit - you used 10 max in class, so it may throw
                Console.WriteLine("Created badDVD (unexpected): " + badDVD.Title);
            }
            catch (InvlaidDurationException ex)
            {
                Console.WriteLine($"Caught expected InvlaidDurationException: {ex.Message}");
            }

            // Test invalid user creation
            Console.WriteLine("\n-- Invalid user creation tests --");
            try
            {
                var badUser = new Customer("Eve", "notdigits", 10m); // invalid phone should throw
            }
            catch (InvalidPhoneExeption ex)
            {
                Console.WriteLine($"Caught expected InvalidPhoneExeption: {ex.Message}");
            }

            // Final library status
            Console.WriteLine("\nFinal library state:");
            library.DisplayLibraryStatus();

            Console.WriteLine("\n=== LIBRARY SYSTEM TEST END ===");
        }
    }
}

    
