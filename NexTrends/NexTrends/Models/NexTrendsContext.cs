using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NexTrends.Models;

public partial class NexTrendsContext : DbContext
{
    public NexTrendsContext()
    {
    }

    public NexTrendsContext(DbContextOptions<NexTrendsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponUsage> CouponUsages { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=NEXSUS-DV94\\SQLEXPRESS;Initial Catalog=NexTrends;User Id=sa;Password=ccntspl@123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ADMIN__3214EC279EAF7A63");

            entity.ToTable("ADMIN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("admin");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CART__3214EC27F989FAD8");

            entity.ToTable("CART");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_ID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId).HasColumnName("Product_ID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__CART__Customer_I__2E1BDC42");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CATEGORY__3214EC2769419568");

            entity.ToTable("CATEGORY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__COUPONS__3214EC27805FC703");

            entity.ToTable("COUPONS");

            entity.HasIndex(e => e.CouponCode, "UQ__COUPONS__A3478C1758200818").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("Category_ID");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(100)
                .HasColumnName("Coupon_Code");
            entity.Property(e => e.DiscountPercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Discount_Percentage");
            entity.Property(e => e.ExpiryDate).HasColumnName("Expiry_Date");
            entity.Property(e => e.Occasion).HasMaxLength(100);
            entity.Property(e => e.Prize).HasColumnName("prize");
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__COUPON_U__3214EC27FFAAF3F0");

            entity.ToTable("COUPON_USAGE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CouponId).HasColumnName("Coupon_ID");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_ID");
            entity.Property(e => e.UsageDate).HasColumnName("Usage_Date");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK__COUPON_US__Coupo__403A8C7D");

            entity.HasOne(d => d.Customer).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__COUPON_US__Custo__3F466844");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CUSTOMER__3214EC27B5E3100C");

            entity.ToTable("CUSTOMER");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Pincode).HasMaxLength(10);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FEEDBACK__3214EC2747587351");

            entity.ToTable("FEEDBACK");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cid).HasColumnName("CID");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CidNavigation).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.Cid)
                .HasConstraintName("FK__FEEDBACK__CID__4316F928");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ORDERS__3214EC2714D2E267");

            entity.ToTable("ORDERS", tb => tb.HasTrigger("UpdateProductQuantity5"));

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CartId).HasColumnName("Cart_ID");
            entity.Property(e => e.CouponId).HasColumnName("Coupon_ID");
            entity.Property(e => e.ModeOfPayment)
                .HasMaxLength(50)
                .HasColumnName("Mode_of_Payment");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Cart).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__ORDERS__Cart_ID__3B75D760");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__ORDERS__Coupon_I__3C69FB99");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PRODUCT__3214EC274D8313D2");

            entity.ToTable("PRODUCT");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("Category_ID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__PRODUCT__Categor__2B3F6F97");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
