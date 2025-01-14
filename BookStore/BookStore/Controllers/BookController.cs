﻿using BookStore.Models;
using BookStore.Models.Repositories;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class BookController : Controller

    {
        private readonly IBookstoreRepository<Book> bookRepository;
        private readonly IBookstoreRepository<Author> authorRepository;
        [Obsolete]
        private readonly IHostingEnvironment hosting;

        [Obsolete]
        public BookController(IBookstoreRepository<Book> bookRepository,
            IBookstoreRepository<Author> authorRepository,
            IHostingEnvironment hosting)
        {
            this.bookRepository = bookRepository;
            this.authorRepository = authorRepository;
            this.hosting = hosting;
        }
        // GET: BookController
        public ActionResult Index()
        {
            var books = bookRepository.List();
            return View(books);
        }

        // GET: BookController/Details/5
        public ActionResult Details(int id)
        {
            var book = bookRepository.Find(id);
            return View(book);
        }

        // GET: BookController/Create
        public ActionResult Create()
        {
            var model = new BookAuthorViewModel()
            {
                Authors = FillSelectList()
            };
            return View(model);
        }

        // POST: BookController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookAuthorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName =UploadFile(model.File) ??  string.Empty;
                    
                    if (model.AuthorId == -1)
                    {
                        ViewBag.message = "please selecte an author from the list!";

                        return View(GetAllAuthors());
                    }
                    var author = authorRepository.Find(model.AuthorId);
                    Book book = new Book
                    {
                        Id = model.BookId,
                        Title = model.Title,
                        Description = model.Description,
                        author = author,
                        ImageUrl = fileName

                    };
                    bookRepository.Add(book);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
            ModelState.AddModelError("", "You have to fill all the requied fields!");
            return View(GetAllAuthors());
        }

        // GET: BookController/Edit/5
        public ActionResult Edit(int id)
        {
            var book = bookRepository.Find(id);
            //var authorId = book.author == null ? book.author.Id = 0 : book.author.Id;
            var viewmodel = new BookAuthorViewModel
            {
                BookId = book.Id,
                Title = book.Title,
                Description = book.Description,
                //AuthorId = authorId,
                Authors = authorRepository.List().ToList(),
                ImageUrl = book.ImageUrl

            };
            return View(viewmodel);
        }

        // POST: BookController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookAuthorViewModel viewModel)
        {
            try
            {
                string fileName =UploadFile(viewModel.File,viewModel.ImageUrl);

                

                var author = authorRepository.Find(viewModel.AuthorId);
                Book book = new Book
                {
                    Id=viewModel.BookId,
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    author = author,
                    ImageUrl = fileName

                };
                bookRepository.Update(viewModel.BookId, book);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookController/Delete/5
        public ActionResult Delete(int id)
        {
            var book = bookRepository.Find(id);

            return View(book);
        }

        // POST: BookController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int id)
        { 
            BookstoreDbcontext dbcontext = new BookstoreDbcontext();
            try
            {
                bookRepository.Delete(id);
                dbcontext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        List<Author> FillSelectList()
        {
            var authors = authorRepository.List().ToList();
            authors.Insert(0, new Author { Id = -1, FullName = "---Please Select an Author--- " });
            return authors;
        }
        BookAuthorViewModel GetAllAuthors()
        {
            var vmodel = new BookAuthorViewModel()
            {
                Authors = FillSelectList()
            };
            return vmodel;
        }

        string UploadFile(IFormFile file) 
        {
            if (file != null)
            {
                string uploads = Path.Combine(hosting.WebRootPath, "Uploads");
                string fullPath = Path.Combine(uploads, file.FileName);
                file.CopyTo(new FileStream(fullPath, FileMode.Create));

                return file.FileName;
            }

            return null;
        }


        string UploadFile(IFormFile file, string imageUrl)
        {
            if (file != null)
            {
                string uploads = Path.Combine(hosting.WebRootPath, "Uploads");

                string newPath = Path.Combine(uploads, file.FileName);

                //Delete the old file

                string oldPath = Path.Combine(uploads, imageUrl);

                if (oldPath != newPath)
                {
                    System.IO.File.Delete(oldPath);
                    //Save the new file
                    file.CopyTo(new FileStream(newPath, FileMode.Create));
                }
                return file.FileName;
            }
            return imageUrl;
        }
    }

}
