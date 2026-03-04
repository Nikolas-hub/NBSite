using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class NbshopContext : DbContext
{
    public NbshopContext()
    {
    }

    public NbshopContext(DbContextOptions<NbshopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CatalogDiscount> CatalogDiscounts { get; set; }

    public virtual DbSet<AccountsProfile> AccountsProfiles { get; set; }

    public virtual DbSet<AuthGroup> AuthGroups { get; set; }

    public virtual DbSet<AuthGroupPermission> AuthGroupPermissions { get; set; }

    public virtual DbSet<AuthPermission> AuthPermissions { get; set; }

    public virtual DbSet<AuthUser> AuthUsers { get; set; }

    public virtual DbSet<AuthUserGroup> AuthUserGroups { get; set; }

    public virtual DbSet<AuthUserUserPermission> AuthUserUserPermissions { get; set; }

    public virtual DbSet<CatalogCategory> CatalogCategories { get; set; }

    public virtual DbSet<CatalogCategoryrelation> CatalogCategoryrelations { get; set; }

    public virtual DbSet<CatalogCoupon> CatalogCoupons { get; set; }

    public virtual DbSet<CatalogDelivery> CatalogDeliveries { get; set; }

    public virtual DbSet<CatalogInstocksubscription> CatalogInstocksubscriptions { get; set; }

    public virtual DbSet<CatalogManufacturer> CatalogManufacturers { get; set; }

    public virtual DbSet<CatalogOrder> CatalogOrders { get; set; }

    public virtual DbSet<CatalogOrderproduct> CatalogOrderproducts { get; set; }

    public virtual DbSet<CatalogPayment> CatalogPayments { get; set; }

    public virtual DbSet<CatalogProduct> CatalogProducts { get; set; }

    public virtual DbSet<CatalogPromo> CatalogPromos { get; set; }

    public virtual DbSet<CatalogPromoProduct> CatalogPromoProducts { get; set; }

    public virtual DbSet<CeleryTaskmetum> CeleryTaskmeta { get; set; }

    public virtual DbSet<CeleryTasksetmetum> CeleryTasksetmeta { get; set; }

    public virtual DbSet<ContentFile> ContentFiles { get; set; }

    public virtual DbSet<ContentFolder> ContentFolders { get; set; }

    public virtual DbSet<ContentNews> ContentNews { get; set; }

    public virtual DbSet<ContentPage> ContentPages { get; set; }

    public virtual DbSet<DjangoAdminLog> DjangoAdminLogs { get; set; }

    public virtual DbSet<DjangoContentType> DjangoContentTypes { get; set; }

    public virtual DbSet<DjangoMigration> DjangoMigrations { get; set; }

    public virtual DbSet<DjangoSession> DjangoSessions { get; set; }

    public virtual DbSet<DjangoSite> DjangoSites { get; set; }

    public virtual DbSet<DjceleryCrontabschedule> DjceleryCrontabschedules { get; set; }

    public virtual DbSet<DjceleryIntervalschedule> DjceleryIntervalschedules { get; set; }

    public virtual DbSet<DjceleryPeriodictask> DjceleryPeriodictasks { get; set; }

    public virtual DbSet<DjceleryPeriodictask1> DjceleryPeriodictasks1 { get; set; }

    public virtual DbSet<DjceleryTaskstate> DjceleryTaskstates { get; set; }

    public virtual DbSet<DjceleryWorkerstate> DjceleryWorkerstates { get; set; }

    public virtual DbSet<DjkombuMessage> DjkombuMessages { get; set; }

    public virtual DbSet<DjkombuQueue> DjkombuQueues { get; set; }

    public virtual DbSet<ReferencesCity> ReferencesCities { get; set; }

    public virtual DbSet<SearchSearchlogentry> SearchSearchlogentries { get; set; }

    public virtual DbSet<TempOldProduct> TempOldProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogDiscount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_catalog_discount");
            entity.ToTable("catalog_discount", "nbshop");
            entity.HasIndex(e => e.ProductId, "ix_catalog_discount_product_id");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();  // или UseSerialColumn()

            entity.Property(e => e.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("bigint");  // или оставьте без указания типа, если ProductId в CatalogProduct тоже bigint

            entity.Property(e => e.DiscountType)
                .HasColumnName("discount_type")
                .HasColumnType("integer");

            entity.Property(e => e.Value)
                .HasColumnName("value")
                .HasColumnType("numeric(18,2)");

            entity.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean");

            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasColumnType("integer");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.CatalogDiscounts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_catalog_discount_product_id");
        });

        modelBuilder.Entity<AccountsProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16391_primary");

            entity.ToTable("accounts_profile", "nbshop");

            entity.HasIndex(e => e.Email, "idx_16391_accounts_profile_0c83f57c");

            entity.HasIndex(e => e.ResetPasswordKey, "idx_16391_accounts_profile_80531b51");

            entity.HasIndex(e => e.Fio, "idx_16391_accounts_profile_b068931c");

            entity.HasIndex(e => e.CityId, "idx_16391_accounts_profile_c7141997");

            entity.HasIndex(e => e.Email, "idx_16391_accounts_profile_email_71583453_uniq").IsUnique();

            entity.HasIndex(e => e.Phone, "idx_16391_accounts_profile_f7a42fe7");

            entity.HasIndex(e => e.UserId, "idx_16391_user_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('accounts_profile_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.Company)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("company");
            entity.Property(e => e.CompanyNavCode)
                .HasMaxLength(30)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("company_nav_code");
            entity.Property(e => e.CompanyPost)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("company_post");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("email");
            entity.Property(e => e.Fio)
                .HasMaxLength(255)
                .HasColumnName("fio");
            entity.Property(e => e.Image)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("image");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("phone");
            entity.Property(e => e.PricesVisible).HasColumnName("prices_visible");
            entity.Property(e => e.ResetPasswordKey)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("reset_password_key");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.City).WithMany(p => p.AccountsProfiles)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("accounts_profile_city_id_267b3d7f_fk");

            entity.HasOne(d => d.User).WithOne(p => p.AccountsProfile)
                .HasForeignKey<AccountsProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("accounts_profile_user_id_49a85d32_fk_auth_user_id");
        });

        modelBuilder.Entity<AuthGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16409_primary");

            entity.ToTable("auth_group", "nbshop");

            entity.HasIndex(e => e.Name, "idx_16409_name").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('auth_group_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
        });

        modelBuilder.Entity<AuthGroupPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16416_primary");

            entity.ToTable("auth_group_permissions", "nbshop");

            entity.HasIndex(e => e.PermissionId, "idx_16416_auth_group_permissi_permission_id_84c5c92e_fk_auth_pe");

            entity.HasIndex(e => new { e.GroupId, e.PermissionId }, "idx_16416_auth_group_permissions_group_id_0cd325b0_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('auth_group_permissions_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");

            entity.HasOne(d => d.Group).WithMany(p => p.AuthGroupPermissions)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_group_permissions_group_id_b120cbf9_fk_auth_group_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.AuthGroupPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_group_permissi_permission_id_84c5c92e_fk_auth_permission_i");
        });

        modelBuilder.Entity<AuthPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16424_primary");

            entity.ToTable("auth_permission", "nbshop");

            entity.HasIndex(e => new { e.ContentTypeId, e.Codename }, "idx_16424_auth_permission_content_type_id_01ab375a_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('auth_permission_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Codename)
                .HasMaxLength(100)
                .HasColumnName("codename");
            entity.Property(e => e.ContentTypeId).HasColumnName("content_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.ContentType).WithMany(p => p.AuthPermissions)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_permissi_content_type_id_2f476e4b_fk_django_content_type_i");
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16433_primary");

            entity.ToTable("auth_user", "nbshop");

            entity.HasIndex(e => e.Username, "idx_16433_username").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('auth_user_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.DateJoined).HasColumnName("date_joined");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsStaff).HasColumnName("is_staff");
            entity.Property(e => e.IsSuperuser).HasColumnName("is_superuser");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.Username)
                .HasMaxLength(150)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AuthUserGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16450_primary");

            entity.ToTable("auth_user_groups", "nbshop");

            entity.HasIndex(e => e.GroupId, "idx_16450_auth_user_groups_group_id_97559544_fk_auth_group_id");

            entity.HasIndex(e => new { e.UserId, e.GroupId }, "idx_16450_auth_user_groups_user_id_94350c0c_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('auth_user_groups_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.AuthUserGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_user_groups_group_id_97559544_fk_auth_group_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuthUserGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_user_groups_user_id_6a12ed8b_fk_auth_user_id");
        });

        modelBuilder.Entity<AuthUserUserPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16458_primary");

            entity.ToTable("auth_user_user_permissions", "nbshop");

            entity.HasIndex(e => e.PermissionId, "idx_16458_auth_user_user_perm_permission_id_1fbb5f2c_fk_auth_pe");

            entity.HasIndex(e => new { e.UserId, e.PermissionId }, "idx_16458_auth_user_user_permissions_user_id_14a6b632_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('auth_user_user_permissions_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.AuthUserUserPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_user_user_perm_permission_id_1fbb5f2c_fk_auth_permission_i");

            entity.HasOne(d => d.User).WithMany(p => p.AuthUserUserPermissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("auth_user_user_permissions_user_id_a95ead1b_fk_auth_user_id");
        });

        modelBuilder.Entity<CatalogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16466_primary");

            entity.ToTable("catalog_category", "nbshop");

            entity.HasIndex(e => e.Alias, "idx_16466_alias").IsUnique();

            entity.HasIndex(e => e.Name, "idx_16466_catalog_category_b068931c");

            entity.HasIndex(e => e.Active, "idx_16466_catalog_category_c76a5e84");

            entity.HasIndex(e => e.Sort, "idx_16466_catalog_category_cadc8c8d");

            entity.HasIndex(e => e.Parent, "idx_16466_catalog_category_d0e45878");

            entity.HasIndex(e => e.Code, "idx_16466_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_category_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Introtext).HasColumnName("introtext");
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_keywords");
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(80)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_title");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Parent)
                .HasMaxLength(30)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("parent");
            entity.Property(e => e.Sort).HasColumnName("sort");
            entity.Property(e => e.SvgIcon).HasColumnName("svg_icon");
        });

        modelBuilder.Entity<CatalogCategoryrelation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16483_primary");

            entity.ToTable("catalog_categoryrelation", "nbshop");

            entity.HasIndex(e => e.SourceId, "idx_16483_catalog_categoryrela_source_id_6b651b4f_fk_catalog_c");

            entity.HasIndex(e => e.TargetId, "idx_16483_catalog_categoryrela_target_id_cf9820e4_fk_catalog_c");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_categoryrelation_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Source).WithMany(p => p.CatalogCategoryrelationSources)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_categoryrela_source_id_6b651b4f_fk_catalog_c");

            entity.HasOne(d => d.Target).WithMany(p => p.CatalogCategoryrelationTargets)
                .HasForeignKey(d => d.TargetId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_categoryrela_target_id_cf9820e4_fk_catalog_c");
        });

        modelBuilder.Entity<CatalogCoupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16492_primary");

            entity.ToTable("catalog_coupon", "nbshop");

            entity.HasIndex(e => e.Code, "idx_16492_catalog_coupon_code_05b2baa4_uniq");

            entity.HasIndex(e => e.PromoId, "idx_16492_catalog_coupon_promo_id_1908d326_fk_catalog_promo_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_coupon_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.PromoId).HasColumnName("promo_id");

            entity.HasOne(d => d.Promo).WithMany(p => p.CatalogCoupons)
                .HasForeignKey(d => d.PromoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_coupon_promo_id_1908d326_fk");
        });

        modelBuilder.Entity<CatalogDelivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16501_primary");

            entity.ToTable("catalog_delivery", "nbshop");

            entity.HasIndex(e => e.Name, "idx_16501_catalog_delivery_b068931c");

            entity.HasIndex(e => e.Handler, "idx_16501_catalog_delivery_c1cbfe27");

            entity.HasIndex(e => e.Sort, "idx_16501_catalog_delivery_cadc8c8d");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_delivery_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Handler)
                .HasMaxLength(255)
                .HasColumnName("handler");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Sort).HasColumnName("sort");
        });

        modelBuilder.Entity<CatalogInstocksubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16512_primary");

            entity.ToTable("catalog_instocksubscription", "nbshop");

            entity.HasIndex(e => e.Email, "idx_16512_catalog_instocksubscription_0c83f57c");

            entity.HasIndex(e => e.ProductId, "idx_16512_catalog_instocksubscription_9bea82de");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_instocksubscription_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Product).WithMany(p => p.CatalogInstocksubscriptions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_instocksubscription_product_id_ac990966_fk");
        });

        modelBuilder.Entity<CatalogManufacturer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16521_primary");

            entity.ToTable("catalog_manufacturer", "nbshop");

            entity.HasIndex(e => e.Alias, "idx_16521_alias").IsUnique();

            entity.HasIndex(e => e.CountryCode, "idx_16521_catalog_manufacturer_55eceb8d");

            entity.HasIndex(e => e.Name, "idx_16521_catalog_manufacturer_b068931c");

            entity.HasIndex(e => e.Active, "idx_16521_catalog_manufacturer_c76a5e84");

            entity.HasIndex(e => e.Sort, "idx_16521_catalog_manufacturer_cadc8c8d");

            entity.HasIndex(e => e.Code, "idx_16521_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_manufacturer_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(4)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("country_code");
            entity.Property(e => e.Image)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("image");
            entity.Property(e => e.Introtext).HasColumnName("introtext");
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_keywords");
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(80)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_title");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Sort).HasColumnName("sort");
        });

        modelBuilder.Entity<CatalogOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16539_primary");

            entity.ToTable("catalog_order", "nbshop");

            entity.HasIndex(e => e.Email, "idx_16539_catalog_order_0c83f57c");

            entity.HasIndex(e => e.CouponId, "idx_16539_catalog_order_2fb72c6e");

            entity.HasIndex(e => e.PaymentId, "idx_16539_catalog_order_376ebbba");

            entity.HasIndex(e => e.CityId, "idx_16539_catalog_order_c7141997");

            entity.HasIndex(e => e.Reciever, "idx_16539_catalog_order_d97dac9b");

            entity.HasIndex(e => e.DeliveryId, "idx_16539_catalog_order_delivery_id_9852ee47_fk_catalog_deliver");

            entity.HasIndex(e => e.UserId, "idx_16539_catalog_order_e8701ad4");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_order_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeliveryId).HasColumnName("delivery_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("phone");
            entity.Property(e => e.Reciever)
                .HasMaxLength(255)
                .HasColumnName("reciever");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.City).WithMany(p => p.CatalogOrders)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_order_city_id_e970f45c_fk");

            entity.HasOne(d => d.Delivery).WithMany(p => p.CatalogOrders)
                .HasForeignKey(d => d.DeliveryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_order_delivery_id_9852ee47_fk");

            entity.HasOne(d => d.Payment).WithMany(p => p.CatalogOrders)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_order_payment_id_c27a8c29_fk");

            entity.HasOne(d => d.User).WithMany(p => p.CatalogOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_order_user_id_312bd45e_fk_auth_user_id");
        });

        modelBuilder.Entity<CatalogOrderproduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16551_primary");

            entity.ToTable("catalog_orderproduct", "nbshop");

            entity.HasIndex(e => e.Volume, "idx_16551_catalog_orderproduct_210ab9e7");

            entity.HasIndex(e => e.Quantity, "idx_16551_catalog_orderproduct_221d2a4b");

            entity.HasIndex(e => e.Price, "idx_16551_catalog_orderproduct_78a5eb43");

            entity.HasIndex(e => e.Weight, "idx_16551_catalog_orderproduct_7edabf99");

            entity.HasIndex(e => e.ProductId, "idx_16551_catalog_orderproduct_9bea82de");

            entity.HasIndex(e => e.OrderId, "idx_16551_catalog_orderproduct_order_id_036e46b5_fk_catalog_ord");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_orderproduct_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Order).WithMany(p => p.CatalogOrderproducts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_orderproduct_order_id_036e46b5_fk");

            entity.HasOne(d => d.Product).WithMany(p => p.CatalogOrderproducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_orderproduct_product_id_1b64b5b7_fk");
        });

        modelBuilder.Entity<CatalogPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16563_primary");

            entity.ToTable("catalog_payment", "nbshop");

            entity.HasIndex(e => e.Name, "idx_16563_catalog_payment_b068931c");

            entity.HasIndex(e => e.Handler, "idx_16563_catalog_payment_c1cbfe27");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_payment_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Handler)
                .HasMaxLength(255)
                .HasColumnName("handler");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CatalogProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16573_primary");

            entity.ToTable("catalog_product", "nbshop");

            entity.HasIndex(e => e.Alias, "idx_16573_alias").IsUnique();

            entity.HasIndex(e => e.Volume, "idx_16573_catalog_product_210ab9e7");

            entity.HasIndex(e => e.Quantity, "idx_16573_catalog_product_221d2a4b");

            entity.HasIndex(e => e.ManufacturerId, "idx_16573_catalog_product_4d136c4a");

            entity.HasIndex(e => e.Price, "idx_16573_catalog_product_78a5eb43");

            entity.HasIndex(e => e.Weight, "idx_16573_catalog_product_7edabf99");

            entity.HasIndex(e => e.Ean13, "idx_16573_catalog_product_a3b6a4d0");

            entity.HasIndex(e => e.Name, "idx_16573_catalog_product_b068931c");

            entity.HasIndex(e => e.OldPrice, "idx_16573_catalog_product_b61a4a9a");

            entity.HasIndex(e => e.Active, "idx_16573_catalog_product_c76a5e84");

            entity.HasIndex(e => e.Sort, "idx_16573_catalog_product_cadc8c8d");

            entity.HasIndex(e => e.CategoryId, "idx_16573_catalog_product_category_id_35bf920b_fk_catalog_categ");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_product_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Ean13)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("ean13");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.HasReject).HasColumnName("has_reject");
            entity.Property(e => e.Image)
                .HasMaxLength(1000)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("image");
            entity.Property(e => e.Introtext).HasColumnName("introtext");
            entity.Property(e => e.Manual)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("manual");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_keywords");
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(80)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_title");
            entity.Property(e => e.Multiplicity).HasColumnName("multiplicity");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.New).HasColumnName("new");
            entity.Property(e => e.OldPrice).HasColumnName("old_price");
            entity.Property(e => e.Popular).HasColumnName("popular");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SearchKeywords).HasColumnName("search_keywords");
            entity.Property(e => e.Sort).HasColumnName("sort");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Category).WithMany(p => p.CatalogProducts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_product_category_id_35bf920b_fk");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.CatalogProducts)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("catalog_product_manufacturer_id_a67ee459_fk");
        });

        modelBuilder.Entity<CatalogPromo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16600_primary");

            entity.ToTable("catalog_promo", "nbshop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_promo_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CatalogPromoProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16607_primary");

            entity.ToTable("catalog_promo_products", "nbshop");

            entity.HasIndex(e => e.ProductId, "idx_16607_catalog_promo_products_product_id_12e0e1fb_fk_catalog");

            entity.HasIndex(e => new { e.PromoId, e.ProductId }, "idx_16607_catalog_promo_products_promo_id_998ae650_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('catalog_promo_products_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.PromoId).HasColumnName("promo_id");
        });

        modelBuilder.Entity<CeleryTaskmetum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16615_primary");

            entity.ToTable("celery_taskmeta", "nbshop");

            entity.HasIndex(e => e.Hidden, "idx_16615_celery_taskmeta_662f707d");

            entity.HasIndex(e => e.TaskId, "idx_16615_task_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('celery_taskmeta_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.DateDone).HasColumnName("date_done");
            entity.Property(e => e.Hidden).HasColumnName("hidden");
            entity.Property(e => e.Meta).HasColumnName("meta");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Traceback).HasColumnName("traceback");
        });

        modelBuilder.Entity<CeleryTasksetmetum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16627_primary");

            entity.ToTable("celery_tasksetmeta", "nbshop");

            entity.HasIndex(e => e.Hidden, "idx_16627_celery_tasksetmeta_662f707d");

            entity.HasIndex(e => e.TasksetId, "idx_16627_taskset_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('celery_tasksetmeta_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.DateDone).HasColumnName("date_done");
            entity.Property(e => e.Hidden).HasColumnName("hidden");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.TasksetId)
                .HasMaxLength(255)
                .HasColumnName("taskset_id");
        });

        modelBuilder.Entity<ContentFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16639_primary");

            entity.ToTable("content_file", "nbshop");

            entity.HasIndex(e => e.FolderId, "idx_16639_content_file_a8a44dbb");

            entity.HasIndex(e => e.Name, "idx_16639_content_file_b068931c");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('content_file_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Attachment)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("attachment");
            entity.Property(e => e.FolderId).HasColumnName("folder_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("name");

            entity.HasOne(d => d.Folder).WithMany(p => p.ContentFiles)
                .HasForeignKey(d => d.FolderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("content_file_folder_id_63bdcdfd_fk");
        });

        modelBuilder.Entity<ContentFolder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16648_primary");

            entity.ToTable("content_folder", "nbshop");

            entity.HasIndex(e => e.Name, "idx_16648_content_folder_b068931c");

            entity.HasIndex(e => e.ParentId, "idx_16648_content_folder_parent_id_2654a18b_fk_content_folder_i");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('content_folder_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("content_folder_parent_id_2654a18b_fk");
        });

        modelBuilder.Entity<ContentNews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16655_primary");

            entity.ToTable("content_news", "nbshop");

            entity.HasIndex(e => e.Alias, "idx_16655_alias").IsUnique();

            entity.HasIndex(e => e.Name, "idx_16655_content_news_b068931c");

            entity.HasIndex(e => e.Active, "idx_16655_content_news_c76a5e84");

            entity.HasIndex(e => e.Sort, "idx_16655_content_news_cadc8c8d");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('content_news_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Image)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("image");
            entity.Property(e => e.Introtext).HasColumnName("introtext");
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_keywords");
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(80)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_title");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Sort).HasColumnName("sort");
        });

        modelBuilder.Entity<ContentPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16672_primary");

            entity.ToTable("content_page", "nbshop");

            entity.HasIndex(e => e.Alias, "idx_16672_alias").IsUnique();

            entity.HasIndex(e => e.Name, "idx_16672_content_page_b068931c");

            entity.HasIndex(e => e.Active, "idx_16672_content_page_c76a5e84");

            entity.HasIndex(e => e.Sort, "idx_16672_content_page_cadc8c8d");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('content_page_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Introtext).HasColumnName("introtext");
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_keywords");
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(80)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_title");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Sort).HasColumnName("sort");
        });

        modelBuilder.Entity<DjangoAdminLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16687_primary");

            entity.ToTable("django_admin_log", "nbshop");

            entity.HasIndex(e => e.ContentTypeId, "idx_16687_django_admin__content_type_id_c4bce8eb_fk_django_cont");

            entity.HasIndex(e => e.UserId, "idx_16687_django_admin_log_user_id_c564eba6_fk_auth_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('django_admin_log_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ActionFlag).HasColumnName("action_flag");
            entity.Property(e => e.ActionTime).HasColumnName("action_time");
            entity.Property(e => e.ChangeMessage).HasColumnName("change_message");
            entity.Property(e => e.ContentTypeId).HasColumnName("content_type_id");
            entity.Property(e => e.ObjectId).HasColumnName("object_id");
            entity.Property(e => e.ObjectRepr)
                .HasMaxLength(200)
                .HasColumnName("object_repr");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ContentType).WithMany(p => p.DjangoAdminLogs)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("django_admin__content_type_id_c4bce8eb_fk_django_content_type_i");

            entity.HasOne(d => d.User).WithMany(p => p.DjangoAdminLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("django_admin_log_user_id_c564eba6_fk_auth_user_id");
        });

        modelBuilder.Entity<DjangoContentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16700_primary");

            entity.ToTable("django_content_type", "nbshop");

            entity.HasIndex(e => new { e.AppLabel, e.Model }, "idx_16700_django_content_type_app_label_76bd3d3b_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('django_content_type_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.AppLabel)
                .HasMaxLength(100)
                .HasColumnName("app_label");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
        });

        modelBuilder.Entity<DjangoMigration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16708_primary");

            entity.ToTable("django_migrations", "nbshop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('django_migrations_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.App)
                .HasMaxLength(255)
                .HasColumnName("app");
            entity.Property(e => e.Applied).HasColumnName("applied");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DjangoSession>(entity =>
        {
            entity.HasKey(e => e.SessionKey).HasName("idx_16718_primary");

            entity.ToTable("django_session", "nbshop");

            entity.HasIndex(e => e.ExpireDate, "idx_16718_django_session_de54fa62");

            entity.Property(e => e.SessionKey)
                .HasMaxLength(40)
                .HasColumnName("session_key");
            entity.Property(e => e.ExpireDate).HasColumnName("expire_date");
            entity.Property(e => e.SessionData).HasColumnName("session_data");
        });

        modelBuilder.Entity<DjangoSite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16727_primary");

            entity.ToTable("django_site", "nbshop");

            entity.HasIndex(e => e.Domain, "idx_16727_django_site_domain_a2e37b91_uniq").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('django_site_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Domain)
                .HasMaxLength(100)
                .HasColumnName("domain");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DjceleryCrontabschedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16735_primary");

            entity.ToTable("djcelery_crontabschedule", "nbshop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djcelery_crontabschedule_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.DayOfMonth)
                .HasMaxLength(64)
                .HasColumnName("day_of_month");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(64)
                .HasColumnName("day_of_week");
            entity.Property(e => e.Hour)
                .HasMaxLength(64)
                .HasColumnName("hour");
            entity.Property(e => e.Minute)
                .HasMaxLength(64)
                .HasColumnName("minute");
            entity.Property(e => e.MonthOfYear)
                .HasMaxLength(64)
                .HasColumnName("month_of_year");
        });

        modelBuilder.Entity<DjceleryIntervalschedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16746_primary");

            entity.ToTable("djcelery_intervalschedule", "nbshop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djcelery_intervalschedule_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Every).HasColumnName("every");
            entity.Property(e => e.Period)
                .HasMaxLength(24)
                .HasColumnName("period");
        });

        modelBuilder.Entity<DjceleryPeriodictask>(entity =>
        {
            entity.HasKey(e => e.Ident).HasName("idx_16772_primary");

            entity.ToTable("djcelery_periodictasks", "nbshop");

            entity.Property(e => e.Ident)
                .ValueGeneratedNever()
                .HasColumnName("ident");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update");
        });

        modelBuilder.Entity<DjceleryPeriodictask1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16754_primary");

            entity.ToTable("djcelery_periodictask", "nbshop");

            entity.HasIndex(e => e.IntervalId, "idx_16754_djcelery_pe_interval_id_b426ab02_fk_djcelery_interval");

            entity.HasIndex(e => e.CrontabId, "idx_16754_djcelery_peri_crontab_id_75609bab_fk_djcelery_crontab");

            entity.HasIndex(e => e.Name, "idx_16754_name").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djcelery_periodictask_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Args).HasColumnName("args");
            entity.Property(e => e.CrontabId).HasColumnName("crontab_id");
            entity.Property(e => e.DateChanged).HasColumnName("date_changed");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.Exchange)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("exchange");
            entity.Property(e => e.Expires).HasColumnName("expires");
            entity.Property(e => e.IntervalId).HasColumnName("interval_id");
            entity.Property(e => e.Kwargs).HasColumnName("kwargs");
            entity.Property(e => e.LastRunAt).HasColumnName("last_run_at");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Queue)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("queue");
            entity.Property(e => e.RoutingKey)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("routing_key");
            entity.Property(e => e.Task)
                .HasMaxLength(200)
                .HasColumnName("task");
            entity.Property(e => e.TotalRunCount).HasColumnName("total_run_count");

            entity.HasOne(d => d.Crontab).WithMany(p => p.DjceleryPeriodictask1s)
                .HasForeignKey(d => d.CrontabId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("djcelery_peri_crontab_id_75609bab_fk_djcelery_crontabschedule_i");

            entity.HasOne(d => d.Interval).WithMany(p => p.DjceleryPeriodictask1s)
                .HasForeignKey(d => d.IntervalId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("djcelery_pe_interval_id_b426ab02_fk_djcelery_intervalschedule_i");
        });

        modelBuilder.Entity<DjceleryTaskstate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16778_primary");

            entity.ToTable("djcelery_taskstate", "nbshop");

            entity.HasIndex(e => e.Hidden, "idx_16778_djcelery_taskstate_662f707d");

            entity.HasIndex(e => e.Tstamp, "idx_16778_djcelery_taskstate_863bb2ee");

            entity.HasIndex(e => e.State, "idx_16778_djcelery_taskstate_9ed39e2e");

            entity.HasIndex(e => e.Name, "idx_16778_djcelery_taskstate_b068931c");

            entity.HasIndex(e => e.WorkerId, "idx_16778_djcelery_taskstate_ce77e6ef");

            entity.HasIndex(e => e.TaskId, "idx_16778_task_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djcelery_taskstate_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Args).HasColumnName("args");
            entity.Property(e => e.Eta).HasColumnName("eta");
            entity.Property(e => e.Expires).HasColumnName("expires");
            entity.Property(e => e.Hidden).HasColumnName("hidden");
            entity.Property(e => e.Kwargs).HasColumnName("kwargs");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("name");
            entity.Property(e => e.Result).HasColumnName("result");
            entity.Property(e => e.Retries).HasColumnName("retries");
            entity.Property(e => e.Runtime).HasColumnName("runtime");
            entity.Property(e => e.State)
                .HasMaxLength(64)
                .HasColumnName("state");
            entity.Property(e => e.TaskId)
                .HasMaxLength(36)
                .HasColumnName("task_id");
            entity.Property(e => e.Traceback).HasColumnName("traceback");
            entity.Property(e => e.Tstamp).HasColumnName("tstamp");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");

            entity.HasOne(d => d.Worker).WithMany(p => p.DjceleryTaskstates)
                .HasForeignKey(d => d.WorkerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("djcelery_taskstate_worker_id_f7f57a05_fk_djcelery_workerstate_i");
        });

        modelBuilder.Entity<DjceleryWorkerstate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16792_primary");

            entity.ToTable("djcelery_workerstate", "nbshop");

            entity.HasIndex(e => e.LastHeartbeat, "idx_16792_djcelery_workerstate_f129901a");

            entity.HasIndex(e => e.Hostname, "idx_16792_hostname").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djcelery_workerstate_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Hostname)
                .HasMaxLength(255)
                .HasColumnName("hostname");
            entity.Property(e => e.LastHeartbeat).HasColumnName("last_heartbeat");
        });

        modelBuilder.Entity<DjkombuMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16799_primary");

            entity.ToTable("djkombu_message", "nbshop");

            entity.HasIndex(e => e.Visible, "idx_16799_djkombu_message_46cf0e59");

            entity.HasIndex(e => e.QueueId, "idx_16799_djkombu_message_75249aa1");

            entity.HasIndex(e => e.SentAt, "idx_16799_djkombu_message_df2f2974");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djkombu_message_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.QueueId).HasColumnName("queue_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Visible).HasColumnName("visible");

            entity.HasOne(d => d.Queue).WithMany(p => p.DjkombuMessages)
                .HasForeignKey(d => d.QueueId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("djkombu_message_queue_id_38d205a7_fk_djkombu_queue_id");
        });

        modelBuilder.Entity<DjkombuQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16810_primary");

            entity.ToTable("djkombu_queue", "nbshop");

            entity.HasIndex(e => e.Name, "idx_16810_name").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('djkombu_queue_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ReferencesCity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16817_primary");

            entity.ToTable("references_city", "nbshop");

            entity.HasIndex(e => e.NativeDelivery, "idx_16817_references_city_38ab6aea");

            entity.HasIndex(e => e.Active, "idx_16817_references_city_c76a5e84");

            entity.HasIndex(e => e.Name, "idx_16817_references_city_name_c7da022b_uniq");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('references_city_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.DelLineCode)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("del_line_code");
            entity.Property(e => e.KladrCode)
                .HasMaxLength(255)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("kladr_code");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NativeDelivery).HasColumnName("native_delivery");
        });

        modelBuilder.Entity<SearchSearchlogentry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16830_primary");

            entity.ToTable("search_searchlogentry", "nbshop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('search_searchlogentry_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Text).HasColumnName("text");
        });

        modelBuilder.Entity<TempOldProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idx_16840_primary");

            entity.ToTable("temp_old_products", "nbshop");

            entity.HasIndex(e => e.Alias, "idx_16840_alias").IsUnique();

            entity.HasIndex(e => e.Volume, "idx_16840_catalog_product_210ab9e7");

            entity.HasIndex(e => e.Quantity, "idx_16840_catalog_product_221d2a4b");

            entity.HasIndex(e => e.ManufacturerId, "idx_16840_catalog_product_4d136c4a");

            entity.HasIndex(e => e.Price, "idx_16840_catalog_product_78a5eb43");

            entity.HasIndex(e => e.Weight, "idx_16840_catalog_product_7edabf99");

            entity.HasIndex(e => e.Ean13, "idx_16840_catalog_product_a3b6a4d0");

            entity.HasIndex(e => e.Name, "idx_16840_catalog_product_b068931c");

            entity.HasIndex(e => e.OldPrice, "idx_16840_catalog_product_b61a4a9a");

            entity.HasIndex(e => e.Active, "idx_16840_catalog_product_c76a5e84");

            entity.HasIndex(e => e.Sort, "idx_16840_catalog_product_cadc8c8d");

            entity.HasIndex(e => e.CategoryId, "idx_16840_catalog_product_category_id_35bf920b_fk_catalog_categ");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('temp_old_products_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Ean13)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("ean13");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.HasReject).HasColumnName("has_reject");
            entity.Property(e => e.Image)
                .HasMaxLength(1000)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("image");
            entity.Property(e => e.Introtext).HasColumnName("introtext");
            entity.Property(e => e.Manual)
                .HasMaxLength(100)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("manual");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.MetaDescription)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_description");
            entity.Property(e => e.MetaKeywords)
                .HasMaxLength(200)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_keywords");
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(80)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("meta_title");
            entity.Property(e => e.Multiplicity).HasColumnName("multiplicity");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.New).HasColumnName("new");
            entity.Property(e => e.OldPrice).HasColumnName("old_price");
            entity.Property(e => e.Popular).HasColumnName("popular");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SearchKeywords).HasColumnName("search_keywords");
            entity.Property(e => e.Sort).HasColumnName("sort");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.Weight).HasColumnName("weight");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
