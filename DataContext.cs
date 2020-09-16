using ORM.Contracts;
using ORM.Db;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Text;



namespace ORM
{
    public class ToUpdateCache
    {
        Dictionary<string, object> _updateCache = new Dictionary<string, object>();
        public void CreateItem<T>(T item) where T : DbEntity
        {

            if (!_updateCache.ContainsKey(item.Id))
            {
                _updateCache[item.Id] = item;
            }
        }

        public void Clear()
        {
            _updateCache.Clear();
        }
        public Dictionary<string, object> GetItems()
        {
            return _updateCache;
        }
        public T GetItem<T>(string key) where T : DbEntity
        {
            return (T)_updateCache[key];
        }
        public bool ItemContains(string key)
        {
            return _updateCache.ContainsKey(key);
        }
    }



    public class ToInsertCache
    {
        Dictionary<string, object> _insertCache = new Dictionary<string, object>();
        public void CreateItem<T>(T item) where T : DbEntity
        {

            if (!_insertCache.ContainsKey(item.Id))
            {
                _insertCache[item.Id] = item;

            }
        }

        public void Clear()
        {
            _insertCache.Clear();
        }
        public void Clear(string key)
        {
            _insertCache.Remove(key);
            //_insertCache.Clear();
        }
        public Dictionary<string, object> GetItems<T>()
        {
            return _insertCache;
        }

        public T GetItem<T>(string key) where T : DbEntity
        {
            return (T)_insertCache[key];
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

        public T Find<T>(string id) where T : DbEntity
        {
            Console.WriteLine("FIND ENTRY:" + id);
            if (_updCache.ItemContains(id))
                return _updCache.GetItem<T>(id);
            else
            {
                string answer = dbEngine.Execute($"get Id={id};");
                if (answer == ";")
                    return null;
                else
                {
                    answer = DeleteScreening(answer);
                    T obj = Serializator.Deserialize<T>(answer);
                    _updCache.CreateItem(obj);
                    Console.WriteLine(answer);
                    return obj;
                }
            }

        }

        public T Read<T>(string id) where T : DbEntity
        {
            Console.WriteLine("READ ENTRY");
            if (_updCache.ItemContains(id))
                return _updCache.GetItem<T>(id);
            else
            {
                string answer = dbEngine.Execute($"get Id={id};");
                if (answer == ";")
                    throw new Exception();
                else
                {
                    answer = DeleteScreening(answer);
                    Console.WriteLine(answer);
                    T obj = Serializator.Deserialize<T>(answer);
                    _updCache.CreateItem(obj);
                    Console.WriteLine(answer);
                    return obj;

                }
            }

        }

        public void Insert<T>(T entity) where T : DbEntity
        {
            Console.WriteLine("INSERT ENTRY");
            if (entity is null)
                throw new Exception();
            else
            {
                _insCache.CreateItem<T>(entity);
                Console.WriteLine(typeof(T));
            }

        }
        //public void testmethod()
        //{
        //    Book book = new Book() { Author = "author", Id = "iddd", Price = 25, Skill = "skill", Title = "title", Weight = 72 };
        //    Car car = new Car() { Id = "iddd", Price = 25, Capacity=121, Title = "title", Engine="Двигло", Speed = 300};
        //    Console.WriteLine("SERIALIZATION: ");
        //    string str = Serializator.Serialize(book);
        //    string req = $"add ";
        //    Console.WriteLine(req+str);
        //    string strin = Serializator.Serialize(car); 
        //    Console.WriteLine(strin);

        //    object obj = Serializator.Deserialize<Book>(str);
        //    Console.WriteLine(obj.ToString());
        //}
        public string DeleteScreening(string answ)
        {
            string answWithoutScreening;
            answ = answ.Replace("\\;", ";");
            answ = answ.Replace("\\\\", "%");
            answ = answ.Replace("\\,", "\\-");
            answ = answ.Replace(@"\", "\\");
            answWithoutScreening = answ.Replace("\\=", "\\&");

            return answWithoutScreening;
        }

        public void SubmitChanges()
        {
            Console.WriteLine("SUBMITCHANGES ENTRY");
            string req = "";

            foreach (var item in _insCache.GetItems<object>().Values)
            {
                req += $"add ";
                req += Serializator.Serialize(item);
                ///////////////////// req+= Собрать запрос из полей объекта и добавить экранирование
            }
            foreach (var item in _updCache.GetItems().Values)
            {
                req += $"upd ";
                req += Serializator.Serialize(item);
                ///////////////////// req+= Собрать запрос из полей объекта и добавить экранирование
            }
            string answer = dbEngine.Execute(req);
            if (answer.Contains("ok"))
            {
                foreach (var item in _insCache.GetItems<object>().Values)
                {

                }
            }
            else
                throw new Exception();

            _insCache.Clear();
        }
    }

    class Serializator
    {
        public static string Serialize(object obj)
        {
            var sb = new StringBuilder();
            foreach (var p in obj.GetType().GetProperties())
            {
                var key = p.Name;
                var value = p.GetValue(obj, null).ToString();

                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0}={1},", key, value);

            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(";");
            return sb.ToString();
        }

        public static T Deserialize<T>(string str) where T : DbEntity
        {
            Type type = Type.GetType(typeof(T).FullName, false, true);
            ConstructorInfo ci = type.GetConstructor(new Type[] { });
            object obj = ci.Invoke(new object[] { });

            Dictionary<string, string> dict = Parser.ParseAnswer(str);

            foreach (var p in obj.GetType().GetProperties())
            {
                if (dict.ContainsKey(p.Name))
                {
                    Console.WriteLine(p.PropertyType);
                    if (p.PropertyType == typeof(string))
                        p.SetValue(obj, dict[p.Name]);
                    if (p.PropertyType == typeof(int))
                        p.SetValue(obj, Convert.ToInt32(dict[p.Name]));
                    if (p.PropertyType == typeof(bool))
                        p.SetValue(obj, Convert.ToBoolean(dict[p.Name]));
                    if (p.PropertyType == typeof(short))
                        p.SetValue(obj, Convert.ToInt16(dict[p.Name]));
                    if (p.PropertyType == typeof(long))
                        p.SetValue(obj, Convert.ToInt64(dict[p.Name]));
                    if (p.PropertyType == typeof(decimal))
                    {
                        //Console.WriteLine(dict[p.Name]);
                        p.SetValue(obj, Convert.ToDecimal(dict[p.Name]));
                    }
                    if (p.PropertyType == typeof(double))
                        p.SetValue(obj, Convert.ToDouble(dict[p.Name]));
                    if (p.PropertyType == typeof(DateTime))
                        p.SetValue(obj, Convert.ToDateTime(dict[p.Name]));
                    if (p.PropertyType == typeof(TimeSpan))
                        p.SetValue(obj, TimeSpan.Parse(dict[p.Name]));
                    if (p.PropertyType == typeof(Guid))
                        p.SetValue(obj, Guid.Parse(dict[p.Name]));
                }
            }
            return (T)obj;
        }
    }
    class Parser
    {
        public static Dictionary<string, string> ParseAnswer(string answ)
        {
            //Console.WriteLine(answ);
            answ = answ.Trim(',', ';');
            string[] pairs = answ.Split(',');

            Dictionary<string, string> dict = pairs.Select(s => s.Split('=')).ToDictionary(arr => arr[0].Replace("%", "\\").Replace("\\&", "=").Replace("\\-", ","),
                arr => arr[1].Replace("%", "\\").Replace("\\&", "=").Replace("\\-", ","));

            return dict;
        }
    }

}