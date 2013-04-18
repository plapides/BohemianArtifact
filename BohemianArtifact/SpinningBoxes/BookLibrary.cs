using System;
using System.Collections;
using System.IO;

namespace BohemianBookshelf
{
    public class Book
    {
        private string author;
        private string title;

        public string Author
        {
            get
            {
                return author;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
        }

        public Book(string line)
        {
        }
    }

    public class BookLibrary
    {
        private ArrayList books;
        private Book selectedBook;

        public delegate void SelectedBookHandler(Book selectedBook);
        public event SelectedBookHandler SelectedBookChanged;

        public Book SelectedBook
        {
            get
            {
                return selectedBook;
            }
        }

        public BookLibrary(string filename)
        {
            books = new ArrayList();
            StreamReader file = new StreamReader(filename);
            string line = "";
            using (file)
            {
                while (true)
                {
                    line = file.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    Book newBook = new Book(line);
                    books.Add(newBook);
                }
            }
        }

        public void SelectBook(Book sBook)
        {
            if (books.Contains(sBook) == true)
            {
                selectedBook = sBook;
                SelectedBookChanged(selectedBook);
            }
        }
    }
}
