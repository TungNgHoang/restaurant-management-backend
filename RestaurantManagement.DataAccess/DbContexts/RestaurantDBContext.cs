using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.DataAccess.Models;

namespace RestaurantManagement.DataAccess.DbContexts;

public partial class RestaurantDBContext : DbContext
{
    public RestaurantDBContext()
    {
    }

    public RestaurantDBContext(DbContextOptions<RestaurantDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAttendance> TblAttendances { get; set; }

    public virtual DbSet<TblBlackListToken> TblBlackListTokens { get; set; }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblMenu> TblMenus { get; set; }

    public virtual DbSet<TblNotification> TblNotifications { get; set; }

    public virtual DbSet<TblOrderDetail> TblOrderDetails { get; set; }

    public virtual DbSet<TblOrderInfo> TblOrderInfos { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblPayroll> TblPayrolls { get; set; }

    public virtual DbSet<TblPromotion> TblPromotions { get; set; }

    public virtual DbSet<TblReservation> TblReservations { get; set; }

    public virtual DbSet<TblShift> TblShifts { get; set; }

    public virtual DbSet<TblShiftAssignment> TblShiftAssignments { get; set; }

    public virtual DbSet<TblStaff> TblStaffs { get; set; }

    public virtual DbSet<TblTableInfo> TblTableInfos { get; set; }

    public virtual DbSet<TblUserAccount> TblUserAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=cmcsv.ric.vn,22956;Initial Catalog=TKTKPM_NHOM5;Persist Security Info=True;User ID=cmcsvtkpm;Password=cMc!@#$2025;Trust Server Certificate=True;encrypt=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__tblAtten__8B69261CCDCF5379");

            entity.ToTable("tblAttendance");

            entity.Property(e => e.AttendanceId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckIn).HasColumnType("datetime");
            entity.Property(e => e.CheckOut).HasColumnType("datetime");
            entity.Property(e => e.StaId).HasColumnName("StaID");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<TblBlackListToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblBlack__3214EC07228DA9ED");

            entity.ToTable("tblBlackListToken");

            entity.HasIndex(e => e.ExpiryDate, "IX_tblBlackListToken_Expiry");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.RevokedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UacId).HasColumnName("UacID");

            entity.HasOne(d => d.Uac).WithMany(p => p.TblBlackListTokens)
                .HasForeignKey(d => d.UacId)
                .HasConstraintName("FK_BlackListToken_UserAccount");
        });

        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CusId);

            entity.ToTable("tblCustomer");

            entity.Property(e => e.CusId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CusID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CusContact).HasMaxLength(50);
            entity.Property(e => e.CusEmail).HasMaxLength(255);
            entity.Property(e => e.CusName).HasMaxLength(255);
            entity.Property(e => e.CusTier).HasMaxLength(20);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblMenu>(entity =>
        {
            entity.HasKey(e => e.MnuId);

            entity.ToTable("tblMenu");

            entity.Property(e => e.MnuId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("MnuID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.MnuDescription).HasMaxLength(255);
            entity.Property(e => e.MnuImage).HasMaxLength(255);
            entity.Property(e => e.MnuName).HasMaxLength(255);
            entity.Property(e => e.MnuPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MnuStatus).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblNotification>(entity =>
        {
            entity.HasKey(e => e.NotiId);

            entity.ToTable("tblNotification");

            entity.HasIndex(e => new { e.IsRead, e.CreatedAt }, "IX_tblNotification_IsRead").IsDescending(false, true);

            entity.Property(e => e.NotiId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("NotiID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.ResId).HasColumnName("ResID");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Res).WithMany(p => p.TblNotifications)
                .HasForeignKey(d => d.ResId)
                .HasConstraintName("FK_tblNotification_tblReservation");
        });

        modelBuilder.Entity<TblOrderDetail>(entity =>
        {
            entity.HasKey(e => e.OdtId);

            entity.ToTable("tblOrderDetail");

            entity.Property(e => e.OdtId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OdtID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.MnuId).HasColumnName("MnuID");
            entity.Property(e => e.OdtStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Normal");
            entity.Property(e => e.OrdId).HasColumnName("OrdID");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Mnu).WithMany(p => p.TblOrderDetails)
                .HasForeignKey(d => d.MnuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblOrderDetail_tblMenu");

            entity.HasOne(d => d.Ord).WithMany(p => p.TblOrderDetails)
                .HasForeignKey(d => d.OrdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblOrderDetail_tblOrderInfo");
        });

        modelBuilder.Entity<TblOrderInfo>(entity =>
        {
            entity.HasKey(e => e.OrdId);

            entity.ToTable("tblOrderInfo");

            entity.HasIndex(e => e.OrdStatus, "IX_tblOrderInfo_Status");

            entity.Property(e => e.OrdId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("OrdID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CusId).HasColumnName("CusID");
            entity.Property(e => e.OrdStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.ResId).HasColumnName("ResID");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TbiId).HasColumnName("TbiID");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Cus).WithMany(p => p.TblOrderInfos)
                .HasForeignKey(d => d.CusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblOrderInfo_tblCustomer");

            entity.HasOne(d => d.Res).WithMany(p => p.TblOrderInfos)
                .HasForeignKey(d => d.ResId)
                .HasConstraintName("FK_tblOrderInfo_tblReservation");

            entity.HasOne(d => d.Tbi).WithMany(p => p.TblOrderInfos)
                .HasForeignKey(d => d.TbiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblOrderInfo_tblTableInfo");

            entity.HasMany(d => d.Pros).WithMany(p => p.Ords)
                .UsingEntity<Dictionary<string, object>>(
                    "TblOrderPromotion",
                    r => r.HasOne<TblPromotion>().WithMany()
                        .HasForeignKey("ProId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_tblOrderPromotion_tblPromotion"),
                    l => l.HasOne<TblOrderInfo>().WithMany()
                        .HasForeignKey("OrdId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_tblOrderPromotion_tblOrderInfo"),
                    j =>
                    {
                        j.HasKey("OrdId", "ProId").HasName("PK__tblOrder__7182AA4966BB4BB2");
                        j.ToTable("tblOrderPromotion");
                        j.IndexerProperty<Guid>("OrdId").HasColumnName("OrdID");
                        j.IndexerProperty<Guid>("ProId").HasColumnName("ProID");
                    });
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PayId);

            entity.ToTable("tblPayment");

            entity.Property(e => e.PayId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("PayID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CusId).HasColumnName("CusID");
            entity.Property(e => e.OrdId).HasColumnName("OrdID");
            entity.Property(e => e.PayMethod).HasMaxLength(50);
            entity.Property(e => e.PayStatus).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Cus).WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.CusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblPayment_tblCustomer");

            entity.HasOne(d => d.Ord).WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.OrdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblPayment_tblOrderInfo");
        });

        modelBuilder.Entity<TblPayroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__tblPayro__99DFC6720CABE126");

            entity.ToTable("tblPayroll");

            entity.Property(e => e.PayrollId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StaId).HasColumnName("StaID");
            entity.Property(e => e.TotalHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalSalary).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TblPromotion>(entity =>
        {
            entity.HasKey(e => e.ProId);

            entity.ToTable("tblPromotion");

            entity.HasIndex(e => e.ProCode, "UX_tblPromotion_ProCode").IsUnique();

            entity.Property(e => e.ProId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProID");
            entity.Property(e => e.ConditionVal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountVal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProCode).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblReservation>(entity =>
        {
            entity.HasKey(e => e.ResId);

            entity.ToTable("tblReservation");

            entity.HasIndex(e => e.ResStatus, "IX_tblReservation_Status");

            entity.Property(e => e.ResId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ResID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CusId).HasColumnName("CusID");
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.ResDate).HasColumnType("datetime");
            entity.Property(e => e.ResEndTime).HasColumnType("datetime");
            entity.Property(e => e.ResStatus).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TbiId).HasColumnName("TbiID");
            entity.Property(e => e.TempCustomerMail).HasMaxLength(255);
            entity.Property(e => e.TempCustomerName).HasMaxLength(255);
            entity.Property(e => e.TempCustomerPhone).HasMaxLength(50);

            entity.HasOne(d => d.Cus).WithMany(p => p.TblReservations)
                .HasForeignKey(d => d.CusId)
                .HasConstraintName("FK_tblReservation_tblCustomer");

            entity.HasOne(d => d.Tbi).WithMany(p => p.TblReservations)
                .HasForeignKey(d => d.TbiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblReservation_tblTableInfo");
        });

        modelBuilder.Entity<TblShift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__tblShift__C0A838817BBE8795");

            entity.ToTable("tblShift");

            entity.Property(e => e.ShiftId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.ShiftName).HasMaxLength(50);
        });

        modelBuilder.Entity<TblShiftAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__tblShift__32499E77A4E18497");

            entity.ToTable("tblShiftAssignment");

            entity.Property(e => e.AssignmentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StaId).HasColumnName("StaID");
        });

        modelBuilder.Entity<TblStaff>(entity =>
        {
            entity.HasKey(e => e.StaId);

            entity.ToTable("tblStaff");

            entity.Property(e => e.StaId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("StaID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.StaBaseSalary).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.StaName).HasMaxLength(255);
            entity.Property(e => e.StaPhone).HasMaxLength(20);
            entity.Property(e => e.StaRole).HasMaxLength(50);
            entity.Property(e => e.UacId).HasColumnName("UacID");

            entity.HasOne(d => d.Uac).WithMany(p => p.TblStaffs)
                .HasForeignKey(d => d.UacId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblStaff_tblUserAccount");
        });

        modelBuilder.Entity<TblTableInfo>(entity =>
        {
            entity.HasKey(e => e.TbiId);

            entity.ToTable("tblTableInfo");

            entity.Property(e => e.TbiId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("TbiID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TbiCapacity).HasDefaultValue(4);
            entity.Property(e => e.TbiQrcode)
                .HasMaxLength(255)
                .HasColumnName("TbiQRCode");
            entity.Property(e => e.TbiStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<TblUserAccount>(entity =>
        {
            entity.HasKey(e => e.UacId);

            entity.ToTable("tblUserAccount");

            entity.HasIndex(e => e.UacEmail, "UQ__tblUserA__21846F66D509F4F4").IsUnique();

            entity.Property(e => e.UacId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UacID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UacEmail).HasMaxLength(255);
            entity.Property(e => e.UacPassword).HasMaxLength(255);
            entity.Property(e => e.UacRole).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
