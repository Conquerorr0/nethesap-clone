using System;
//BaseEntity: Tüm entity'ler için temel özellikleri içeren abstract sınıf.
// Id, oluşturma tarihi, güncelleme tarihi ve silinme durumu gibi ortak alanları içerir.
namespace Nethesap.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
} 