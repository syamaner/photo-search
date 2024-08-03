﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PhotoSearch.Data;
using PhotoSearch.Data.GeoJson;

#nullable disable

namespace PhotoSearch.Data.Migrations
{
    [DbContext(typeof(PhotoSearchContext))]
    partial class PhotoSearchContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PhotoSearch.Data.Models.Photo", b =>
                {
                    b.Property<string>("RelativePath")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<DateTime?>("CaptureDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ExactPath")
                        .IsRequired()
                        .HasMaxLength(600)
                        .HasColumnType("character varying(600)");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Height")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ImportedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double?>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<FeatureCollection>("LocationInformation")
                        .HasColumnType("jsonb");

                    b.Property<double?>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("PublicUrl")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<long>("SizeKb")
                        .HasColumnType("bigint");

                    b.Property<int>("Width")
                        .HasColumnType("integer");

                    b.HasKey("RelativePath");

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("PhotoSearch.Data.Models.Photo", b =>
                {
                    b.OwnsOne("PhotoSearch.Data.Models.Thumbnail", "Thumbnails", b1 =>
                        {
                            b1.Property<string>("PhotoRelativePath")
                                .HasColumnType("character varying(250)");

                            b1.Property<string>("LargeThumbnailUrl")
                                .HasColumnType("text");

                            b1.Property<string>("MediumhumbnailUrl")
                                .HasColumnType("text");

                            b1.Property<string>("SmallThumbnailUrl")
                                .HasColumnType("text");

                            b1.HasKey("PhotoRelativePath");

                            b1.ToTable("Photos");

                            b1.ToJson("Thumbnails");

                            b1.WithOwner()
                                .HasForeignKey("PhotoRelativePath");
                        });

                    b.OwnsOne("System.Collections.Generic.List<PhotoSearch.Data.Models.ExifData>", "Metadata", b1 =>
                        {
                            b1.Property<string>("PhotoRelativePath")
                                .HasColumnType("character varying(250)");

                            b1.Property<int>("Capacity")
                                .HasColumnType("integer");

                            b1.HasKey("PhotoRelativePath");

                            b1.ToTable("Photos");

                            b1.ToJson("Metadata");

                            b1.WithOwner()
                                .HasForeignKey("PhotoRelativePath");
                        });

                    b.OwnsOne("System.Collections.Generic.List<PhotoSearch.Data.Models.PhotoSummary>", "PhotoSummaries", b1 =>
                        {
                            b1.Property<string>("PhotoRelativePath")
                                .HasColumnType("character varying(250)");

                            b1.Property<int>("Capacity")
                                .HasColumnType("integer");

                            b1.HasKey("PhotoRelativePath");

                            b1.ToTable("Photos");

                            b1.ToJson("PhotoSummaries");

                            b1.WithOwner()
                                .HasForeignKey("PhotoRelativePath");
                        });

                    b.Navigation("Metadata");

                    b.Navigation("PhotoSummaries");

                    b.Navigation("Thumbnails");
                });
#pragma warning restore 612, 618
        }
    }
}
