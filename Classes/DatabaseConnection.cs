using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Classes
{
    public class DatabaseConnection : DbContext
    {
        // Таблица пользователей
        public DbSet<User> Users { get; set; }

        // Таблица типов оборудования
        public DbSet<EquipmentType> EquipmentTypes { get; set; }

        // Таблица направлений
        public DbSet<Direction> Directions { get; set; }

        // Таблица статусов
        public DbSet<Status> Statuses { get; set; }

        // Таблица разработчиков
        public DbSet<Developer> Developers { get; set; }

        // Таблица программного обеспечения
        public DbSet<Software> Software { get; set; }

        // Таблица аудиторий
        public DbSet<Room> Rooms { get; set; }

        // Таблица моделей оборудования
        public DbSet<Model> Models { get; set; }

        // Таблица оборудования
        public DbSet<Equipment> Equipment { get; set; }

        // Таблица связей оборудование-ПО
        public DbSet<EquipmentSoftware> EquipmentSoftware { get; set; }

        // Таблица истории перемещений оборудования по аудиториям
        public DbSet<EquipmentRoomHistory> EquipmentRoomHistory { get; set; }

        // Таблица истории ответственных за оборудование
        public DbSet<EquipmentResponsibleHistory> EquipmentResponsibleHistory { get; set; }

        // Таблица сетевых настроек
        public DbSet<NetworkSettings> NetworkSettings { get; set; }

        // Таблица типов расходных материалов
        public DbSet<ConsumableType> ConsumableTypes { get; set; }

        // Таблица характеристик расходных материалов
        public DbSet<ConsumableCharacteristic> ConsumableCharacteristics { get; set; }

        // Таблица расходных материалов
        public DbSet<Consumable> Consumables { get; set; }

        // Таблица значений характеристик расходных материалов
        public DbSet<ConsumableCharacteristicValue> ConsumableCharacteristicValues { get; set; }

        // Таблица связей расходные материалы-оборудование
        public DbSet<ConsumableEquipment> ConsumableEquipment { get; set; }

        // Таблица истории ответственных за расходные материалы
        public DbSet<ConsumableResponsibleHistory> ConsumableResponsibleHistory { get; set; }

        // Таблица инвентаризаций
        public DbSet<Inventory> Inventories { get; set; }

        // Таблица проверок инвентаризации
        public DbSet<InventoryCheck> InventoryChecks { get; set; }

        public DatabaseConnection()
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Конфигурация подключения к базе данных MySQL
        /// </summary>
        /// <param name="optionsBuilder">Билдер опций контекста БД</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Измените строку подключения согласно вашим настройкам MySQL
            // server - адрес сервера
            // port - порт (по умолчанию 3306)
            // uid - имя пользователя
            // pwd - пароль
            // database - название базы данных
            optionsBuilder.UseMySql(
                "server=127.0.0.1;port=3306;uid=root;pwd=;database=equipment_management",
                new MySqlServerVersion(new Version(8, 0, 11)));
        }

        /// <summary>
        /// Конфигурация моделей базы данных
        /// </summary>
        /// <param name="modelBuilder">Билдер моделей</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка таблицы Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Настройка таблицы EquipmentTypes
            modelBuilder.Entity<EquipmentType>()
                .HasIndex(et => et.Name)
                .IsUnique();

            // Настройка таблицы Directions
            modelBuilder.Entity<Direction>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // Настройка таблицы Statuses
            modelBuilder.Entity<Status>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // Настройка таблицы Developers
            modelBuilder.Entity<Developer>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // Настройка таблицы ConsumableTypes
            modelBuilder.Entity<ConsumableType>()
                .HasIndex(ct => ct.Name)
                .IsUnique();

            // Настройка таблицы Equipment
            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.InventoryNumber)
                .IsUnique();

            // Настройка таблицы EquipmentSoftware (уникальный составной ключ)
            modelBuilder.Entity<EquipmentSoftware>()
                .HasIndex(es => new { es.EquipmentId, es.SoftwareId })
                .IsUnique();

            // Настройка таблицы ConsumableCharacteristics (уникальный составной ключ)
            modelBuilder.Entity<ConsumableCharacteristic>()
                .HasIndex(cc => new { cc.ConsumableTypeId, cc.Name })
                .IsUnique();

            // Настройка таблицы ConsumableCharacteristicValues (уникальный составной ключ)
            modelBuilder.Entity<ConsumableCharacteristicValue>()
                .HasIndex(ccv => new { ccv.ConsumableId, ccv.CharacteristicId })
                .IsUnique();

            // Настройка таблицы ConsumableEquipment (уникальный составной ключ)
            modelBuilder.Entity<ConsumableEquipment>()
                .HasIndex(ce => new { ce.ConsumableId, ce.EquipmentId })
                .IsUnique();

            // Настройка таблицы InventoryChecks (уникальный составной ключ)
            modelBuilder.Entity<InventoryCheck>()
                .HasIndex(ic => new { ic.InventoryId, ic.EquipmentId })
                .IsUnique();

            // Настройка таблицы NetworkSettings (уникальный IP-адрес)
            modelBuilder.Entity<NetworkSettings>()
                .HasIndex(ns => ns.IpAddress)
                .IsUnique();
        }
    }
}
