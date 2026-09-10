using BackendTechnicalEquipmentBorrowingSystem.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackendTechnicalEquipmentBorrowingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Faculty> Faculties => Set<Faculty>();

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public DbSet<ArchivedUser> ArchivedUsers => Set<ArchivedUser>();
    public DbSet<ArchivedItem> ArchivedItems => Set<ArchivedItem>();
    public DbSet<ArchivedBorrowing> ArchivedBorrowings => Set<ArchivedBorrowing>();

    public DbSet<ItemRfidRegistration> ItemRfidRegistrations => Set<ItemRfidRegistration>();
    public DbSet<StudentRfidRegistration> StudentRfidRegistrations => Set<StudentRfidRegistration>();
    public DbSet<HardwareBorrowSession> HardwareBorrowSessions => Set<HardwareBorrowSession>();
    public DbSet<HardwareReturnSession> HardwareReturnSessions => Set<HardwareReturnSession>();
    public DbSet<GuestScanSession> GuestScanSessions => Set<GuestScanSession>();
    public DbSet<RfidTag> RfidTags => Set<RfidTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TPT: each subtype gets its own table joined to Users by Id.
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Student>().ToTable("Students");
        modelBuilder.Entity<Faculty>().ToTable("Faculties");

        // Store every enum as a string (no raw string statuses, no brittle ints).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (type.IsEnum)
                {
                    var converter = (ValueConverter)Activator.CreateInstance(
                        typeof(EnumToStringConverter<>).MakeGenericType(type))!;
                    property.SetValueConverter(converter);
                }
            }
        }

        // Unique constraints (Postgres treats multiple NULLs as distinct, so optional UIDs are fine).
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(s => s.StudentNumber).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(s => s.RfidCardUid).IsUnique();
        modelBuilder.Entity<Item>().HasIndex(i => i.SerialNumber).IsUnique();
        modelBuilder.Entity<Item>().HasIndex(i => i.RfidUid).IsUnique();
        modelBuilder.Entity<RfidTag>().HasIndex(t => t.Uid).IsUnique();

        // Keep borrowing history even if the item/user row is later removed.
        modelBuilder.Entity<Borrowing>()
            .HasOne(b => b.Item).WithMany(i => i.Borrowings)
            .HasForeignKey(b => b.ItemId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Borrowing>()
            .HasOne(b => b.Borrower).WithMany(u => u.Borrowings)
            .HasForeignKey(b => b.BorrowerId).OnDelete(DeleteBehavior.Restrict);
    }
}
