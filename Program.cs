using System;

namespace PROGRAM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== LIBRARY SYSTEM FULL TEST START ====\n");

            // Create the in-memory inventory
            var inventory = new LibraryInventory();
            var finance = new LibraryFinance();

            // Build the Library (injected with dependencies)
            var library = new Library(
                inventory,   // reader
                inventory,   // writer
                inventory,   // borrowable
                inventory,   // sellable
                finance      // finance
            );

            Console.WriteLine("-- Create item templates --");
            var hpTemplate = new Book(0, "Harry Potter", 30.00m, "J.K. Rowling");
            var hobbitTemplate = new Book(0, "The Hobbit", 25.00m, "J.R.R. Tolkien");
            var timeMag = new Magazine(0, "Time Magazine", 6.50m, 2021);
            var inceptionDvd = new DVD(0, "Inception", 15.00m, 2.5);

            Console.WriteLine("\n-- Add items to inventory --");
            library.AddNewItem("Books", hpTemplate, 3);
            library.AddNewItem("Books", hobbitTemplate, 2);
            library.AddNewItem("Magazines", timeMag, 1);
            library.AddNewItem("DVDs", inceptionDvd, 2);

            Console.WriteLine("\n-- Show initial library status --");
            library.ShowReport();

            Console.WriteLine("\n-- BORROW FLOW: borrow specific copy (by ID) --");
            var borrowReq = new BorrowRequest
            {
                Category = "Books",
                ItemId = 1,
                ItemTitle = "Harry Potter",
                Charge = 3.00m,
                Period = 7,
                UserId = 1001
            };
            library.ExecTransaction(OperationType.Borrow, borrowReq);

            Console.WriteLine("\n-- SELL FLOW: sell Hobbit copy (id 3) --");
            var sellReq = new SellRequest
            {
                Category = "Books",
                ItemId = 3,
                ItemTitle = "The Hobbit",
                Charge = 25.00m,
                UserId = 2001
            };
            library.ExecTransaction(OperationType.Sell, sellReq);

            Console.WriteLine("\n-- RETURN FLOW: return borrowed Harry Potter (id 1) --");
            var returnReq = new ReturnRequest
            {
                Category = "Books",
                ItemId = 1
            };
            library.ExecTransaction(OperationType.Return, returnReq);

            Console.WriteLine("\n-- RESTOCK FLOW: restock magazine (id 5) with 2 more copies --");
            library.RestockItem("Magazines", 5, 2);

            Console.WriteLine("\n-- Show final library status --");
            library.ShowReport();

            Console.WriteLine("\n==== LIBRARY SYSTEM FULL TEST FINISHED ====");
        }
    }
}
