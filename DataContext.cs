using ORM.Contracts;
using ORM.Db;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ORM
{
    public class ToUpdateCache
    {
        Dictionary<string, Book> _updateCache = new Dictionary<string, Book>();
        public void CreateItem(Book book)
        {
            
            if (!_updateCache.ContainsKey(book.Id))
            {
                _updateCache[book.Id] = book;
                
            }
        }
        public Dictionary<string, Book> GetItems()
        {
            return _updateCache;
        }
        public Book GetItem(string key)
        {
            return _updateCache[key];
        }
        public bool ItemContains(string key)
        {
            return _updateCache.ContainsKey(key);
        }
    }

    public class ToInsertCache
    {
        Dictionary<string, Book> _insertCache = new Dictionary<string, Book>();
        public void CreateItem(Book book)
        {

            if (!_insertCache.ContainsKey(book.Id))
            {
                _insertCache[book.Id] = book;

            }
        }
        public void Clear(string key)
        {
            _insertCache.Remove(key);
            //_insertCache.Clear();
        }
        public Dictionary<string, Book> GetItems()
        {
            return _insertCache;
        }

        public Book GetItem(string key)
        {
            return _insertCache[key];
        }
        public bool ItemContains(string key)
        {
            return _insertCache.ContainsKey(key);
        }
    }

    public class DataContext : IDataContext
    {
        ToInsertCache _insCache = new ToInsertCache();
        ToUpdateCache _updCache = new ToUpdateCache();
        private readonly IDbEngine dbEngine;
        
        
        public DataContext(IDbEngine dbEngine)
        {
            this.dbEngine = dbEngine;
        }
        
        public Book Find(string id)
        {
            if (_updCache.ItemContains(id))
            {
                return _updCache.GetItem(id);
            }
            else
            {
                string answer = dbEngine.Execute($"get Id={id};");
                if (answer == ";")
                    return null;
                else
                {
                    Book book = ParseString(answer);
                    _updCache.CreateItem(book);
                    return book;
                }
            } 
        }

        public Book Read(string id)
        {
            if (_updCache.ItemContains(id))
            {
                return _updCache.GetItem(id);
            }
            else
            {
                string answ = dbEngine.Execute($"get Id={id};");
                if (answ == ";")
                {
                    throw new Exception();

                }
                else
                {
                    Book book = ParseString(answ);
                    _updCache.CreateItem(book);
                    return book;
                }
            } 
        }

        public void Insert(Book entity)
        {
            if (entity is null)
                throw new Exception();
            else
                _insCache.CreateItem(entity);
        }

        public void SubmitChanges()
        {
            string req = "";
            foreach (var book in _insCache.GetItems().Values)
            {
                req += CollectStringAdd(book);
                
                
            }
            foreach (var book in _updCache.GetItems().Values)
            {
                req += CollectStringUpd(book);
            }
             string answ = dbEngine.Execute(req);

           
        

            if (answ.Contains("ok"))
                {
                
            }
            else throw new Exception();
           
            foreach(var book in _insCache.GetItems().Values)
            {
                _updCache.CreateItem(book);
                
            }

            
            
        }

        public void testmethod()
        {
            string str = @"add Id=2;upd Id=2,F1=\=\;\,;get Id=2;";
            string ssssss = dbEngine.Execute(str);
            string s = ssssss.Replace("\\\\","\\");
            string ssss = dbEngine.Execute("get Id=2;");
            Console.WriteLine(ssssss + "\t" + s);
            string strIN = str.Replace("\\\\", "\\");
            //char[] chars = str.ToCharArray();
            //for(int i=0;i<chars.Length;i++)
            //{
            //    if (chars[i].Equals(";") && chars[i+1].Equals(";"))
            //    {
            //        str = str.tr
            //    }
            //}
        }
        public string CollectStringAdd(Book book)
        {
            string str = "add Id=000243EC\\,Title=The Warp; in, the West,Price=25,Weight=1,Author=Ulvius Tero,Skill=Block;";
            str = str.Replace(@"\", @"\\");
            str = str.Replace(@";", @"\;");
            str = str.Replace(@",", @"\,");
            return $"add Id={book.Id},Title={book.Title},Price={book.Price.ToString()},Weight={book.Weight.ToString()},Author={book.Author},Skill={book.Skill};";
        }
        public string CollectStringUpd(Book book)
        {

            return $"upd Id={book.Id},Title={book.Title},Price={book.Price.ToString()},Weight={book.Weight.ToString()},Author={book.Author},Skill={book.Skill};";
        }
        public string[] ParseAnswer(string answ)
        {
            return answ.Split(';');
        }

        public Book ParseString(string answ)
        {
            
            answ = answ.Trim(';');
            string[] pairs = answ.Split(',');
            Dictionary<string, string> dict = pairs.Select(s => s.Split('=')).ToDictionary(arr => arr[0], arr => arr[1]);

            Book book = new Book();
            
            if (dict.ContainsKey("Id"))
                book.Id = dict["Id"];
            if (dict.ContainsKey("Title"))
                book.Title = dict["Title"];
            if (dict.ContainsKey("Price"))
                book.Price = Convert.ToInt32(dict["Price"]);
            if (dict.ContainsKey("Weight"))
                book.Weight = Convert.ToDecimal(dict["Weight"]);
            if (dict.ContainsKey("Author"))
                book.Author = dict["Author"];
            if (dict.ContainsKey("Skill"))
                book.Skill = dict["Skill"];



            return book;
        }
    }
}