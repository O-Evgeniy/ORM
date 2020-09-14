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

            Console.WriteLine("FIND BOOK ID:" + id);
            if (_updCache.ItemContains(id))
            {
                return _updCache.GetItem(id);
            }
            else
            {
                string answer = dbEngine.Execute($"get Id={id};");
                Console.WriteLine("FIND ANSWER:" + answer);
                answer = DeleteScreening(answer);
                Console.WriteLine("FIND ANSWER after deleteSCreen:"+ answer);
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
                    answ = DeleteScreening(answ);
                    //Console.WriteLine(answ);
                    Book book = ParseString(answ);
                    _updCache.CreateItem(book);
                    return book;
                }
            } 
        }

        public void Insert(Book entity)
        {
           // Console.WriteLine("Insert BOOK:" + entity.Id);
            if (entity is null)
                throw new Exception();
            else
            {
                Console.WriteLine("Insert BOOK: " + $"Id={entity.Id},Title={entity.Title},Price={entity.Price.ToString()},Weight={entity.Weight.ToString()},Author={entity.Author},Skill={entity.Skill};");
                _insCache.CreateItem(entity);
            }
        }

        public void SubmitChanges()
        {
            //Console.WriteLine("SUbmitCHange REQ");
            string req = "";
            foreach (var book in _insCache.GetItems().Values)
            {
                //req += CollectStringAdd(book);
                req += CollectStringAdd(AddScreening(book));


            }
            foreach (var book in _updCache.GetItems().Values)
            {
                req += CollectStringUpd(AddScreening(book));
            }
             string answ = dbEngine.Execute(req);
            Console.WriteLine("SubmitChange REQ: " + req);
           
        

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
            string str = @"add Id=000243DE,Author=Marobar Sul,Price=35,Skill=Athletics,Title=The Ransom of Zarek,Weight=1;";
            string answe = dbEngine.Execute(str);
            str = @"get Id=000243DE;";
            string ssssss = dbEngine.Execute(str);
            //Book book = ParseString(ssssss);
            string answ = DeleteScreening(ssssss);
            Book book = ParseString(answ);
            Insert(book);
            //char[] chars = str.ToCharArray();
            //for(int i=0;i<chars.Length;i++)
            //{
            //    if (chars[i].Equals("; ") && chars[i+1].Equals(";"))
            //    {
            //        str = str.tr
            //    }
            //}
        }
        public string CollectStringAdd(Book book)
        {
            
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
            var result = Regex.Matches(answ, @"(?<key>\w*)\,(?<value>\w*)")
              .OfType<Match>()
              .ToDictionary(match => match.Groups["key"], match => match.Groups["value"]);

            answ = answ.Trim(';');
            string[] pairs = answ.Split(',');
            Dictionary<string, string> dict = pairs.Select(s => s.Split('=')).ToDictionary(arr => arr[0].Replace("%", "\\").Replace("\\&", "=").Replace("\\-", ","), arr => arr[1].Replace("%", "\\").Replace("\\&", "=").Replace("\\-", ","));
            string str="" ;
            foreach(var i in dict)
            {
                str += $"{i.Key}={i.Value},";
            }
            Console.WriteLine(str);

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

        public string DeleteScreening(string answ)
        {
            string answWithoutScreening;
            answ = answ.Replace("\\;",";");
            answ = answ.Replace("\\\\", "%");
            answ = answ.Replace("\\,", "\\-");
            answ = answ.Replace(@"\", "\\");
            answWithoutScreening = answ.Replace("\\=", "\\&");

            return answWithoutScreening;
        }

        public Book AddScreening(Book book)
        {
            Console.WriteLine(book.Author);
            Book tempBook = new Book();
            tempBook.Id = book.Id.Replace(";", "\\;").Replace(",", "\\,").Replace("=", "\\=").Replace(@"\\", "\\").Replace("\\=","\\\\=");
            tempBook.Price=Convert.ToInt32(book.Price.ToString().Replace(";", "\\;").Replace(",", "\\,").Replace("=", "\\=").Replace(@"\\", "\\").Replace("\\=", "\\\\="));
            tempBook.Skill=book.Skill.Replace(";", "\\;").Replace(",", "\\,").Replace("=", "\\=").Replace(@"\\", "\\").Replace("\\=", "\\\\=");
            tempBook.Title = book.Title.Replace("\\=", @"\\\\=").Replace("=", "\\=").Replace("\\;", @"\\\\;").Replace("\\,", @"\\\\,").Replace(";", "\\;").Replace(",", "\\,").Replace(@"\\", "\\");
            tempBook.Weight= Convert.ToDecimal(book.Weight.ToString().Replace(";", "\\;").Replace(",", "\\,").Replace("=", "\\=").Replace(@"\\", "\\").Replace("\\=", "\\\\="));
            tempBook.Author = book.Author.Replace("\\", @"\\").Replace("\\=", @"\\\\=").Replace(";", "\\;").Replace(",", "\\,");
            return tempBook;
            //Replace("\\=", "\\\\=").Replace("\\;", "\\\\;").Replace("\\,", "\\\\,")
            //book.Title.Replace("=", "\\=")
            //.Replace(";", "\\;").Replace(",", "\\,").Replace(@"\\", "\\")


            //.Replace("=", "\\=").Replace("\\;", @"\\;")
        }
    }
}