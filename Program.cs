﻿using System;
using ORM.Contracts;
using ORM.Db;

namespace ORM
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var dbEngine = new DbEngine();
            dbEngine.Execute("add Id=000243DE,Title=The Ransom of Zarek,Price=35,Weight=1,Author=Marobar Sul,Skill=Athletics;");
            dbEngine.Execute("add Id=000243EC,Title=The Warp in the West,Price=25,Weight=1,Author=Ulvius Tero,Skill=Block;");

            var dataContext = new DataContext(dbEngine);
            //dataContext.testmethod();
            var checkDataContext = new DataContext(dbEngine);

            var book = dataContext.Read<Book>("000243DE");
            Console.WriteLine($"The Author of '{book.Title}' is '{book.Author}'");

            book.Author = "Gor Felim";
            dataContext.SubmitChanges();
            Console.WriteLine("Submitting changes...");

            var actualBook = checkDataContext.Read<Book>("000243DE");
            Console.WriteLine($"The Author of '{actualBook.Title}' is '{actualBook.Author}'");
        }
    }
}