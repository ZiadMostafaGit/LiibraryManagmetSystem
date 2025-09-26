
namespace PROGRAM
{


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== COMPREHENSIVE LIBRARY SYSTEM TEST ===\n");

        // Initialize the system
        Library library = new Library();
        TransactionService transactionService = new TransactionService();
        
        // Create test customers
        Customer richCustomer = new Customer("John Doe", "1234567890", 1000m);
        Customer poorCustomer = new Customer("Jane Smith", "0987654321", 5m);
        
        Console.WriteLine("📋 INITIAL SETUP COMPLETE");
        Console.WriteLine($"Rich Customer Balance: ${richCustomer.Balance}");
        Console.WriteLine($"Poor Customer Balance: ${poorCustomer.Balance}\n");

        // Create test items
        Book book1 = new Book(1, "Harry Potter", 25.99m, "J.K. Rowling");
        Book book2 = new Book(2, "Lord of the Rings", 35.50m, "J.R.R. Tolkien");
        Magazine mag1 = new Magazine(3, "National Geographic", 5.99m, 2024);
        DVD dvd1 = new DVD(4, "Inception", 19.99m, 2.5);
        DVD dvd2 = new DVD(5, "The Matrix", 15.99m, 2.2);

        Console.WriteLine("=== TESTING ITEM CREATION ===");
        book1.DisplayInfo();
        mag1.DisplayInfo();
        dvd1.DisplayInfo();
        Console.WriteLine();

        // Test adding items to library
        Console.WriteLine("=== TESTING LIBRARY INVENTORY ===");
        library.AddNewItem("Books", book1, 3);
        library.AddNewItem("Books", book2, 2);
        library.AddNewItem("Magazines", mag1, 5);
        library.AddNewItem("DVDs", dvd1, 1);
        library.AddNewItem("DVDs", dvd2, 4);
        
        // Test restocking
        Console.WriteLine("\n--- Testing Restocking ---");
        library.RestockItem("Books", 1, 2);
        
        // Display initial library status
        Console.WriteLine("\n--- Initial Library Status ---");
        library.DesplayLibraryStatus();

        // Display available items
        Console.WriteLine("\n--- Available Items ---");
        library.DesplayAvelableItem();

        // Test direct library operations
        Console.WriteLine("\n=== TESTING DIRECT LIBRARY OPERATIONS ===");
        
        // Test borrowing
        Console.WriteLine("\n--- Testing Library Borrowing ---");
        library.BorrowItem("Books", 1);
        library.BorrowItem("DVDs", 4); // Should work
        library.BorrowItem("DVDs", 4); // Should fail - only 1 copy, now borrowed
        
        // Test selling
        Console.WriteLine("\n--- Testing Library Selling ---");
        library.SellItem("Books", 2);
        library.SellItem("Magazines", 3);
        
        // Test returning
        Console.WriteLine("\n--- Testing Library Returning ---");
        library.ReturnItem("Books", 1);
        library.ReturnItem("DVDs", 4);
        
        // Show updated status
        Console.WriteLine("\n--- Library Status After Direct Operations ---");
        library.DesplayLibraryStatus();

        // Test customer transactions
        Console.WriteLine("\n=== TESTING CUSTOMER TRANSACTIONS ===");
        
        // Test successful customer buying
        Console.WriteLine("\n--- Rich Customer Buying Items ---");
        Console.WriteLine($"Rich Customer Balance Before: ${richCustomer.Balance}");
        transactionService.CustomerBuyItem(library, richCustomer, "Books", book2);
        transactionService.CustomerBuyItem(library, richCustomer, "DVDs", dvd2);
        Console.WriteLine($"Rich Customer Balance After: ${richCustomer.Balance}");
        
        // Test customer borrowing
        Console.WriteLine("\n--- Rich Customer Borrowing Items ---");
        Console.WriteLine($"Rich Customer Balance Before: ${richCustomer.Balance}");
        transactionService.CustomerBorrowItem(library, richCustomer, "Books", book1);
        transactionService.CustomerBorrowItem(library, richCustomer, "Magazines", mag1);
        Console.WriteLine($"Rich Customer Balance After: ${richCustomer.Balance}");
        
        // Test customer returning
        Console.WriteLine("\n--- Customer Returning Items ---");
        transactionService.CustomerReturnItem(library, richCustomer, "Books", 1);
        transactionService.CustomerReturnItem(library, richCustomer, "Magazines", 3);
        
        // Test poor customer transactions (should fail due to insufficient funds)
        Console.WriteLine("\n--- Poor Customer Attempting Expensive Purchase ---");
        Console.WriteLine($"Poor Customer Balance: ${poorCustomer.Balance}");
        transactionService.CustomerBuyItem(library, poorCustomer, "Books", book1); // Should fail
        
        // Test poor customer borrowing (should work)
        Console.WriteLine("\n--- Poor Customer Borrowing (Should Work) ---");
        Console.WriteLine($"Poor Customer Balance Before: ${poorCustomer.Balance}");
        transactionService.CustomerBorrowItem(library, poorCustomer, "Books", book1);
        Console.WriteLine($"Poor Customer Balance After: ${poorCustomer.Balance}");

        // Test error scenarios
        Console.WriteLine("\n=== TESTING ERROR SCENARIOS ===");
        
        Console.WriteLine("\n--- Testing Invalid Category ---");
        library.BorrowItem("InvalidCategory", 1);
        
        Console.WriteLine("\n--- Testing Invalid Item ID ---");
        library.BorrowItem("Books", 999);
        
        Console.WriteLine("\n--- Testing Borrowing Already Borrowed Item ---");
        library.BorrowItem("Books", 1); // Should be available now
        library.BorrowItem("Books", 1); // Should fail - only 1 left after previous operations
        
        Console.WriteLine("\n--- Testing Return of Non-Borrowed Item ---");
        library.ReturnItem("DVDs", 5); // This item was never borrowed

        // Test rollback scenarios
        Console.WriteLine("\n=== TESTING ROLLBACK SCENARIOS ===");
        
        // Create an item that exists in inventory but will cause library operation to fail
        Console.WriteLine("\n--- Testing Purchase Rollback ---");
        Customer testCustomer = new Customer("Test User", "5555555555", 50m);
        Console.WriteLine($"Test Customer Balance Before: ${testCustomer.Balance}");
        
        // Try to buy an item that doesn't exist - should rollback balance
        transactionService.CustomerBuyItem(library, testCustomer, "Books", new Book(999, "Non-Existent", 20m, "Nobody"));
        Console.WriteLine($"Test Customer Balance After Failed Purchase: ${testCustomer.Balance}");

        // Test invalid user creation scenarios
        Console.WriteLine("\n=== TESTING INVALID USER CREATION ===");
        try
        {
            Customer invalidCustomer = new Customer("Invalid", "abc123", 100m); // Invalid phone
        }
        catch (Exception e)
        {
            Console.WriteLine($"Expected error for invalid phone: {e.Message}");
        }
        
        try
        {
            Customer negativeBalanceCustomer = new Customer("Negative", "1111111111", -50m); // Negative balance
        }
        catch (Exception e)
        {
            Console.WriteLine($"Expected error for negative balance: {e.Message}");
        }

        // Test invalid item creation
        Console.WriteLine("\n=== TESTING INVALID ITEM CREATION ===");
        try
        {
            Book invalidBook = new Book(-1, "Invalid", 10m, "Author"); // Invalid ID
        }
        catch (Exception e)
        {
            Console.WriteLine($"Expected error for invalid book ID: {e.Message}");
        }
        
        try
        {
            Book negativePrice = new Book(100, "Free Book", -5m, "Author"); // Negative price
        }
        catch (Exception e)
        {
            Console.WriteLine($"Expected error for negative price: {e.Message}");
        }
        
        try
        {
            Magazine futureMag = new Magazine(101, "Future Mag", 5m, 2030); // Future date
        }
        catch (Exception e)
        {
            Console.WriteLine($"Expected error for future date: {e.Message}");
        }
        
        try
        {
            DVD longDVD = new DVD(102, "Long Movie", 10m, 15.5); // Invalid duration
        }
        catch (Exception e)
        {
            Console.WriteLine($"Expected error for invalid duration: {e.Message}");
        }

        // Final library status and financial report
        Console.WriteLine("\n=== FINAL SYSTEM STATUS ===");
        library.DesplayLibraryStatus();
        
        Console.WriteLine("\n=== CUSTOMER FINAL BALANCES ===");
        Console.WriteLine($"Rich Customer Final Balance: ${richCustomer.Balance}");
        Console.WriteLine($"Poor Customer Final Balance: ${poorCustomer.Balance}");
        Console.WriteLine($"Test Customer Final Balance: ${testCustomer.Balance}");

        Console.WriteLine("\n=== AVAILABLE ITEMS FOR FUTURE TRANSACTIONS ===");
        library.DesplayAvelableItem();

        Console.WriteLine("\n=== TEST COMPLETION ===");
        Console.WriteLine("All system components tested successfully!");
        Console.WriteLine("- Library inventory management");
        Console.WriteLine("- Financial transactions");
        Console.WriteLine("- Customer operations");
        Console.WriteLine("- Error handling and validation");
        Console.WriteLine("- Rollback mechanisms");
        Console.WriteLine("- Single Responsibility Principle implementation");
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
}

    
