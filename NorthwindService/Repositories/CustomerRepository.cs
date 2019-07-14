using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using Packt.CS7;

namespace NorthwindService.Repositories
{
    public class CustomerRepository :ICustomerRepository
    {
        // кэшируем клиентов в потокобезопасном словаре для повышения
        // производительности
        private static ConcurrentDictionary<string, Customer> customersCache;
        private Northwind db;
        public CustomerRepository(Northwind db)
        {
            this.db = db;
            // предварительно загружаем клиентов из обычного словаря с ключом
            // CustomerID,
            // а затем конвертируем в потокобезопасный ConcurrentDictionary
            if (customersCache == null)
            {
                customersCache = new ConcurrentDictionary<string, Customer>(
                db.Customers.ToDictionary(c => c.CustomerID));
            }
        }
        public async Task<Customer> CreateAsync(Customer c)
        {
            // приводим CustomerID к верхнему регистру
            c.CustomerID = c.CustomerID.ToUpper();
            // добавляем в базу данных с помощью EF Core
            EntityEntry<Customer> added = await db.Customers.AddAsync(c);
            int affected = await db.SaveChangesAsync();
            if (affected == 1)
            {
                // если клиент новый, то добавляем в кэш, иначе вызываем
                // метод UpdateCache
                return customersCache.AddOrUpdate(c.CustomerID, c, UpdateCache);
            }
            else
            {
                return null;
            }
        }
        public async Task<IEnumerable<Customer>> RetrieveAllAsync()
        {
            // для повышения производительности извлекаем из кэша
            return await Task.Run<IEnumerable<Customer>>(
            () => customersCache.Values);
        }
        public async Task<Customer> RetrieveAsync(string id)
        {
            return await Task.Run(() =>
            {
                // для повышения производительности извлекаем из кэша
                id = id.ToUpper();
                Customer c;
                customersCache.TryGetValue(id, out c);
                return c;
            });
        }
        private Customer UpdateCache(string id, Customer c)
        {
            Customer old;
            if (customersCache.TryGetValue(id, out old))
            {
                if (customersCache.TryUpdate(id, c, old))
                {
                    return c;
                }
            }
            return null;
        }
        public async Task<Customer> UpdateAsync(string id, Customer c)
        {
            return await Task.Run(() =>
            {
                // нормализуем идентификатор клиента
                id = id.ToUpper();
                c.CustomerID = c.CustomerID.ToUpper();
                // обновляем в базе данных
                db.Customers.Update(c);
                int affected = db.SaveChanges();
                if (affected == 1)
                {
                    // обновляем в кэше
                    return Task.Run(() => UpdateCache(id, c));
                }
                return null;
            });
        }
        public async Task<bool> DeleteAsync(string id)
        {
            return await Task.Run(() =>
            {
                id = id.ToUpper();
                // удаляем из базы данных
                Customer c = db.Customers.Find(id);
                db.Customers.Remove(c);
                int affected = db.SaveChanges();
                if (affected == 1)
                {
                    // удаляем из кэша
                    return Task.Run(() => customersCache.TryRemove(id, out c));
                }
                else
                {
                    return null;
                }
            });
        }
    }
}
