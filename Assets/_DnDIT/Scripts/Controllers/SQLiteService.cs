using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DnDInitiativeTracker.SQLData;
using SQLite;
using UnityEngine;

namespace DnDInitiativeTracker.Service
{
    public class SQLiteService
    {
        SQLiteConnection _dataBase;
        string _dataBasePath;

        public void Initialize(string databaseName)
        {
            var dataBaseFullName = Path.ChangeExtension(databaseName, "db");
            _dataBasePath = Path.Combine(Application.persistentDataPath, dataBaseFullName);
            _dataBase = new SQLiteConnection(_dataBasePath);
        }

        public void Clear()
        {
            _dataBase.Close();
        }

        public void CreateTable<T>() where T : SQLiteData, new()
        {
            _dataBase.CreateTable<T>();
        }

        public void Insert<T>(T data) where T : SQLiteData, new()
        {
            _dataBase.Insert(data);
        }

        public List<T> GetAll<T>() where T : SQLiteData, new()
        {
            var table = _dataBase.Table<T>().ToList();
            return table;
        }

        public List<T> GetAllBy<T>(Expression<Func<T, bool>> filter) where T : SQLiteData, new()
        {
            var table = _dataBase.Table<T>().Where(filter).ToList();
            return table;
        }

        public T GetBy<T>(Expression<Func<T, bool>> filter) where T : SQLiteData, new()
        {
            var entry = _dataBase.Table<T>().FirstOrDefault(filter);
            return entry;
        }

        public T GetById<T>(int id) where T : SQLiteData, new()
        {
            var entry = _dataBase.Table<T>().FirstOrDefault(e => e.Id == id);
            return entry;
        }

        public void Update<T>(T data) where T : SQLiteData, new()
        {
            _dataBase.Update(data);
        }

        public void UpdateAll<T>(IEnumerable<T> data) where T : SQLiteData, new()
        {
            _dataBase.UpdateAll(data);
        }

        public void Destroy<T>(T data) where T : SQLiteData, new()
        {
            _dataBase.Delete<T>(data.Id);
        }

        public bool IsTableEmpty<T>() where T : SQLiteData, new()
        {
            var table = _dataBase.Table<T>();
            bool hasAny = table.Any();
            return !hasAny;
        }

        public TableQuery<T> GetTableQuery<T>() where T : SQLiteData, new()
        {
            return _dataBase.Table<T>();
        }
    }
}