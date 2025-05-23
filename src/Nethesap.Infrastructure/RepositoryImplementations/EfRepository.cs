using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using Nethesap.Domain.IRepositories;
using Nethesap.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

/// <summary>
/// Entity Framework Core kullanarak generic repository pattern'in implementasyonu.
/// Tüm entity'ler için ortak veritabanı işlemlerini gerçekleştirir.
/// </summary>
/// <remarks>
/// Bu sınıf, temel CRUD operasyonlarını ve yaygın sorgulama işlemlerini sağlar.
/// Soft delete özelliği ve otomatik audit alanları (CreatedAt, UpdatedAt) yönetimi içerir.
/// </remarks>
/// <typeparam name="T">BaseEntity'den türetilmiş entity tipi</typeparam>
namespace Nethesap.Infrastructure.RepositoryImplementations
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Veritabanı bağlantı context'i
        /// </summary>
        protected readonly AppDbContext _context;

        /// <summary>
        /// Entity'nin DbSet referansı
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Repository constructor'ı
        /// </summary>
        /// <param name="context">Veritabanı context'i</param>
        public EfRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// ID'ye göre entity getirir
        /// </summary>
        /// <param name="id">Entity ID'si</param>
        /// <returns>Bulunan entity veya null</returns>
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Tüm entity'leri getirir
        /// </summary>
        /// <returns>Entity listesi</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Belirtilen koşula göre entity'leri filtreler
        /// </summary>
        /// <param name="predicate">Filtreleme koşulu</param>
        /// <returns>Filtrelenmiş entity listesi</returns>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Belirtilen koşula göre tek bir entity getirir
        /// </summary>
        /// <param name="predicate">Filtreleme koşulu</param>
        /// <returns>Bulunan entity veya null</returns>
        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Yeni entity ekler
        /// </summary>
        /// <param name="entity">Eklenecek entity</param>
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Birden fazla entity ekler
        /// </summary>
        /// <param name="entities">Eklenecek entity'ler</param>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Entity'yi günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Entity'yi soft delete yapar
        /// </summary>
        /// <param name="entity">Silinecek entity</param>
        public virtual async Task RemoveAsync(T entity)
        {
            entity.IsDeleted = true;
            _dbSet.Update(entity);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Birden fazla entity'yi soft delete yapar
        /// </summary>
        /// <param name="entities">Silinecek entity'ler</param>
        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
            }
            _dbSet.UpdateRange(entities);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Belirtilen koşula göre entity varlığını kontrol eder
        /// </summary>
        /// <param name="predicate">Kontrol koşulu</param>
        /// <returns>Entity varsa true, yoksa false</returns>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// Belirtilen koşula göre entity sayısını hesaplar
        /// </summary>
        /// <param name="predicate">Sayım koşulu</param>
        /// <returns>Entity sayısı</returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        /// <summary>
        /// Değişiklikleri veritabanına kaydeder
        /// </summary>
        /// <returns>Etkilenen satır sayısı</returns>
        protected async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 