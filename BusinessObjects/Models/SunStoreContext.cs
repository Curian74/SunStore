using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObjects.Models;

public partial class SunStoreContext : DbContext
{
    public SunStoreContext()
    {
    }

    public SunStoreContext(DbContextOptions<SunStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductOption> ProductOptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<VoucherCustomer> VoucherCustomers { get; set; }

    private string GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true, true).Build();
        return configuration["ConnectionStrings:DBContext"]!;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC27E99C39E8");

            entity.ToTable("Category");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Customer__1788CCAC620BCDA3");

            entity.ToTable("Customer");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Ranking).HasMaxLength(10);

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .HasConstraintName("FK__Customer__UserID__29572725");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Employee__1788CCAC88421470");

            entity.ToTable("Employee");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Status).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .HasConstraintName("FK__Employee__UserID__267ABA7A");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC27FCBB84F5");

            entity.ToTable("Notification");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Content).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.NotificationCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Notificat__Creat__440B1D61");

            entity.HasOne(d => d.Order).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Notificat__Order__4222D4EF");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__4316F928");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC273446ADB0");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AdrDelivery).HasMaxLength(300);
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.DenyReason).HasMaxLength(2000);
            entity.Property(e => e.Note).HasMaxLength(2000);
            entity.Property(e => e.Payment)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(100);
            entity.Property(e => e.ShipperId).HasColumnName("ShipperID");
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

            entity.HasOne(d => d.Shipper).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShipperId)
                .HasConstraintName("FK__Orders__ShipperI__398D8EEE");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Orders)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Orders__VoucherI__3A81B327");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC278A4A3ACB");

            entity.ToTable("OrderItem");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItem__Custo__3D5E1FD2");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItem__Order__3F466844");

            entity.HasOne(d => d.ProductOption).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItem__Produ__3E52440B");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC279F33B3E2");

            entity.ToTable("Product");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__Categor__2E1BDC42");
        });

        modelBuilder.Entity<ProductOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductO__3214EC27DD6EDD51");

            entity.ToTable("ProductOption");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Size).HasMaxLength(10);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductOptions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductOp__Produ__30F848ED");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC27BE1179FC");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.LoginProvider).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(11);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.VertificationCode).HasMaxLength(200);
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79C15783F498");

            entity.Property(e => e.VoucherId).HasColumnName("VoucherID");
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Vpercent).HasColumnName("VPercent");
        });

        modelBuilder.Entity<VoucherCustomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VoucherC__3214EC27D58A105E");

            entity.ToTable("VoucherCustomer");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

            entity.HasOne(d => d.Customer).WithMany(p => p.VoucherCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VoucherCu__Custo__36B12243");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherCustomers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VoucherCu__Vouch__35BCFE0A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
