﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Outbox.Infrastructure;

#nullable disable

namespace Outbox.Infrastructure.Migrations
{
    [DbContext(typeof(OutboxContext))]
    [Migration("20221221200338_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.1");

            modelBuilder.Entity("Outbox.Core.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DeliveryStatus")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PartitionKey")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PublishedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReceivedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("RetryCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("Outbox.Core.Partition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("PartitionKey")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PartitionKey")
                        .IsUnique();

                    b.ToTable("Partition");
                });
#pragma warning restore 612, 618
        }
    }
}