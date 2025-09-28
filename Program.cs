using System;

namespace PROGRAM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== LIBRARY SYSTEM FULL TEST START ====\n");

            // Create library and helper services
            var library = new Library();
            var txService = new TransactionService();

            Console.WriteLine("-- Create item templates (these are templates; inventory clones them) --");
            var hpTemplate = new Book(0, "Harry Potter", 30.00m, "J.K. Rowling");
            var hobbitTemplate = new Book(0, "The Hobbit", 25.00m, "J.R.R. Tolkien");
            var timeMag = new Magazine(0, "Time Magazine", 6.50m, 2021);
            var inceptionDvd = new DVD(0, "Inception", 15.00m, 2.5);

            Console.WriteLine("\n-- Add items to inventory --");
            // Add copies (IDs will be assigned by inventory in insertion order: 0,1,2,...)
            library.AddNewItem("Books", hpTemplate, 3);      // HP copies -> expected ids: 0,1,2
            library.AddNewItem("Books", hobbitTemplate, 2);  // Hobbit copies -> expected ids: 3,4
            library.AddNewItem("Magazines", timeMag, 1);     // Magazine -> expected id: 5
            library.AddNewItem("DVDs", inceptionDvd, 2);     // DVDs -> expected ids: 6,7

            Console.WriteLine("\n-- Show initial library status --");
            library.DesplayLibraryStatus();

            Console.WriteLine("\n-- BORROW FLOW: borrow specific copy (by ID) --");
            try
            {
                Console.WriteLine("Borrowing book with ID = 1 (Harry Potter copy #2)...");
                library.BorrowItem("Books", 1); // should mark item 1 as Borrowed
                library.GetItemQuantity("Books", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while borrowing by id: " + ex.Message);
            }

            Console.WriteLine("\n-- BORROW FLOW: borrow first available by title via ExecTransaction (BorrowRequest without Id) --");
            try
            {
                var borrowByTitle = new BorrowRequest
                {
                    Category = "Books",
                    ItemTitle = "Harry Potter",
                    ItemId = -1,     // sentinel to indicate "no specific copy, pick first available"
                    Charge = 3.00m,
                    Period = 7,
                    UserId = 1001
                };
                library.ExecTransaction(OperationType.Borrow, borrowByTitle);
                // We borrowed one more HP (first available)
            }
            catch (Exception ex)
            {
                Console.WriteLine("Borrow by title failed: " + ex.Message);
            }

            Console.WriteLine("\n-- BORROW FLOW: borrow the last copy (by ID) --");
            try
            {
                Console.WriteLine("Borrowing book with ID = 2 (last HP copy)...");
                library.BorrowItem("Books", 2);
                library.GetItemQuantity("Books", 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while borrowing last copy: " + ex.Message);
            }

            Console.WriteLine("\n-- Attempt to borrow Harry Potter again (should fail because all copies borrowed) --");
            try
            {
                var brFail = new BorrowRequest
                {
                    Category = "Books",
                    ItemTitle = "Harry Potter",
                    ItemId = -1,
                    Charge = 3.00m,
                    Period = 5,
                    UserId = 1002
                };
                library.ExecTransaction(OperationType.Borrow, brFail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected failure: " + ex.Message);
            }

            Console.WriteLine("\n-- SELL FLOW: sell specific Hobbit copy (id 3) via ExecTransaction --");
            try
            {
                var sellReq = new SellRequest
                {
                    Category = "Books",
                    ItemTitle = "The Hobbit",
                    ItemId = 3,     // sell the first Hobbit copy
                    Charge = 25.00m,
                    UserId = 2001
                };
                library.ExecTransaction(OperationType.Sell, sellReq);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sell failed: " + ex.Message);
            }

            Console.WriteLine("\n-- Sell another Hobbit copy directly via Library.SellItem (id 4) --");
            try
            {
                library.SellItem("Books", 4);
                Console.WriteLine("Sold Hobbit id 4 successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Direct sell failed: " + ex.Message);
            }

            Console.WriteLine("\n-- Attempt to sell a Hobbit again (should fail because all were sold) --");
            try
            {
                // there is no more Hobbit copy in catalog, selling id 3 or 4 again will throw
                library.SellItem("Books", 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected selling failure: " + ex.Message);
            }

            Console.WriteLine("\n-- RETURN FLOW: return one borrowed Harry Potter (id 1) --");
            try
            {
                var returnReq = new ReturnRequest
                {
                    Category = "Books",
                    ItemId = 1
                };
                library.ExecTransaction(OperationType.Return, returnReq);
                library.GetItemQuantity("Books", 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Return failed: " + ex.Message);
            }

            Console.WriteLine("\n-- Attempt to return an item that isn't borrowed (id 3 which is sold) --");
            try
            {
                var badReturn = new ReturnRequest { Category = "Books", ItemId = 3 };
                library.ExecTransaction(OperationType.Return, badReturn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected failure on return: " + ex.Message);
            }

            Console.WriteLine("\n-- RESTOCK FLOW: restock magazine (id 5) with 2 more copies --");
            try
            {
                library.RestockItem("Magazines", 5, 2); // new magazine item IDs should be appended to items list
                Console.WriteLine("Restocked magazine id 5 with 2 copies.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Restock failed: " + ex.Message);
            }

            Console.WriteLine("\n-- Display status after borrows/sells/restock --");
            library.DesplayLibraryStatus();

            Console.WriteLine("\n-- TRANSACTION SERVICE: Customer buys a DVD (use id = 6) --");
            var alice = new Customer("Alice", "01012345678", 100.00m);
            try
            {
                // We'll craft a small Item-like object with matching ID and price for transaction service.
                // TransactionService uses only item.ID and item.Price of the passed item.
                var dvdForSale = new DVD(6, "Inception", 15.00m, 2.5); // id 6 corresponds to an inventory DVD copy
                bool bought = txService.CustomerBuyItem(library, alice, "DVDs", dvdForSale);
                Console.WriteLine($"Alice balance after buy: {alice.Balance}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Customer buy failed: " + ex.Message);
            }

            Console.WriteLine("\n-- TRANSACTION SERVICE: Customer borrows a DVD (id 7) --");
            var bob = new Customer("Bob", "01098765432", 5.00m); // small balance to test fee edge
            try
            {
                var dvdToBorrow = new DVD(7, "Inception", 15.00m, 2.5);
                bool borrowed = txService.CustomerBorrowItem(library, bob, "DVDs", dvdToBorrow);
                Console.WriteLine($"Bob balance after borrow attempt: {bob.Balance}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Customer borrow failed: " + ex.Message);
            }

            Console.WriteLine("\n-- Try borrow same DVD id 7 again (should fail or already borrowed) --");
            try
            {
                library.BorrowItem("DVDs", 7);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected failure: " + ex.Message);
            }

            Console.WriteLine("\n-- Check item states for a few IDs --");
            library.GetItemQuantity("Books", 0);
            library.GetItemQuantity("Books", 1);
            library.GetItemQuantity("Books", 2);
            library.GetItemQuantity("Books", 3); // sold
            library.GetItemQuantity("Magazines", 5);
            library.GetItemQuantity("DVDs", 6);
            library.GetItemQuantity("DVDs", 7);

            Console.WriteLine("\n-- Final library report --");
            library.DesplayLibraryStatus();

            Console.WriteLine("\n-- Display available items only --");
            library.DesplayAvelableItem();

            Console.WriteLine("\n-- Edge case tests: invalid category / id --");
            try
            {
                library.BorrowItem("NonExistentCategory", 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected invalid category error: " + ex.Message);
            }

            try
            {
                library.SellItem("Books", 9999);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Expected invalid id error: " + ex.Message);
            }

            Console.WriteLine("\n==== LIBRARY SYSTEM FULL TEST FINISHED ====");
        }
    }
}
