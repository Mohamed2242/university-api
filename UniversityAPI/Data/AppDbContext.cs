using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniversityAPI.Core.Entities;
using UniversityAPI.Core.Models;

namespace UniversityAPI.Data;

public partial class AppDbContext : IdentityDbContext<ApplicationUser>
{    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

	//public DbSet<ApplicationUser> ApplicationUsers { get; set; }

	public DbSet<Student> Students { get; set; }

	public DbSet<Doctor> Doctors { get; set; }

	public DbSet<Assistant> Assistants { get; set; }

	public DbSet<Employee> Employees { get; set; }

	public DbSet<Course> Courses { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<StudentDegree> StudentDegrees { get; set; }

	public DbSet<Petition> Petitions { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Student and Course many-to-many relationship through StudentDegree
		modelBuilder.Entity<StudentDegree>()
			.HasKey(sd => new { sd.StudentId, sd.CourseId });

		modelBuilder.Entity<StudentDegree>()
			.HasOne(sd => sd.Student)
			.WithMany(s => s.StudentDegrees)
			.HasForeignKey(sd => sd.StudentId);

		modelBuilder.Entity<StudentDegree>()
			.HasOne(sd => sd.Course)
			.WithMany(c => c.StudentDegrees)
			.HasForeignKey(sd => sd.CourseId);

		// Doctor and Course many-to-many relationship
		modelBuilder.Entity<Course>()
			.HasMany(c => c.Doctors)
			.WithMany(d => d.Courses)
			.UsingEntity(j => j.ToTable("DoctorCourses"));

		// Assistant and Course many-to-many relationship
		modelBuilder.Entity<Course>()
			.HasMany(c => c.Assistants)
			.WithMany(a => a.Courses)
			.UsingEntity(j => j.ToTable("AssistantCourses"));

		// Department and Course many-to-many relationship
		modelBuilder.Entity<Course>()
			.HasMany(c => c.Departments)
			.WithMany(d => d.Courses)
			.UsingEntity(j => j.ToTable("DepartmentCourses"));

		// Department primary key
		modelBuilder.Entity<Department>()
			.HasKey(d => d.Id);

		// Course primary key
		modelBuilder.Entity<Course>()
			.HasKey(c => c.Id);

		modelBuilder.Entity<Petition>()
			.HasNoKey();

		// Ensure AppUser tables map correctly (Inheritance hierarchy)
		modelBuilder.Entity<Student>()
			.ToTable("Students");

		modelBuilder.Entity<Doctor>()
			.ToTable("Doctors");

		modelBuilder.Entity<Assistant>()
			.ToTable("Assistants");

		modelBuilder.Entity<Employee>()
			.ToTable("Employees");

		modelBuilder.Entity<Course>()
			.ToTable("Courses");
	}
}
