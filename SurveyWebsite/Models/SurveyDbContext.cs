using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SurveyWebsite.Models;

public partial class SurveyDbContext : DbContext
{
    public SurveyDbContext()
    {
    }

    public SurveyDbContext(DbContextOptions<SurveyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Participation> Participations { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Survey> Surveys { get; set; }

    public virtual DbSet<SurveyAllowedUser> SurveyAllowedUsers { get; set; }

    public virtual DbSet<SurveySetting> SurveySettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=SurveyDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D4825024F7CEE4D9");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.ParticipationId).HasColumnName("ParticipationID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Option).WithMany(p => p.Answers)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK_Answer_Option");

            entity.HasOne(d => d.Participation).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ParticipationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Participation");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E328F8277B6");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Option__92C7A1DFB05C6B22");

            entity.ToTable("Option");

            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.OptionText).HasMaxLength(200);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Option_Question");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.HasKey(e => e.ParticipationId).HasName("PK__Particip__4EA27080040D0DF2");

            entity.ToTable("Participation");

            entity.Property(e => e.ParticipationId).HasColumnName("ParticipationID");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Survey).WithMany(p => p.Participations)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participation_Survey");

            entity.HasOne(d => d.User).WithMany(p => p.Participations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Participation_User");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8C6D374E2C");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuestionType).HasMaxLength(50);
            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");

            entity.HasOne(d => d.Survey).WithMany(p => p.Questions)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Question_Survey");
        });

        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.SurveyId).HasName("PK__Survey__A5481F9D3946880C");

            entity.ToTable("Survey");

            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatorUserId).HasColumnName("CreatorUserID");
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CreatorUser).WithMany(p => p.Surveys)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Survey_User");
        });

        modelBuilder.Entity<SurveyAllowedUser>(entity =>
        {
            entity.HasKey(e => new { e.SurveyId, e.UserId }).HasName("PK__SurveyAl__743093579D94C614");

            entity.ToTable("SurveyAllowedUser");

            entity.Property(e => e.SurveyId).HasColumnName("SurveyID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyAllowedUsers)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SurveyAllowedUser_Survey");

            entity.HasOne(d => d.User).WithMany(p => p.SurveyAllowedUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SurveyAllowedUser_User");
        });

        modelBuilder.Entity<SurveySetting>(entity =>
        {
            entity.HasKey(e => e.SurveyId).HasName("PK__SurveySe__A5481F9D08555409");

            entity.Property(e => e.SurveyId)
                .ValueGeneratedNever()
                .HasColumnName("SurveyID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Survey).WithOne(p => p.SurveySetting)
                .HasForeignKey<SurveySetting>(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SurveySettings_Survey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC9A2679F4");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E40BEA3A5B").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
