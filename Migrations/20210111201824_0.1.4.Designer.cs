﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OSnack.API.Database;

namespace OSnack.API.Migrations
{
    [DbContext(typeof(OSnackDbContext))]
    [Migration("20210111201824_0.1.4")]
    partial class _014
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("OSnack.API.Database.Models.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("FirstLine")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Instructions")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Postcode")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.Property<string>("SecondLine")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<int>("UserId1")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId1");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.AppLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("JsonObject")
                        .HasColumnType("nvarchar(MAX)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("TimeStamp")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AppLogs");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("ImagePath")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("OriginalImagePath")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Rate")
                        .HasColumnType("int");

                    b.Property<string>("Reply")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("UserId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Communication", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nvarchar(36)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("FullName")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("OrderId")
                        .HasColumnType("nvarchar(7)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique()
                        .HasFilter("[OrderId] IS NOT NULL");

                    b.ToTable("Communications");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Coupon", b =>
                {
                    b.Property<string>("Code")
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.Property<decimal>("DiscountAmount")
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("ExpiryDate")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("MaxUseQuantity")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<decimal?>("MinimumOrderPrice")
                        .IsRequired()
                        .HasColumnType("decimal(7,2)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Code");

                    b.ToTable("Coupons");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.DeliveryOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<bool>("IsPremitive")
                        .HasColumnType("bit");

                    b.Property<decimal>("MinimumOrderTotal")
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(7,2)");

                    b.HasKey("Id");

                    b.ToTable("DeliveryOption");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.EmailTemplate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("DesignPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HtmlPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("TemplateType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EmailTemplates");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Body")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("CommunicationId")
                        .IsRequired()
                        .HasColumnType("nvarchar(36)");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsCustomer")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("CommunicationId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Newsletter", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nvarchar(36)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.ToTable("Newsletters");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.NutritionalInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal?>("Carbohydrate")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("CarbohydrateSugar")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("EnergyKJ")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("EnergyKcal")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("Fat")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("Fibre")
                        .HasColumnType("decimal(5,2)");

                    b.Property<int>("PerGram")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<decimal?>("Protein")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("Salt")
                        .HasColumnType("decimal(5,2)");

                    b.Property<decimal?>("SaturateFat")
                        .HasColumnType("decimal(5,2)");

                    b.HasKey("Id");

                    b.HasIndex("ProductId")
                        .IsUnique();

                    b.ToTable("NutritionalInfos");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(7)
                        .HasColumnType("nvarchar(7)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Code")
                        .HasColumnType("nvarchar(25)");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("DeliveryOptionId")
                        .HasColumnType("int");

                    b.Property<decimal>("DeliveryPrice")
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("FirstLine")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Postcode")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.Property<string>("SecondLine")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<decimal>("ShippingPrice")
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("ShippingReference")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalDiscount")
                        .HasColumnType("decimal(7,2)");

                    b.Property<decimal>("TotalItemPrice")
                        .HasColumnType("decimal(7,2)");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(7,2)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Code");

                    b.HasIndex("DeliveryOptionId");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.OrderItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("ImagePath")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(7)");

                    b.Property<decimal?>("Price")
                        .IsRequired()
                        .HasColumnType("decimal(7,2)");

                    b.Property<string>("ProductCategoryName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int?>("UnitQuantity")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("UnitType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Message")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(7)");

                    b.Property<string>("PaymentProvider")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("RefundAmount")
                        .HasColumnType("decimal(7,2)");

                    b.Property<DateTime?>("RefundDateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("ImagePath")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("OriginalImagePath")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal?>("Price")
                        .IsRequired()
                        .HasColumnType("decimal(7,2)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<int>("StockQuantity")
                        .HasColumnType("int");

                    b.Property<int?>("UnitQuantity")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("UnitType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.RegistrationMethod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("ExternalLinkedId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RegisteredDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasFilter("[UserId] IS NOT NULL");

                    b.ToTable("RegistrationMethod");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("AccessClaim")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Token", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("ExpiaryDateTime")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(MAX)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("RoleId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("OSnack.API.Extras.ClassOverrides.OSnackAccessClaim<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("AccessClaims");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Address", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.User", "User")
                        .WithMany("Addresses")
                        .HasForeignKey("UserId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.AppLog", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("User");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Comment", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Product", "Product")
                        .WithMany("Comments")
                        .HasForeignKey("ProductId");

                    b.HasOne("OSnack.API.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Product");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Communication", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Order", "Order")
                        .WithOne("Dispute")
                        .HasForeignKey("OSnack.API.Database.Models.Communication", "OrderId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Order");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Message", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Communication", "Communication")
                        .WithMany("Messages")
                        .HasForeignKey("CommunicationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Communication");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.NutritionalInfo", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Product", "Product")
                        .WithOne("NutritionalInfo")
                        .HasForeignKey("OSnack.API.Database.Models.NutritionalInfo", "ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Order", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Coupon", "Coupon")
                        .WithMany("Orders")
                        .HasForeignKey("Code")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("OSnack.API.Database.Models.DeliveryOption", "DeliveryOption")
                        .WithMany("Orders")
                        .HasForeignKey("DeliveryOptionId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("OSnack.API.Database.Models.User", "User")
                        .WithMany("Orders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Coupon");

                    b.Navigation("DeliveryOption");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.OrderItem", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OSnack.API.Database.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Payment", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Order", "Order")
                        .WithOne("Payment")
                        .HasForeignKey("OSnack.API.Database.Models.Payment", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Product", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.RegistrationMethod", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.User", "User")
                        .WithOne("RegistrationMethod")
                        .HasForeignKey("OSnack.API.Database.Models.RegistrationMethod", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("User");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Token", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("User");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.User", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("OSnack.API.Extras.ClassOverrides.OSnackAccessClaim<int>", b =>
                {
                    b.HasOne("OSnack.API.Database.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Communication", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Coupon", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.DeliveryOption", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Order", b =>
                {
                    b.Navigation("Dispute");

                    b.Navigation("OrderItems");

                    b.Navigation("Payment")
                        .IsRequired();
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Product", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("NutritionalInfo");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("OSnack.API.Database.Models.User", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("Orders");

                    b.Navigation("RegistrationMethod")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
