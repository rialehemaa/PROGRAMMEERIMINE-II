using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{
    public class ToodedContext : DbContext
    {
        public DbSet<Toode> Toodetabel { get; set; }
        public DbSet<Kategooria> Kategooriatabel { get; set; }
        public DbSet<Kasutaja> Kasutajad { get; set; }
        public DbSet<Klient> Kliendid { get; set; }
        public DbSet<Muuk> Muugid { get; set; }
        public DbSet<Pakkumine> Pakkumised { get; set; }
        public DbSet<MuugiRida> MuugiRead { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=Tooded_DB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
            modelBuilder.Entity<Toode>(e =>
            {
                e.ToTable("Toodetabel");
                e.Property(t => t.Kogus).HasPrecision(18, 3);
                e.Property(t => t.TukiKaal).HasPrecision(18, 3);
                e.Property(t => t.Uhik).HasMaxLength(10).HasDefaultValue("tk");
            });

            modelBuilder.Entity<Kasutaja>(e =>
            {
                e.ToTable("Kasutajad");
                e.HasIndex(k => k.Kasutajanimi).IsUnique();   // sama nime ei saa kaks korda
                e.Property(k => k.Kasutajanimi).HasMaxLength(50).IsRequired();
                e.Property(k => k.Roll).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Klient>(e =>
            {
                e.ToTable("Kliendid");
                e.HasIndex(k => k.Kliendikaart).IsUnique();   // iga kaart on unikaalne
                e.Property(k => k.Nimi).HasMaxLength(100).IsRequired();
                e.Property(k => k.Email).HasMaxLength(100);
                e.Property(k => k.Telefon).HasMaxLength(30);
                e.Property(k => k.Kliendikaart).HasMaxLength(20).IsRequired();
                e.Property(k => k.Boonus).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Pakkumine>(e =>
            {
                e.ToTable("Pakkumised");
                e.Property(p => p.Tuup).HasMaxLength(60).IsRequired();
                e.Property(p => p.KomplektHind).HasPrecision(18, 2);
                e.HasOne(p => p.Toode).WithMany().HasForeignKey(p => p.ToodeId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Toode2).WithMany().HasForeignKey(p => p.Toode2Id).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<MuugiRida>(e =>
            {
                e.ToTable("MuugiRead");
                e.Property(r => r.Toode).HasMaxLength(100);
                e.Property(r => r.Kogus).HasPrecision(18, 3);
                e.Property(r => r.Summa).HasPrecision(18, 2);
                e.HasOne(r => r.Muuk).WithMany(m => m.Read).HasForeignKey(r => r.MuukId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Muuk>(e =>
            {
                e.ToTable("Muugid");
                e.Property(m => m.Kokku).HasPrecision(18, 2);
                e.Property(m => m.KasutatudBoonus).HasPrecision(18, 2);
                e.Property(m => m.Makstud).HasPrecision(18, 2);
                e.Property(m => m.SynnipaevaSoodus).HasPrecision(18, 2);
                e.Property(m => m.Maksetuup).HasMaxLength(50);
                e.Property(m => m.SaadudBoonus).HasPrecision(18, 2);
                e.Property(m => m.Arve).HasMaxLength(100);
                // kui müüja või klient kustutatakse, jääb müük alles
                e.HasOne(m => m.Kasutaja).WithMany().HasForeignKey(m => m.KasutajaId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(m => m.Klient).WithMany().HasForeignKey(m => m.KlientId).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}